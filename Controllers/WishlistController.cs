using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Controllers
{
    public class WishlistController : Controller
    {
        private readonly AppDbContext _db;
        public WishlistController(AppDbContext db) { _db = db; }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        // ─── View Wishlist ────────────────────────────────────
        public IActionResult Index()
        {
            if (UserId == null)
                return RedirectToAction("Login", "Auth", new { returnUrl = "/Wishlist" });

            var uid = UserId.Value;
            var items = _db.WishlistItems
                .Include(w => w.Product)
                .Where(w => w.UserId == uid)
                .OrderByDescending(w => w.AddedAt)
                .ToList();

            return View(items);
        }

        // ─── Toggle (Add/Remove) ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int productId, string? returnUrl = null)
        {
            if (UserId == null)
                return RedirectToAction("Login", "Auth", new { returnUrl = $"/Products/Detail/{productId}" });

            var uid = UserId.Value;
            var existing = _db.WishlistItems
                .FirstOrDefault(w => w.UserId == uid && w.ProductId == productId);

            if (existing != null)
            {
                _db.WishlistItems.Remove(existing);
                TempData["WishlistMsg"] = "removed";
            }
            else
            {
                _db.WishlistItems.Add(new WishlistItem
                {
                    UserId    = uid,
                    ProductId = productId,
                    AddedAt   = DateTime.UtcNow
                });
                TempData["WishlistMsg"] = "added";
            }

            _db.SaveChanges();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // ─── Remove From Wishlist ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");
            var uid = UserId.Value;
            var item = _db.WishlistItems.FirstOrDefault(w => w.UserId == uid && w.ProductId == productId);
            if (item != null) { _db.WishlistItems.Remove(item); _db.SaveChanges(); }
            TempData["Success"] = "Removed from wishlist.";
            return RedirectToAction("Index");
        }

        // ─── Move to Cart ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoveToCart(int productId)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null || product.Stock <= 0)
            {
                TempData["Error"] = "Product is out of stock.";
                return RedirectToAction("Index");
            }

            var uid = UserId.Value;

            // Add to cart
            var cartItem = _db.CartItems.FirstOrDefault(c => c.UserId == uid && c.ProductId == productId);
            if (cartItem != null) cartItem.Quantity++;
            else _db.CartItems.Add(new CartItem { UserId = uid, ProductId = productId, Quantity = 1 });

            // Remove from wishlist
            var wish = _db.WishlistItems.FirstOrDefault(w => w.UserId == uid && w.ProductId == productId);
            if (wish != null) _db.WishlistItems.Remove(wish);

            _db.SaveChanges();
            TempData["Success"] = $"{product.Name} moved to cart!";
            return RedirectToAction("Index", "Cart");
        }

        // ─── Count (for badge) ────────────────────────────────
        public IActionResult Count()
        {
            if (UserId == null) return Json(0);
            var uid = UserId.Value;
            var count = _db.WishlistItems.Count(w => w.UserId == uid);
            return Json(count);
        }
    }
}
