using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Data;
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

        // ─── Homepage / Product List ─────────────────────────────
        public IActionResult Index(string? category, string? brand)
        {
            var query = _db.Products.AsQueryable();
            if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category == category);
            if (!string.IsNullOrEmpty(brand))    query = query.Where(p => p.Brand == brand);

            var products = query.OrderByDescending(p => p.CreatedAt).ToList();

            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.Recommendations = _rec.GetRecommendations(userId, 4);

            // Hero slides from DB (active only, ordered by SortOrder)
            ViewBag.HeroSlides = _db.HeroSlides
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToList();

            // Filter out nulls so views get clean List<string>
            ViewBag.Categories = _db.Products
                .Select(p => p.Category).Distinct().ToList()
                .Where(c => c != null).Select(c => c!).ToList();
            ViewBag.Brands = _db.Products
                .Select(p => p.Brand).Distinct().ToList()
                .Where(b => b != null).Select(b => b!).ToList();

            ViewBag.SelectedCategory = category;
            ViewBag.SelectedBrand    = brand;
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
            if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category == category);
            if (!string.IsNullOrEmpty(brand))    query = query.Where(p => p.Brand == brand);

            var results = query.ToList();
            ViewBag.Query = q;
            ViewBag.Categories = _db.Products
                .Select(p => p.Category).Distinct().ToList()
                .Where(c => c != null).Select(c => c!).ToList();
            ViewBag.Brands = _db.Products
                .Select(p => p.Brand).Distinct().ToList()
                .Where(b => b != null).Select(b => b!).ToList();
            return View(results);
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

            return View(product);
        }
    }
}
