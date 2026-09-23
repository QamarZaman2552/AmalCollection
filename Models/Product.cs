using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal Discount { get; set; } = 0;   // percentage 0-100
        public int Stock { get; set; } = 0;

        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? ImagePath { get; set; }

        // ── Ladies suits attributes ──
        public string? Season { get; set; }           // "Summer" | "Winter"
        public string? Fabric { get; set; }           // Lawn, Cotton, Chiffon, Khaddar, Wool...
        public string? Sizes { get; set; }            // "XS,S,M,L,XL,XXL"
        public string? Colors { get; set; }           // "Sand,Charcoal,White"
        public string? StitchedType { get; set; }     // "Stitched" | "Unstitched" | "Semi-Stitched"
        public int? Pieces { get; set; }              // 2 | 3

        // ── Delivery ──
        public bool IsFreeDelivery { get; set; } = true;
        public decimal? DeliveryCharge { get; set; }  // when IsFreeDelivery == false

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Gallery (up to 5 images; ImagePath = cover/primary for compat)
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        // Computed helper
        public decimal FinalPrice => Price - (Price * Discount / 100);
    }
}
