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

        public CartController(AppDbContext db) { _db = db; }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        // ─── View Cart ───────────────────────────────────────────
        public IActionResult Index()
        {
            if (UserId == null) return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
            if (HttpContext.Session.GetString("UserRole") == "admin")
            {
                TempData["Error"] = "Admins cannot purchase products.";
                return RedirectToAction("Index", "Home");
            }

            var items = _db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == UserId)
                .ToList();

            var cartTotal = CheckoutService.ComputeCartTotal(items);
            ViewBag.CartTotal = cartTotal;

            return View(items);
        }

        // ─── Add to Cart (GET redirects, POST processes) ─────────
        [HttpGet]
        public IActionResult Add(int productId, int quantity = 1)
        {
            // Redirect GET requests to product detail with error
            TempData["Error"] = "Invalid request. Please use the Add to Cart button on the product page.";
            return RedirectToAction("Detail", "Products", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPost(int productId, int quantity = 1)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Add", "Cart", new { productId, quantity }) });
            if (HttpContext.Session.GetString("UserRole") == "admin")
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

            // Formal rule: requested_quantity ≤ stock
            var existing = _db.CartItems.FirstOrDefault(c => c.UserId == UserId && c.ProductId == productId);
            var newQty = (existing?.Quantity ?? 0) + quantity;
            if (newQty > product.Stock)
            {
                TempData["Error"] = $"Not enough stock available. Only {product.Stock} unit(s) left.";
                return RedirectToAction("Detail", "Products", new { id = productId });
            }

            if (existing != null)
                existing.Quantity = newQty;
            else
                _db.CartItems.Add(new CartItem { UserId = UserId.Value, ProductId = productId, Quantity = quantity });

            _db.SaveChanges();
            TempData["Success"] = $"{product.Name} added to cart!";
            return RedirectToAction("Index");
        }

        // ─── Update Quantity ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            if (UserId == null) return Unauthorized();
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
