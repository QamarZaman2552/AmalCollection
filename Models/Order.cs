namespace ShoppingApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        // null = guest checkout
        public int? UserId { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal TotalAmount { get; set; }

        public string? DeliveryAddress { get; set; }
        public string? PaymentMethod { get; set; }

        // Guest fields (when UserId is null)
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }

        // "pending" | "confirmed" | "shipped" | "delivered" | "cancelled"
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
