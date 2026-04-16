using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _db;

        public CartController(AppDbContext db) { _db = db; }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        // ─── View Cart ───────────────────────────────────────────
        public IActionResult Index()
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var items = _db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == UserId)
                .ToList();
            return View(items);
        }

        // ─── Add to Cart ─────────────────────────────────────────
        [HttpPost]
        public IActionResult Add(int productId, int quantity = 1)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound();

            if (product.Stock < quantity)
            {
                TempData["Error"] = "Not enough stock available.";
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            var existing = _db.CartItems.FirstOrDefault(c => c.UserId == UserId && c.ProductId == productId);
            if (existing != null)
                existing.Quantity += quantity;
            else
                _db.CartItems.Add(new CartItem { UserId = UserId.Value, ProductId = productId, Quantity = quantity });

            _db.SaveChanges();
            TempData["Success"] = $"{product.Name} added to cart!";
            return RedirectToAction("Detail", "Products", new { id = productId });
        }

        // ─── Update Quantity ─────────────────────────────────────
        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            if (UserId == null) return Unauthorized();
            var item = _db.CartItems.FirstOrDefault(c => c.Id == cartItemId && c.UserId == UserId);
            if (item == null) return NotFound();

            if (quantity <= 0)
                _db.CartItems.Remove(item);
            else
                item.Quantity = quantity;

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ─── Remove from Cart ────────────────────────────────────
        [HttpPost]
        public IActionResult Remove(int cartItemId)
        {
            if (UserId == null) return Unauthorized();
            var item = _db.CartItems.FirstOrDefault(c => c.Id == cartItemId && c.UserId == UserId);
            if (item != null) { _db.CartItems.Remove(item); _db.SaveChanges(); }
            return RedirectToAction("Index");
        }

        // ─── Cart Count (for header badge) ───────────────────────
        public IActionResult Count()
        {
            if (UserId == null) return Json(0);
            var count = _db.CartItems.Where(c => c.UserId == UserId).Sum(c => c.Quantity);
            return Json(count);
        }
    }
}
