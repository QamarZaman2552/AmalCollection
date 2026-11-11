namespace ShoppingApp.Models
{
    // Single-row site settings (Id = 1)
    public class SiteSettings
    {
        public int Id { get; set; } = 1;

        public bool IsFreeDelivery { get; set; } = true;
        public decimal DeliveryCharge { get; set; } = 0;
        public decimal FreeDeliveryThreshold { get; set; } = 0; // 0 = no threshold
        public bool CodEnabled { get; set; } = true;

        public string? ContactPhone { get; set; }
        public string? ContactWhatsapp { get; set; }
        public string? ContactEmail { get; set; }

        // Social profiles (leave blank to hide the icon in the footer)
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TikTokUrl { get; set; }
    }
}
