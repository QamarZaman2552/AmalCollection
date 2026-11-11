using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;

namespace ShoppingApp.Services
{
    /// <summary>Average rating + review count for a single product.</summary>
    public readonly record struct RatingSummary(double Average, int Count)
    {
        public static readonly RatingSummary Empty = new(0, 0);
        public bool HasReviews => Count > 0;
    }

    /// <summary>
    /// Batched rating lookup for product cards, so a grid of 24 cards
    /// costs one query instead of one per product.
    /// </summary>
    public class ReviewSummaryService
    {
        private readonly AppDbContext _db;

        public ReviewSummaryService(AppDbContext db) => _db = db;

        public async Task<Dictionary<int, RatingSummary>> GetAsync(IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, RatingSummary>();

            var rows = await _db.Reviews
                .AsNoTracking()
                .Where(r => ids.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Average = g.Average(r => (double)r.Rating),
                    Count = g.Count()
                })
                .ToListAsync();

            return rows.ToDictionary(r => r.ProductId, r => new RatingSummary(r.Average, r.Count));
        }
    }
}
