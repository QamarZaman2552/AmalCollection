using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CheckoutService _checkout;
        private readonly IEmailSender _emailSender;
        private readonly EmailSettings _emailSettings;

        public OrdersController(AppDbContext db, CheckoutService checkout, IEmailSender emailSender, EmailSettings emailSettings)
        {
            _db = db;
            _checkout = checkout;
            _emailSender = emailSender;
            _emailSettings = emailSettings;
        }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        private string SiteUrl =>
            $"{Request.Scheme}://{Request.Host}";

        private string Logo => EmailTemplates.GetLogoUrl(SiteUrl);

        // ─── Checkout GET ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Checkout()
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
            if (user == null) return RedirectToAction("Login", "Auth");

            var items = _db.CartItems.Include(c => c.Product).Where(c => c.UserId == UserId).ToList();
            if (!items.Any()) return RedirectToAction("Index", "Cart");

            var cartTotal = CheckoutService.ComputeCartTotal(items);
            ViewBag.Total = cartTotal;
            ViewBag.Email = user.Email;
            ViewBag.Phone = user.Phone ?? "";

            var nameParts = (user.FullName ?? "").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            ViewBag.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
            ViewBag.LastName = nameParts.Length > 1 ? nameParts[1] : "";

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
            CancellationToken cancellationToken)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var deliveryAddress = BuildDeliveryAddress(firstName, lastName, address, city, postalCode, phone, country);
            if (deliveryAddress == null)
            {
                TempData["Error"] = "Please fill in all required delivery fields.";
                return RedirectToAction("Checkout");
            }

            try
            {
                // Keep profile phone in sync for admin order list
                var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
                if (user != null && !string.IsNullOrWhiteSpace(phone))
                {
                    user.Phone = phone.Trim();
                    await _db.SaveChangesAsync(cancellationToken);
                }

                var result = await _checkout.ProcessAsync(UserId.Value, deliveryAddress, paymentMethod, cancellationToken);

                if (!result.Success)
                {
                    TempData["Error"] = result.Error;
                    return RedirectToAction("Checkout");
                }

                if (user != null)
                {
                    try
                    {
                        await _emailSender.SendAsync(
                            user.Email,
                            $"BaazWix Order Confirmation #{result.OrderId}",
                            EmailTemplates.OrderConfirmation(
                                user.FullName,
                                result.OrderId!.Value.ToString(),
                                result.OrderTotal?.ToString("N0") ?? "",
                                paymentMethod,
                                Logo,
                                SiteUrl)
                        );
                    }
                    catch
                    {
                        // Never block checkout if email fails
                    }

                    // Notify admin about the new order
                    var adminEmail = string.IsNullOrWhiteSpace(_emailSettings.AdminEmail) ? _emailSettings.FromEmail : _emailSettings.AdminEmail;
                    if (!string.IsNullOrWhiteSpace(adminEmail))
                    {
                        try
                        {
                            await _emailSender.SendAsync(
                                adminEmail,
                                $"BaazWix New Order #{result.OrderId}",
                                EmailTemplates.NewOrder(
                                    user.FullName,
                                    result.OrderId!.Value.ToString(),
                                    result.OrderTotal?.ToString("N0") ?? "",
                                    paymentMethod,
                                    Logo,
                                    SiteUrl)
                            );
                        }
                        catch
                        {
                            // Never block checkout if admin email fails
                        }
                    }
                }

                TempData["OrderId"] = result.OrderId!.Value.ToString();
                TempData["Success"] = $"Order #{result.OrderId} placed successfully!";
                return RedirectToAction("Confirmation");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not place order: {ex.Message}";
                return RedirectToAction("Checkout");
            }
        }

        // ─── Order Confirmation ──────────────────────────────────
        public IActionResult Confirmation()
        {
            ViewBag.OrderId = TempData["OrderId"];
            return View();
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
