namespace ShoppingApp.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public Product? Product { get; set; }
    }
}
