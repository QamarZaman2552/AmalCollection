using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? Phone { get; set; }

        // "customer" | "admin"
        public string Role { get; set; } = "customer";

        public bool IsLocked { get; set; } = false;
        public int FailedAttempts { get; set; } = 0;

        /// <summary>Account wallet balance (PKR). Must stay ≥ 0 after checkout.</summary>
        public decimal Balance { get; set; } = 100_000m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<BrowseHistory> BrowseHistories { get; set; } = new List<BrowseHistory>();
    }
}
