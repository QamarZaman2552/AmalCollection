namespace ShoppingApp.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;

        // Variant (empty = no variant)
        public string Size { get; set; } = "";
        public string Color { get; set; } = "";

        // Navigation
        public User? User { get; set; }
        public Product? Product { get; set; }
    }
}
