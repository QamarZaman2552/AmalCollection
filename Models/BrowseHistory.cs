namespace ShoppingApp.Models
{
    public class BrowseHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Product? Product { get; set; }
    }
}
