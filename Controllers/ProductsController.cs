using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly RecommendationService _rec;

        public ProductsController(AppDbContext db, RecommendationService rec)
        {
            _db = db;
            _rec = rec;
        }

        // ─── Homepage / Product List ──────────────────────────────────────
        public IActionResult Index(string? category, string? brand, string? sort)
        {
            var query = _db.Products.AsQueryable();
            if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category != null && p.Category.ToLower() == category.ToLower());
            if (!string.IsNullOrEmpty(brand))    query = query.Where(p => p.Brand != null && p.Brand.ToLower() == brand.ToLower());

            // Sort by price
            query = sort switch
            {
                "price_asc"  => query.OrderBy(p => p.Price * (1 - p.Discount / 100m)),
                "price_desc" => query.OrderByDescending(p => p.Price * (1 - p.Discount / 100m)),
                _            => query.OrderByDescending(p => p.CreatedAt)
            };

            var products = query.ToList();

            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.Recommendations = _rec.GetRecommendations(userId, 4);

            ViewBag.HeroSlides = _db.HeroSlides
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToList();

            ViewBag.Categories = GetFilterValues(p => p.Category);
            ViewBag.Brands = GetFilterValues(p => p.Brand);

            ViewBag.SelectedCategory = category;
            ViewBag.SelectedBrand    = brand;
            ViewBag.SelectedSort     = sort ?? "";
            return View(products);
        }

        // ─── Search ─────────────────────────────────────────────
        public IActionResult Search(string q, string? category, string? brand)
        {
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Index");

            var query = _db.Products.AsQueryable();
            query = query.Where(p => p.Name.Contains(q) ||
                                     (p.Description != null && p.Description.Contains(q)));
            if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category != null && p.Category.ToLower() == category.ToLower());
            if (!string.IsNullOrEmpty(brand))    query = query.Where(p => p.Brand != null && p.Brand.ToLower() == brand.ToLower());

            var results = query.ToList();
            ViewBag.Query = q;
            ViewBag.Categories = GetFilterValues(p => p.Category);
            ViewBag.Brands = GetFilterValues(p => p.Brand);
            return View(results);
        }

        // Distinct, trimmed, case-insensitive, sorted, in-stock only
        private List<string> GetFilterValues(Func<Product, string?> selector)
        {
            return _db.Products
                .Where(p => p.Stock > 0)
                .Select(p => selector(p))
                .ToList()
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // ─── Product Detail ──────────────────────────────────────
        public IActionResult Detail(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
                _rec.RecordView(userId.Value, id);

            var cat = product.Category;
            ViewBag.Related = _db.Products
                .Where(p => p.Category == cat && p.Id != id && p.Stock > 0)
                .Take(4).ToList();

            // Load reviews with user names
            var reviews = _db.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            ViewBag.Reviews       = reviews;
            ViewBag.ReviewCount   = reviews.Count;
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;

            // Has the current user already reviewed?
            ViewBag.UserReviewed = userId.HasValue &&
                reviews.Any(r => r.UserId == userId.Value);

            // Has the current user purchased this product?
            ViewBag.HasPurchased = userId.HasValue && _db.OrderItems
                .Any(oi => oi.ProductId == id && oi.Order != null && oi.Order.UserId == userId.Value);

            return View(product);
        }

        // ─── Submit Review ───────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitReview(int productId, int rating, string? comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Please login to submit a review.";
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Detail", "Products", new { id = productId }) });
            }

            // Only verified buyers can review
            bool hasPurchased = _db.OrderItems
                .Any(oi => oi.ProductId == productId && oi.Order != null && oi.Order.UserId == userId.Value);
            if (!hasPurchased)
            {
                TempData["Error"] = "Only verified buyers can review this product.";
                return RedirectToAction("Detail", new { id = productId });
            }

            // One review per user per product
            bool alreadyReviewed = _db.Reviews
                .Any(r => r.ProductId == productId && r.UserId == userId.Value);

            if (!alreadyReviewed && rating >= 1 && rating <= 5)
            {
                _db.Reviews.Add(new Review
                {
                    ProductId = productId,
                    UserId    = userId.Value,
                    Rating    = rating,
                    Comment   = comment?.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
                _db.SaveChanges();
                TempData["Success"] = "Your review has been submitted. Thank you!";
            }

            return RedirectToAction("Detail", new { id = productId });
        }
    }
}
