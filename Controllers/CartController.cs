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

        public CartController(AppDbContext db, GuestCartService guestCart)
        {
            _db = db;
            _guestCart = guestCart;
        }

        private int? UserId => HttpContext.Session.GetInt32("UserId");
        private bool IsAdmin => HttpContext.Session.GetString("UserRole") == "admin";

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

        // ─── View Cart ───────────────────────────────────────────
        public IActionResult Index()
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
            ViewBag.DeliveryCharge = CheckoutService.ComputeDeliveryCharge(items, settings);
            ViewBag.GrandTotal = cartTotal + CheckoutService.ComputeDeliveryCharge(items, settings);

            return View(items);
        }

        // ─── Add to Cart (GET redirects, POST processes) ─────────
        [HttpGet]
        public IActionResult Add(int productId, int quantity = 1)
        {
            TempData["Error"] = "Invalid request. Please use the Add to Cart button on the product page.";
            return RedirectToAction("Detail", "Products", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPost(int productId, int quantity = 1, string? size = null, string? color = null)
        {
            if (IsAdmin)
            {
                TempData["Error"] = "Admins cannot purchase products.";
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound();

            if (quantity <= 0)
            {
                TempData["Error"] = "Quantity must be at least 1.";
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            size ??= "";
            color ??= "";

            if (UserId == null)
            {
                // Guest cart in session
                var guest = _guestCart.Get(HttpContext.Session);
                var existing = guest.FirstOrDefault(g =>
                    g.ProductId == productId && g.Size == size && g.Color == color);
                var newQty = (existing?.Quantity ?? 0) + quantity;
                if (newQty > product.Stock)
                {
                    TempData["Error"] = $"Not enough stock available. Only {product.Stock} unit(s) left.";
                    return RedirectToAction("Detail", "Products", new { id = productId });
                }

                if (existing != null)
                    existing.Quantity = newQty;
                else
                    guest.Add(new GuestCartItem { ProductId = productId, Quantity = quantity, Size = size, Color = color });

                _guestCart.Save(HttpContext.Session, guest);
                TempData["Success"] = $"{product.Name} added to cart!";
                return RedirectToAction("Index");
            }

            var dbExisting = _db.CartItems.FirstOrDefault(c =>
                c.UserId == UserId && c.ProductId == productId && c.Size == size && c.Color == color);
            var dbNewQty = (dbExisting?.Quantity ?? 0) + quantity;
            if (dbNewQty > product.Stock)
            {
                TempData["Error"] = $"Not enough stock available. Only {product.Stock} unit(s) left.";
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

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
            TempData["Success"] = $"{product.Name} added to cart!";
            return RedirectToAction("Index");
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
        public IActionResult Count()
        {
            if (UserId == null)
                return Json(_guestCart.Count(HttpContext.Session));
            var count = _db.CartItems.Where(c => c.UserId == UserId).Sum(c => c.Quantity);
            return Json(count);
        }
    }
}
