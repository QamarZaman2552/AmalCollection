using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Services
{
    public class ChatbotService
    {
        private readonly AppDbContext _db;

        public ChatbotService(AppDbContext db)
        {
            _db = db;
        }

        public string GetResponse(string message, int? userId)
        {
            string reply = ProcessIntent(message.Trim().ToLower());
            LogChat(userId, message, reply);
            return reply;
        }

        private string ProcessIntent(string msg)
        {
            // --- Greetings ---
            if (ContainsAny(msg, "hello", "hi", "hey", "salam", "assalam"))
                return "Hello! 👋 Welcome to ShopAI. How can I help you today? You can ask me about products, prices, stock, or categories!";

            if (ContainsAny(msg, "bye", "goodbye", "alvida"))
                return "Goodbye! 👋 Happy shopping! Come back anytime.";

            if (ContainsAny(msg, "thank", "thanks", "shukriya"))
                return "You're welcome! 😊 Is there anything else I can help you with?";

            // --- Shipping & Policy ---
            if (ContainsAny(msg, "shipping", "deliver", "delivery time"))
                return "🚚 We offer free delivery on orders above PKR 5,000. Standard delivery takes 3-5 business days across Pakistan.";

            if (ContainsAny(msg, "return", "refund", "exchange"))
                return "↩️ We have a 7-day return/exchange policy. Products must be unused and in original packaging. Contact support to initiate a return.";

            if (ContainsAny(msg, "payment", "how to pay", "cod", "cash on delivery", "card"))
                return "💳 We accept Cash on Delivery (COD), Credit/Debit Cards, and Bank Transfer. Choose your preferred method at checkout.";

            if (ContainsAny(msg, "discount", "sale", "offer", "promo", "coupon"))
                return "🏷️ Check out our products for active discounts! Many items have up to 20% off right now.";

            if (ContainsAny(msg, "contact", "support", "help", "number", "phone"))
                return "📞 You can reach our support at support@shopai.pk or call 0300-1234567, Mon-Sat 9AM-6PM.";

            // --- Product Categories ---
            string? category = null;
            if (ContainsAny(msg, "laptop", "notebooks", "computer")) category = "Laptops";
            else if (ContainsAny(msg, "phone", "mobile", "smartphone")) category = "Phones";
            else if (ContainsAny(msg, "headphone", "earphone", "earbuds", "audio", "speaker")) category = "Audio";
            else if (ContainsAny(msg, "monitor", "screen", "display")) category = "Monitors";
            else if (ContainsAny(msg, "keyboard", "mouse", "hub", "accessory", "accessories")) category = "Accessories";
            else if (ContainsAny(msg, "ssd", "storage", "hard drive", "drive")) category = "Storage";

            if (category != null)
            {
                var products = _db.Products
                    .Where(p => p.Category == category && p.Stock > 0)
                    .Take(3)
                    .ToList();

                if (!products.Any())
                    return $"😔 Sorry, we currently have no {category} in stock. Check back soon!";

                var list = string.Join("\n", products.Select(p =>
                    $"• **{p.Name}** – PKR {p.FinalPrice:N0}" + (p.Discount > 0 ? $" ~~PKR {p.Price:N0}~~ ({p.Discount}% off)" : "")));

                return $"🛍️ Here are our top {category}:\n\n{list}\n\nVisit our Products page to see all items!";
            }

            // --- Price / Stock specific query ---
            if (ContainsAny(msg, "cheapest", "cheap", "affordable", "budget", "lowest price"))
            {
                var p = _db.Products.Where(x => x.Stock > 0).OrderBy(x => x.Price).FirstOrDefault();
                if (p != null)
                    return $"💰 The most affordable product is **{p.Name}** at PKR {p.FinalPrice:N0}!";
            }

            if (ContainsAny(msg, "expensive", "premium", "best", "top", "high end"))
            {
                var p = _db.Products.Where(x => x.Stock > 0).OrderByDescending(x => x.Price).FirstOrDefault();
                if (p != null)
                    return $"⭐ Our premium pick is **{p.Name}** at PKR {p.FinalPrice:N0}!";
            }

            // --- Keyword search in product names ---
            var words = msg.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                           .Where(w => w.Length > 3).ToList();

            foreach (var word in words)
            {
                var matches = _db.Products
                    .Where(p => p.Name.ToLower().Contains(word) || (p.Description != null && p.Description.ToLower().Contains(word)))
                    .Where(p => p.Stock > 0)
                    .Take(3)
                    .ToList();

                if (matches.Any())
                {
                    var list = string.Join("\n", matches.Select(p =>
                        $"• **{p.Name}** – PKR {p.FinalPrice:N0}"));
                    return $"🔍 I found these products matching \"{word}\":\n\n{list}\n\nSearch our website for more options!";
                }
            }

            // --- Fallback ---
            return "🤔 I didn't quite understand that. You can ask me about:\n• Products (laptops, phones, accessories)\n• Prices & discounts\n• Shipping & delivery\n• Returns & payment\n\nOr browse our product catalog for everything we offer!";
        }

        private static bool ContainsAny(string msg, params string[] keywords)
            => keywords.Any(k => msg.Contains(k));

        private void LogChat(int? userId, string query, string response)
        {
            try
            {
                _db.ChatbotLogs.Add(new ChatbotLog
                {
                    UserId = userId,
                    Query = query,
                    Response = response
                });
                _db.SaveChanges();
            }
            catch { /* Logging failure should not break the chat */ }
        }
    }
}
