using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // null = guest review (order-verified without account)
        public int? UserId { get; set; }
        public User? User { get; set; }

        // ── Order verification (guest-friendly, no login) ──
        public int? OrderId { get; set; }        // the verified order this review belongs to
        public string? GuestName { get; set; }   // display name for guest reviews
        public string? Contact { get; set; }     // phone/email used at checkout (debug/dedupe)
        public bool IsVerified { get; set; }     // true = matched against a real order

        [Required, Range(1, 5)]
        public int Rating { get; set; }  // 1 - 5 stars

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
