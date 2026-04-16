namespace ShoppingApp.Models
{
    /// <summary>
    /// Stores pre-aggregated sales snapshots for analytics dashboards.
    /// Period: "monthly" or "weekly"
    /// Label examples: "Apr 2026"  |  "Week 15 / 2026"
    /// </summary>
    public class SaleAnalytics
    {
        public int Id { get; set; }

        // "monthly" | "weekly"
        public string Period { get; set; } = string.Empty;

        // Human-readable label, e.g. "Apr 2026" or "Week 15 / 2026"
        public string Label { get; set; } = string.Empty;

        // Total revenue for this period
        public decimal TotalSales { get; set; }

        // Number of orders placed in this period
        public int OrderCount { get; set; }

        // When this snapshot was computed/recorded
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }
}
