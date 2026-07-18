using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }  // 1 - 5 stars

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
