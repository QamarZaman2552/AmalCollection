using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Services
{
    public class RecommendationService
    {
        private readonly AppDbContext _db;

        public RecommendationService(AppDbContext db) { _db = db; }

        public async Task<List<Product>> GetRecommendationsAsync(int? userId, int count = 5)
        {
            if (userId.HasValue)
            {
                // Product IDs the user has already browsed (IDs only)
                var viewedIds = await _db.BrowseHistories
                    .AsNoTracking()
                    .Where(b => b.UserId == userId.Value)
                    .Select(b => b.ProductId)
                    .Distinct()
                    .ToListAsync();

                if (viewedIds.Count > 0)
                {
                    // Categories of viewed products — computed in SQL, not in memory
                    var viewedCategories = await _db.Products
                        .AsNoTracking()
                        .Where(p => viewedIds.Contains(p.Id) && p.Category != null)
                        .Select(p => p.Category)
                        .Distinct()
                        .ToListAsync();

                    // Unseen in-stock products from those categories — only `count` rows
                    var recs = new List<Product>();
                    if (viewedCategories.Count > 0)
                    {
                        recs = await _db.Products
                            .AsNoTracking()
                            .Where(p => !viewedIds.Contains(p.Id)
                                        && p.Stock > 0
                                        && p.Category != null
                                        && viewedCategories.Contains(p.Category))
                            .Take(count)
                            .ToListAsync();
                    }

                    if (recs.Count < count)
                    {
                        // Fill remainder with other unseen products
                        var recIds = recs.Select(r => r.Id).ToList();
                        var extra = await _db.Products
                            .AsNoTracking()
                            .Where(p => !viewedIds.Contains(p.Id) && !recIds.Contains(p.Id) && p.Stock > 0)
                            .Take(count - recs.Count)
                            .ToListAsync();
                        recs.AddRange(extra);
                    }
                    return recs;
                }
            }

            // Fallback: random selection done in SQL (NEWID) — only `count` rows
            // are transferred instead of the whole products table.
            return await _db.Products
                .AsNoTracking()
                .Where(p => p.Stock > 0)
                .OrderBy(p => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }

        public async Task RecordViewAsync(int userId, int productId)
        {
            var cutoff = DateTime.UtcNow.AddHours(-1);
            var recent = await _db.BrowseHistories
                .FirstOrDefaultAsync(b => b.UserId == userId &&
                                          b.ProductId == productId &&
                                          b.ViewedAt > cutoff);
            if (recent == null)
            {
                _db.BrowseHistories.Add(new BrowseHistory
                {
                    UserId = userId,
                    ProductId = productId
                });
                await _db.SaveChangesAsync();
            }
        }
    }
}
