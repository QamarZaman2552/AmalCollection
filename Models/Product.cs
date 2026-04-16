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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed helper
        public decimal FinalPrice => Price - (Price * Discount / 100);
    }
}
