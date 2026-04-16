namespace ShoppingApp.Models
{
    public class ChatbotLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Query { get; set; } = string.Empty;
        public string? Response { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
