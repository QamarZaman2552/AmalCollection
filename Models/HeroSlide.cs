namespace ShoppingApp.Models
{
    /// <summary>
    /// Represents a single slide in the homepage hero banner.
    /// Managed by admin from /Admin/HeroSlides.
    /// </summary>
    public class HeroSlide
    {
        public int Id { get; set; }

        // Small label above the title, e.g. "SOUND THAT COMPLETES THE FIT"
        public string Tagline { get; set; } = string.Empty;

        // Big bold product/campaign name, e.g. "MAGNITUDE"
        public string Title { get; set; } = string.Empty;

        // Category label, e.g. "HEADPHONES"
        public string CategoryLabel { get; set; } = string.Empty;

        // Hashtag, e.g. "#SOUNDSGREAT"
        public string? Hashtag { get; set; }

        // Where "Shop Now" button links, e.g. "/?category=Audio"
        public string? LinkUrl { get; set; }

        // Linked product ID — when set, "Shop Now" adds this product to cart
        public int? ProductId { get; set; }

        // Price to display on the hero slide (e.g. PKR 29,999)
        public decimal? DisplayPrice { get; set; }

        // Uploaded hero background image
        public string? ImagePath { get; set; }

        // Controls display order (lower = shown first)
        public int SortOrder { get; set; } = 0;

        // Only active slides are shown on the homepage
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
