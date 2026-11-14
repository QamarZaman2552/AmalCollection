using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _db;
        private readonly GuestCartService _guestCart;
        private readonly RecommendationService _rec;
        private readonly ReviewSummaryService _reviews;

        public CartController(AppDbContext db, GuestCartService guestCart, RecommendationService rec, ReviewSummaryService reviews)
        {
            _db = db;
            _guestCart = guestCart;
            _rec = rec;
            _reviews = reviews;
        }

        private int? UserId => HttpContext.Session.GetInt32("UserId");
        private bool IsAdmin => HttpContext.Session.GetString("UserRole") == "admin";

        // Products with variants must never be added with an empty size/color.
        private static string? MissingOptionMessage(Product product, string? size, string? color)
        {
            if (!string.IsNullOrWhiteSpace(product.Sizes) && string.IsNullOrWhiteSpace(size))
                return "Please select a size before adding this product to your cart.";
            if (!string.IsNullOrWhiteSpace(product.Colors) && string.IsNullOrWhiteSpace(color))
                return "Please select a color before adding this product to your cart.";
            return null;
        }

        private static List<CartItem> HydrateGuest(AppDbContext db, List<GuestCartItem> guest)
        {
            var productIds = guest.Select(g => g.ProductId).Distinct().ToList();
            var products = db.Products.Where(p => productIds.Contains(p.Id)).ToList();
            var items = new List<CartItem>();
            for (int i = 0; i < guest.Count; i++)
            {
                var g = guest[i];
                var p = products.FirstOrDefault(x => x.Id == g.ProductId);
                if (p == null) continue;
                items.Add(new CartItem
                {
                    Id = -(i + 1), // synthetic id for guest forms
                    ProductId = g.ProductId,
                    Quantity = g.Quantity,
                    Size = g.Size,
                    Color = g.Color,
                    Product = p
                });
            }
            return items;
        }

        // One add-to-cart rulebook shared by the quick-add link, the PDP form and
        // the AJAX endpoint, so stock/option checks can never drift apart.
        private (bool Ok, string Message, bool ProductMissing) TryAddToCart(
            int productId, int quantity, string? size, string? color)
        {
            if (IsAdmin)
                return (false, "Admins cannot purchase products.", false);

            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return (false, "That product is no longer available.", true);

            if (quantity <= 0) quantity = 1;

            size ??= "";
            color ??= "";

            var optionError = MissingOptionMessage(product, size, color);
            if (optionError != null) return (false, optionError, false);

            if (UserId == null)
            {
                var guest = _guestCart.Get(HttpContext.Session);
                var existing = guest.FirstOrDefault(g =>
                    g.ProductId == productId && g.Size == size && g.Color == color);
                var newQty = (existing?.Quantity ?? 0) + quantity;
                if (newQty > product.Stock)
                    return (false, StockMessage(product.Stock), false);

                if (existing != null)
                    existing.Quantity = newQty;
                else
                    guest.Add(new GuestCartItem { ProductId = productId, Quantity = quantity, Size = size, Color = color });

                _guestCart.Save(HttpContext.Session, guest);
                return (true, $"{product.Name} added to cart!", false);
            }

            var dbExisting = _db.CartItems.FirstOrDefault(c =>
                c.UserId == UserId && c.ProductId == productId && c.Size == size && c.Color == color);
            var dbNewQty = (dbExisting?.Quantity ?? 0) + quantity;
            if (dbNewQty > product.Stock)
                return (false, StockMessage(product.Stock), false);

            if (dbExisting != null)
                dbExisting.Quantity = dbNewQty;
            else
                _db.CartItems.Add(new CartItem
                {
                    UserId = UserId.Value,
                    ProductId = productId,
                    Quantity = quantity,
                    Size = size,
                    Color = color
                });

            _db.SaveChanges();
            return (true, $"{product.Name} added to cart!", false);
        }

        private static string StockMessage(int stock) =>
            $"Not enough stock available. Only {stock} unit(s) left.";

        private int CartCount()
        {
            if (UserId == null)
                return _guestCart.Count(HttpContext.Session);
            return _db.CartItems.Where(c => c.UserId == UserId).Sum(c => c.Quantity);
        }

        // ─── View Cart ───────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (IsAdmin)
            {
                TempData["Error"] = "Admins cannot purchase products.";
                return RedirectToAction("Index", "Home");
            }

            List<CartItem> items;
            if (UserId == null)
            {
                items = HydrateGuest(_db, _guestCart.Get(HttpContext.Session));
            }
            else
            {
                items = _db.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.UserId == UserId)
                    .ToList();
            }

            var cartTotal = CheckoutService.ComputeCartTotal(items);
            var settings = _db.SiteSettings.FirstOrDefault(s => s.Id == 1);
            ViewBag.CartTotal = cartTotal;
            ViewBag.CartSavings = CheckoutService.ComputeSavings(items);
            ViewBag.DeliveryCharge = CheckoutService.ComputeDeliveryCharge(items, settings);
            ViewBag.GrandTotal = cartTotal + CheckoutService.ComputeDeliveryCharge(items, settings);
            ViewBag.FreeDeliveryThreshold = settings?.FreeDeliveryThreshold ?? 0m;

            // Empty cart is a dead end — offer a few products to restart the journey.
            if (items.Count == 0)
            {
                var recs = await _rec.GetRecommendationsAsync(UserId, 4);
                ViewBag.Recommendations = recs;
                ViewBag.RatingSummaries = await _reviews.GetAsync(recs.Select(p => p.Id));
            }

            return View(items);
        }

        // ─── Add to Cart (GET quick-add) ─────────────────────────
        [HttpGet]
        public IActionResult Add(int productId, int quantity = 1, string? size = null, string? color = null)
        {
            var result = TryAddToCart(productId, quantity, size, color);
            if (result.ProductMissing) return RedirectToAction("Index", "Products");

            if (!result.Ok)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index", "Cart");
        }

        // ─── Add to Cart (form post) ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPost(int productId, int quantity = 1, string? size = null, string? color = null)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Quantity must be at least 1.";
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            var result = TryAddToCart(productId, quantity, size, color);
            if (result.ProductMissing) return NotFound();

            if (!result.Ok)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        // ─── Add to Cart (AJAX) ──────────────────────────────────
        // JSON response lets the page stay put: { ok, message, cartCount }.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddJson(int productId, int quantity = 1, string? size = null, string? color = null)
        {
            if (quantity <= 0)
                return Json(new { ok = false, message = "Quantity must be at least 1.", cartCount = CartCount() });

            var result = TryAddToCart(productId, quantity, size, color);
            return Json(new { ok = result.Ok, message = result.Message, cartCount = CartCount() });
        }

        // ─── Update Quantity ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            if (IsAdmin) return RedirectToAction("Index", "Home");

            if (UserId == null)
            {
                var guest = _guestCart.Get(HttpContext.Session);
                int idx = -cartItemId - 1;
                if (idx < 0 || idx >= guest.Count) return RedirectToAction("Index");

                if (quantity <= 0)
                {
                    guest.RemoveAt(idx);
                }
                else
                {
                    var product = _db.Products.FirstOrDefault(p => p.Id == guest[idx].ProductId);
                    if (product != null && quantity > product.Stock)
                    {
                        TempData["Error"] = $"Not enough stock for \"{product.Name}\". Maximum: {product.Stock}.";
                        return RedirectToAction("Index");
                    }
                    guest[idx].Quantity = quantity;
                }
                _guestCart.Save(HttpContext.Session, guest);
                return RedirectToAction("Index");
            }

            var item = _db.CartItems.Include(c => c.Product).FirstOrDefault(c => c.Id == cartItemId && c.UserId == UserId);
            if (item == null) return NotFound();

            if (quantity <= 0)
            {
                _db.CartItems.Remove(item);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            if (item.Product != null && quantity > item.Product.Stock)
            {
                TempData["Error"] = $"Not enough stock for \"{item.Product.Name}\". Maximum: {item.Product.Stock}.";
                return RedirectToAction("Index");
            }

            item.Quantity = quantity;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ─── Remove from Cart ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int cartItemId)
        {
            if (IsAdmin) return RedirectToAction("Index", "Home");

            if (UserId == null)
            {
                var guest = _guestCart.Get(HttpContext.Session);
                int idx = -cartItemId - 1;
                if (idx >= 0 && idx < guest.Count)
                {
                    guest.RemoveAt(idx);
                    _guestCart.Save(HttpContext.Session, guest);
                }
                return RedirectToAction("Index");
            }

            var item = _db.CartItems.FirstOrDefault(c => c.Id == cartItemId && c.UserId == UserId);
            if (item != null) { _db.CartItems.Remove(item); _db.SaveChanges(); }
            return RedirectToAction("Index");
        }

        // ─── Cart Count (for header badge) ───────────────────────
        public IActionResult Count() => Json(CartCount());

        // ─── Mini-cart payload (drawer in navbar) ────────────────
        [HttpGet]
        public IActionResult Mini()
        {
            if (IsAdmin)
                return Json(new { blocked = true, isEmpty = true, count = 0, items = Array.Empty<object>() });

            List<CartItem> items;
            if (UserId == null)
            {
                items = HydrateGuest(_db, _guestCart.Get(HttpContext.Session));
            }
            else
            {
                items = _db.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.UserId == UserId)
                    .ToList();
            }

            var settings = _db.SiteSettings.FirstOrDefault(s => s.Id == 1);
            var subtotal = CheckoutService.ComputeCartTotal(items);
            var delivery = CheckoutService.ComputeDeliveryCharge(items, settings);

            return Json(new
            {
                isEmpty = items.Count == 0,
                count = items.Sum(i => i.Quantity),
                subtotal,
                delivery,
                grand = subtotal + delivery,
                freeThreshold = settings?.FreeDeliveryThreshold ?? 0,
                items = items.Select(i => new
                {
                    id = i.Id,
                    productId = i.ProductId,
                    name = i.Product?.Name ?? "",
                    image = i.Product?.ImagePath ?? "/images/placeholder.jpg",
                    qty = i.Quantity,
                    size = i.Size ?? "",
                    color = i.Color ?? "",
                    price = i.Product?.FinalPrice ?? 0,
                    lineTotal = (i.Product?.FinalPrice ?? 0) * i.Quantity,
                    url = $"/Products/Detail/{i.ProductId}"
                }).ToList()
            });
        }

        // ─── Quick Add guard: kya product pehle se cart me hai? ──
        public IActionResult Contains(int productId)
        {
            if (UserId == null)
            {
                var guest = _guestCart.Get(HttpContext.Session);
                return Json(new { inCart = guest.Any(g => g.ProductId == productId) });
            }
            var inCart = _db.CartItems.Any(c => c.UserId == UserId && c.ProductId == productId);
            return Json(new { inCart });
        }
    }
}
