using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    /// <summary>
    /// Zarr-style promotional swipe card (homepage deck).
    /// Managed by admin from /Admin/PromoCards.
    /// </summary>
    public class PromotionalCard
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [Required]
        public int DiscountPercent { get; set; }

        [Required]
        public decimal CurrentPrice { get; set; }

        [Required]
        public decimal OriginalPrice { get; set; }

        [MaxLength(100)]
        public string? AvailableOnText { get; set; }

        [MaxLength(100)]
        public string? PlatformName { get; set; }

        public string? ImagePath { get; set; }
        public string? LogoPath { get; set; }

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public string BackgroundColor { get; set; } = "#7B4F52";
    }
}
