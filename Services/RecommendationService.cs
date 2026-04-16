using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Services
{
    public class RecommendationService
    {
        private readonly AppDbContext _db;
        private static readonly Random _rng = new();

        public RecommendationService(AppDbContext db) { _db = db; }

        public List<Product> GetRecommendations(int? userId, int count = 5)
        {
            if (userId.HasValue)
            {
                // Products the user has already browsed
                var viewedIds = _db.BrowseHistories
                    .Where(b => b.UserId == userId.Value)
                    .Select(b => b.ProductId)
                    .Distinct()
                    .ToList();

                if (viewedIds.Count > 0)
                {
                    // Categories of viewed products (nulls removed in memory)
                    var viewedCategories = _db.Products
                        .Where(p => viewedIds.Contains(p.Id))
                        .ToList()
                        .Select(p => p.Category)
                        .Where(c => c != null)
                        .Distinct()
                        .ToList();

                    // Unseen products — pull to memory, then filter by category
                    var recs = _db.Products
                        .Where(p => !viewedIds.Contains(p.Id) && p.Stock > 0)
                        .ToList()
                        .Where(p => p.Category != null && viewedCategories.Contains(p.Category))
                        .Take(count)
                        .ToList();

                    if (recs.Count >= count) return recs;

                    // Fill remainder with other unseen products
                    var recIds = recs.Select(r => r.Id).ToList();
                    var extra = _db.Products
                        .Where(p => !viewedIds.Contains(p.Id) && !recIds.Contains(p.Id) && p.Stock > 0)
                        .Take(count - recs.Count)
                        .ToList();
                    recs.AddRange(extra);
                    return recs;
                }
            }

            // Fallback: pull all in-stock, shuffle in memory (no SQL translation needed)
            var all = _db.Products.Where(p => p.Stock > 0).ToList();
            return all.OrderBy(_ => _rng.Next()).Take(count).ToList();
        }

        public void RecordView(int userId, int productId)
        {
            var cutoff = DateTime.UtcNow.AddHours(-1);
            var recent = _db.BrowseHistories
                .FirstOrDefault(b => b.UserId == userId &&
                                     b.ProductId == productId &&
                                     b.ViewedAt > cutoff);
            if (recent == null)
            {
                _db.BrowseHistories.Add(new BrowseHistory
                {
                    UserId = userId,
                    ProductId = productId
                });
                _db.SaveChanges();
            }
        }
    }
}
