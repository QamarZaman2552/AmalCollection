using System.Linq.Expressions;
using System.Text.RegularExpressions;
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
        private const int PageSize = 24;

        public async Task<IActionResult> Index(string? category, string? season, string? fabric, string? size, string? sort, int page = 1)
        {
            var query = _db.Products.AsNoTracking();
            if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category != null && p.Category.ToLower() == category.ToLower());
            if (!string.IsNullOrEmpty(season))  query = query.Where(p => p.Season != null && p.Season.ToLower() == season.ToLower());
            if (!string.IsNullOrEmpty(fabric))  query = query.Where(p => p.Fabric != null && p.Fabric.ToLower() == fabric.ToLower());
             if (!string.IsNullOrEmpty(size))
             {
                 var s = size.Trim();
                 query = query.Where(p => p.Sizes != null && (
                     p.Sizes == s ||
                     p.Sizes.StartsWith(s + ",") ||
                     p.Sizes.EndsWith("," + s) ||
                     p.Sizes.Contains("," + s + ",")
                 ));
             }

            // Sort by price
            query = sort switch
            {
                "price_asc"  => query.OrderBy(p => p.Price * (1 - p.Discount / 100m)),
                "price_desc" => query.OrderByDescending(p => p.Price * (1 - p.Discount / 100m)),
                _            => query.OrderByDescending(p => p.CreatedAt)
            };

            // Count + paginate (24 per page) — only one page of rows is transferred
            var totalCount = await query.CountAsync();
            var pageCount = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            if (page < 1) page = 1;
            if (page > pageCount) page = pageCount;
            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.Recommendations = await _rec.GetRecommendationsAsync(userId, 4);

            ViewBag.HeroSlides = await _db.HeroSlides
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            ViewBag.PromotionalCards = await _db.PromotionalCards
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Id)
                .ToListAsync();

            ViewBag.Categories = await GetFilterValuesAsync(p => p.Category);
            ViewBag.Seasons    = await GetFilterValuesAsync(p => p.Season);
            ViewBag.Fabrics    = await GetFilterValuesAsync(p => p.Fabric);
            ViewBag.Sizes      = new List<string> { "XS", "S", "M", "L", "XL", "XXL" };

            ViewBag.SelectedCategory = category;
            ViewBag.SelectedSeason   = season;
            ViewBag.SelectedFabric   = fabric;
            ViewBag.SelectedSize     = size;
            ViewBag.SelectedSort     = sort ?? "";
            ViewBag.TotalCount = totalCount;
            ViewBag.PageCount  = pageCount;
            ViewBag.Page       = page;
            return View(products);
        }

        // ─── Search ─────────────────────────────────────────────
        public async Task<IActionResult> Search(string q, string? category, string? season)
        {
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Index");

            var query = _db.Products.AsNoTracking();
            query = query.Where(p => p.Name.Contains(q) ||
                                     (p.Description != null && p.Description.Contains(q)));
            if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category != null && p.Category.ToLower() == category.ToLower());
            if (!string.IsNullOrEmpty(season))   query = query.Where(p => p.Season != null && p.Season.ToLower() == season.ToLower());

            var results = await query.ToListAsync();
            ViewBag.Query = q;
            ViewBag.Categories = await GetFilterValuesAsync(p => p.Category);
            ViewBag.Seasons    = await GetFilterValuesAsync(p => p.Season);
            return View(results);
        }

        // Distinct, trimmed, case-insensitive, sorted, in-stock only.
        // Runs in SQL (DISTINCT + LTRIM/RTRIM) — only the small distinct list
        // comes back, instead of every product row.
        private async Task<List<string>> GetFilterValuesAsync(Expression<Func<Product, string?>> selector)
        {
            var values = await _db.Products
                .AsNoTracking()
                .Where(p => p.Stock > 0)
                .Select(selector)
                .Where(v => v != null && v.Trim() != "")
                .Select(v => v!.Trim())
                .Distinct()
                .ToListAsync();
            return values
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // ─── Product Detail ──────────────────────────────────────
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
                await _rec.RecordViewAsync(userId.Value, id);

            var cat = product.Category;
            ViewBag.Related = await _db.Products
                .AsNoTracking()
                .Where(p => p.Category == cat && p.Id != id && p.Stock > 0)
                .Take(4).ToListAsync();

            // Load reviews with user names
            var reviews = await _db.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Reviews       = reviews;
            ViewBag.ReviewCount   = reviews.Count;
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;

            // Has the current user already reviewed?
            ViewBag.UserReviewed = userId.HasValue &&
                reviews.Any(r => r.UserId == userId.Value);

            // Has the current user purchased this product?
            ViewBag.HasPurchased = userId.HasValue && await _db.OrderItems
                .AnyAsync(oi => oi.ProductId == id && oi.Order != null && oi.Order.UserId == userId.Value);

            return View(product);
        }

        // ─── Submit Review ───────────────────────────────────────
        // Two paths:
        //  1) Logged-in buyer  → verified via their account orders (no extra fields)
        //  2) Anyone (guest)   → order verification: order number + phone/email
        //     used at checkout — no account needed (industry "verified purchase" rule)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitReview(int productId, int rating, string? comment, int? orderId, string? contact)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Please select a star rating.";
                return RedirectToAction("Detail", new { id = productId });
            }

            // Path 1: logged-in buyer (verified via account orders)
            bool hasPurchased = userId.HasValue && _db.OrderItems
                .Any(oi => oi.ProductId == productId && oi.Order != null && oi.Order.UserId == userId.Value);

            if (hasPurchased)
            {
                if (_db.Reviews.Any(r => r.ProductId == productId && r.UserId == userId))
                {
                    TempData["Error"] = "You have already reviewed this product.";
                    return RedirectToAction("Detail", new { id = productId });
                }

                var accountOrder = _db.Orders
                    .Where(o => o.UserId == userId && o.OrderItems.Any(oi => oi.ProductId == productId))
                    .OrderByDescending(o => o.Id)
                    .FirstOrDefault();

                _db.Reviews.Add(new Review
                {
                    ProductId = productId,
                    UserId    = userId,
                    OrderId   = accountOrder?.Id,
                    IsVerified = true,
                    Rating    = rating,
                    Comment   = comment?.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
                _db.SaveChanges();
                TempData["Success"] = "Your review has been submitted. Thank you!";
                return RedirectToAction("Detail", new { id = productId });
            }

            // Path 2: order verification — no login required (guest or non-buyer)
            if (orderId == null || string.IsNullOrWhiteSpace(contact))
            {
                TempData["Error"] = "Enter your order number and the phone or email used at checkout.";
                return RedirectToAction("Detail", new { id = productId });
            }

            var order = _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == orderId.Value);

            if (order == null || !order.OrderItems.Any(oi => oi.ProductId == productId))
            {
                TempData["Error"] = "We couldn't find that order with this product. Check your order number.";
                return RedirectToAction("Detail", new { id = productId });
            }

            if (!ContactMatchesOrder(order, contact.Trim()))
            {
                TempData["Error"] = "The phone or email doesn't match this order.";
                return RedirectToAction("Detail", new { id = productId });
            }

            bool alreadyReviewed = _db.Reviews.Any(r => r.OrderId == order.Id && r.ProductId == productId)
                || (userId.HasValue && _db.Reviews.Any(r => r.ProductId == productId && r.UserId == userId.Value));
            if (alreadyReviewed)
            {
                TempData["Error"] = "You have already reviewed this order.";
                return RedirectToAction("Detail", new { id = productId });
            }

            _db.Reviews.Add(new Review
            {
                ProductId = productId,
                UserId    = userId,               // null for guests
                OrderId   = order.Id,
                GuestName = order.GuestName ?? order.User?.FullName ?? "Customer",
                Contact   = contact.Trim(),
                IsVerified = true,
                Rating    = rating,
                Comment   = comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            });
            _db.SaveChanges();
            TempData["Success"] = "Your review has been submitted. Thank you!";
            return RedirectToAction("Detail", new { id = productId });
        }

        // Phone or email match against the order (guest fields or account user)
        private static bool ContactMatchesOrder(Order order, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            if (!string.IsNullOrWhiteSpace(order.GuestPhone) && PhonesEqual(order.GuestPhone, input)) return true;
            if (order.User?.Phone != null && PhonesEqual(order.User.Phone, input)) return true;

            if (!string.IsNullOrWhiteSpace(order.GuestEmail) &&
                order.GuestEmail.Trim().Equals(input, StringComparison.OrdinalIgnoreCase)) return true;
            if (order.User?.Email != null &&
                order.User.Email.Trim().Equals(input, StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private static bool PhonesEqual(string a, string b)
        {
            var da = Digits(a);
            var db = Digits(b);
            if (da.Length == 0 || db.Length == 0) return false;
            if (da == db) return true;
            // Normalize international format: 923001234567 ↔ 03001234567
            if (da.Length == 12 && da.StartsWith("92")) da = "0" + da[2..];
            if (db.Length == 12 && db.StartsWith("92")) db = "0" + db[2..];
            return da == db;
        }

        private static string Digits(string s) => Regex.Replace(s, @"\D", "");
    }
}
