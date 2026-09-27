using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CheckoutService _checkout;
        private readonly GuestCartService _guestCart;
        private readonly IEmailSender _emailSender;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            AppDbContext db,
            CheckoutService checkout,
            GuestCartService guestCart,
            IEmailSender emailSender,
            EmailSettings emailSettings,
            ILogger<OrdersController> logger)
        {
            _db = db;
            _checkout = checkout;
            _guestCart = guestCart;
            _emailSender = emailSender;
            _emailSettings = emailSettings;
            _logger = logger;
        }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        private string SiteUrl =>
            $"{Request.Scheme}://{Request.Host}";

        private string Logo => EmailTemplates.GetLogoUrl(SiteUrl);

        private List<CartItem> GetCheckoutItems()
        {
            if (UserId == null)
            {
                var guest = _guestCart.Get(HttpContext.Session);
                var productIds = guest.Select(g => g.ProductId).Distinct().ToList();
                var products = _db.Products.Where(p => productIds.Contains(p.Id)).ToList();
                var items = new List<CartItem>();
                for (int i = 0; i < guest.Count; i++)
                {
                    var g = guest[i];
                    var p = products.FirstOrDefault(x => x.Id == g.ProductId);
                    if (p == null) continue;
                    items.Add(new CartItem
                    {
                        Id = -(i + 1),
                        ProductId = g.ProductId,
                        Quantity = g.Quantity,
                        Size = g.Size,
                        Color = g.Color,
                        Product = p
                    });
                }
                return items;
            }

            return _db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == UserId)
                .ToList();
        }

        // ─── Checkout GET ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Checkout()
        {
            if (HttpContext.Session.GetString("UserRole") == "admin")
            {
                TempData["Error"] = "Admins cannot place orders.";
                return RedirectToAction("Index", "Home");
            }

            var items = GetCheckoutItems();
            if (!items.Any()) return RedirectToAction("Index", "Cart");

            var cartTotal = CheckoutService.ComputeCartTotal(items);
            var settings = _db.SiteSettings.FirstOrDefault(s => s.Id == 1);
            var delivery = CheckoutService.ComputeDeliveryCharge(items, settings);

            ViewBag.Total = cartTotal;
            ViewBag.DeliveryCharge = delivery;
            ViewBag.GrandTotal = cartTotal + delivery;
            ViewBag.IsGuest = UserId == null;
            ViewBag.CodEnabled = settings?.CodEnabled ?? true;

            if (UserId != null)
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
                if (user == null) return RedirectToAction("Login", "Auth");

                ViewBag.Email = user.Email;
                ViewBag.Phone = user.Phone ?? "";

                var nameParts = (user.FullName ?? "").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                ViewBag.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
                ViewBag.LastName = nameParts.Length > 1 ? nameParts[1] : "";
            }
            else
            {
                ViewBag.Email = "";
                ViewBag.Phone = "";
                ViewBag.FirstName = "";
                ViewBag.LastName = "";
            }

            return View(items);
        }

        // ─── Checkout POST ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(
            string firstName,
            string lastName,
            string address,
            string city,
            string? postalCode,
            string phone,
            string country,
            string paymentMethod,
            string? email,
            CancellationToken cancellationToken)
        {
            if (HttpContext.Session.GetString("UserRole") == "admin")
            {
                TempData["Error"] = "Admins cannot place orders.";
                return RedirectToAction("Index", "Home");
            }

            var deliveryAddress = BuildDeliveryAddress(firstName, lastName, address, city, postalCode, phone, country);
            if (deliveryAddress == null)
            {
                TempData["Error"] = "Please fill in all required delivery fields.";
                return RedirectToAction("Checkout");
            }

            try
            {
                if (UserId == null)
                {
                    // Guest checkout
                    var guestItems = GetCheckoutItems();
                    var result = await _checkout.ProcessGuestAsync(
                        $"{firstName} {lastName}".Trim(),
                        phone,
                        email,
                        deliveryAddress,
                        paymentMethod,
                        guestItems,
                        cancellationToken);

                    if (!result.Success)
                    {
                        TempData["Error"] = result.Error;
                        return RedirectToAction("Checkout");
                    }

                    _guestCart.Clear(HttpContext.Session);
                    TempData["OrderId"] = result.OrderId!.Value.ToString();
                    TempData["GuestEmail"] = email!.Trim();
                    TempData["Success"] = $"Order #{result.OrderId} placed successfully!";

                    var placedOrder = _db.Orders
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                        .FirstOrDefault(o => o.Id == result.OrderId);

                    if (placedOrder != null && !string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            await _emailSender.SendAsync(
                                email.Trim(),
                                $"Amal Collection Order Confirmation #{result.OrderId}",
                                EmailTemplates.OrderConfirmation(
                                    $"{firstName} {lastName}".Trim(),
                                    placedOrder,
                                    Logo,
                                    SiteUrl)
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Order confirmation email failed for order #{OrderId}", result.OrderId);
                        }
                    }

                    var guestAdminEmail = string.IsNullOrWhiteSpace(_emailSettings.AdminEmail) ? _emailSettings.FromEmail : _emailSettings.AdminEmail;
                    if (placedOrder != null && !string.IsNullOrWhiteSpace(guestAdminEmail))
                    {
                        try
                        {
                            await _emailSender.SendAsync(
                                guestAdminEmail,
                                $"Amal Collection New Order #{result.OrderId}",
                                EmailTemplates.NewOrder(
                                    $"{firstName} {lastName}".Trim(),
                                    placedOrder,
                                    Logo,
                                    SiteUrl)
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "New order admin email failed for order #{OrderId}", result.OrderId);
                        }
                    }

                    return RedirectToAction("Confirmation", new { id = result.OrderId });
                }

                // Logged-in checkout
                var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
                if (user != null && !string.IsNullOrWhiteSpace(phone))
                {
                    user.Phone = phone.Trim();
                    await _db.SaveChangesAsync(cancellationToken);
                }

                var userResult = await _checkout.ProcessAsync(UserId.Value, deliveryAddress, paymentMethod, cancellationToken);

                if (!userResult.Success)
                {
                    TempData["Error"] = userResult.Error;
                    return RedirectToAction("Checkout");
                }

                if (user != null)
                {
                    var placedOrder = _db.Orders
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                        .FirstOrDefault(o => o.Id == userResult.OrderId);

                    if (placedOrder != null)
                    {
                        try
                        {
                            await _emailSender.SendAsync(
                                user.Email,
                                $"Amal Collection Order Confirmation #{userResult.OrderId}",
                                EmailTemplates.OrderConfirmation(
                                    user.FullName,
                                    placedOrder,
                                    Logo,
                                    SiteUrl)
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Order confirmation email failed for order #{OrderId}", userResult.OrderId);
                        }

                        var adminEmail = string.IsNullOrWhiteSpace(_emailSettings.AdminEmail) ? _emailSettings.FromEmail : _emailSettings.AdminEmail;
                        if (!string.IsNullOrWhiteSpace(adminEmail))
                        {
                            try
                            {
                                await _emailSender.SendAsync(
                                    adminEmail,
                                    $"Amal Collection New Order #{userResult.OrderId}",
                                    EmailTemplates.NewOrder(
                                        user.FullName,
                                        placedOrder,
                                        Logo,
                                        SiteUrl)
                                );
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "New order admin email failed for order #{OrderId}", userResult.OrderId);
                            }
                        }
                    }
                }

                TempData["OrderId"] = userResult.OrderId!.Value.ToString();
                TempData["Success"] = $"Order #{userResult.OrderId} placed successfully!";
                return RedirectToAction("Confirmation", new { id = userResult.OrderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not place order: {ex.Message}";
                return RedirectToAction("Checkout");
            }
        }

        // ─── Order Confirmation ──────────────────────────────────
        public IActionResult Confirmation(int? id)
        {
            var orderId = id?.ToString();
            if (string.IsNullOrEmpty(orderId))
                orderId = TempData["OrderId"] as string;
            else
                TempData.Remove("OrderId");

            ViewBag.OrderId = orderId;
            ViewBag.IsGuest = UserId == null;
            ViewBag.GuestEmail = TempData["GuestEmail"] as string ?? "";

            var settings = _db.SiteSettings.FirstOrDefault(s => s.Id == 1);
            ViewBag.SupportPhone = settings?.ContactPhone ?? "0321-6068091";
            return View();
        }

        // ─── Order Tracking (guest-friendly, no login) ───────────
        [HttpGet]
        public IActionResult Track(int? id)
        {
            ViewBag.PrefillId = id;
            return View((Order?)null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Track(int id, string contact)
        {
            ViewBag.PrefillId = id > 0 ? (int?)id : null;

            if (id <= 0 || string.IsNullOrWhiteSpace(contact))
            {
                ViewBag.Error = "Enter your order number and the email or phone you used at checkout.";
                return View((Order?)null);
            }

            var contactTrim = contact.Trim();
            var order = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == id);

            var matched = false;
            if (order != null)
            {
                if (contactTrim.Contains('@'))
                {
                    matched = new[] { order.GuestEmail, order.User?.Email }
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Any(e => string.Equals(e!.Trim(), contactTrim, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    var contactPhone = NormalizePhone(contactTrim);
                    matched = new[] { order.GuestPhone, order.User?.Phone }
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Any(p => NormalizePhone(p!) == contactPhone);
                }
            }

            if (!matched)
            {
                ViewBag.Error = "No order found with those details. Please check the order number and the email or phone used at checkout.";
                return View((Order?)null);
            }

            return View(order);
        }

        private static string NormalizePhone(string phone)
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length > 10 && digits.StartsWith("92"))
                digits = "0" + digits[2..];
            return digits;
        }

        // ─── Order History ────────────────────────────────────────
        public IActionResult History()
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var orders = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == UserId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
            return View(orders);
        }

        private static string? BuildDeliveryAddress(
            string firstName,
            string lastName,
            string address,
            string city,
            string? postalCode,
            string phone,
            string country)
        {
            firstName = firstName?.Trim() ?? "";
            lastName = lastName?.Trim() ?? "";
            address = address?.Trim() ?? "";
            city = city?.Trim() ?? "";
            phone = phone?.Trim() ?? "";
            country = string.IsNullOrWhiteSpace(country) ? "Pakistan" : country.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(address) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(phone))
                return null;

            var lines = new List<string>
            {
                $"{firstName} {lastName}",
                address,
                string.IsNullOrWhiteSpace(postalCode) ? city : $"{city}, {postalCode.Trim()}",
                country,
                $"Phone: {phone}"
            };

            return string.Join(Environment.NewLine, lines);
        }
    }
}
