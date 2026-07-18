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
            // --- CASUAL CONVERSATION RULES ---
            if (ContainsAny(msg, "kaisa hai", "kya haal", "how are you", "how r u", "how do you do"))
                return "I'm doing great, thanks for asking! 😊 How can I assist you?";

            if (ContainsAny(msg, "hi", "hello", "hey", "salam", "assalam o alaikum", "assalam"))
                return "Hey there! 👋 How can I help you today?";

            if (ContainsAny(msg, "fine", "good", "great", "alright"))
                return "Glad to hear that! 😊 How can I help you today?";

            if (ContainsAny(msg, "thank", "thanks", "shukriya", "jazakallah"))
                return "You're welcome! 😊 Anything else I can help with?";

            if (ContainsAny(msg, "bye", "goodbye", "khuda hafiz", "allah hafiz", "see ya"))
                return "Take care! Visit again anytime. 👋";

            if (ContainsAny(msg, "what can you do", "help", "what do you do", "who are you"))
                return "I can show you products, prices, compare items, and give recommendations — but only from our store catalog. How can I assist you?";

            if (ContainsAny(msg, "yes", "no", "ok", "okay", "alright"))
                return "Got it! Let me know if you need help finding any products from our catalog. 😊";

            // --- PRODUCT QUERY RULES ---
            
            // Cheapest / budget / sasta -> 3 lowest priced products
            if (ContainsAny(msg, "cheapest", "sasta", "budget", "lowest", "cheap", "affordable"))
            {
                var budgetProds = _db.Products.Where(p => p.Stock > 0).OrderBy(p => p.Price).Take(3).ToList();
                if (!budgetProds.Any()) return "Sorry, we don't have that product available in our store right now. 😔 Can I help you find something else?";
                var list = string.Join("\n", budgetProds.Select(p => $"• **{p.Name}** ({p.Category}) – PKR {p.FinalPrice:N0}"));
                return $"Here are our 3 most budget-friendly options:\n\n{list}";
            }

            // Most expensive / premium -> 3 highest priced products
            if (ContainsAny(msg, "expensive", "premium", "top", "highest", "mahanga", "best"))
            {
                var premiumProds = _db.Products.Where(p => p.Stock > 0).OrderByDescending(p => p.Price).Take(3).ToList();
                if (!premiumProds.Any()) return "Sorry, we don't have that product available in our store right now. 😔 Can I help you find something else?";
                var list = string.Join("\n", premiumProds.Select(p => $"• **{p.Name}** ({p.Category}) – PKR {p.FinalPrice:N0}"));
                return $"Here are our 3 premium high-end products:\n\n{list}";
            }

            // Show all products -> Display catalog neatly (capped to 10 for UI layout)
            if (ContainsAny(msg, "show all", "all products", "catalog", "list all", "everything"))
            {
                var allProds = _db.Products.Where(p => p.Stock > 0).Take(10).ToList(); 
                if (!allProds.Any()) return "Sorry, we don't have any products available in our store right now. 😔";
                var list = string.Join("\n", allProds.Select(p => $"• **{p.Name}** ({p.Category}) – PKR {p.FinalPrice:N0}"));
                return $"Here is our currently available catalog:\n\n{list}\n\nSearch our site for more details!";
            }

            // Compare products
            if (ContainsAny(msg, "compare", "vs ", "versus"))
            {
                return "To compare items, please provide the exact names of the two products from our catalog. If an item is not in our catalog, I won't be able to compare it! ⚖️";
            }

            // Recommendations
            if (ContainsAny(msg, "recommend", "suggest", "best one", "which one"))
            {
                return "I'd love to help! What is your budget and what exactly are you looking for? I'll suggest the absolute best match from our catalog. 🛒";
            }

            // --- STRICT PRODUCT MATCHING RULE ---
            var words = msg.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                           .Where(w => w.Length > 2).ToList();

            var matches = new List<Product>();
            foreach (var word in words)
            {
                var found = _db.Products
                    .Where(p => p.Name.ToLower().Contains(word) || p.Category.ToLower().Contains(word))
                    .Where(p => p.Stock > 0)
                    .ToList();
                matches.AddRange(found);
            }
            matches = matches.Distinct().Take(3).ToList();

            if (matches.Any())
            {
                var list = string.Join("\n\n", matches.Select(p => 
                    $"**{p.Name}**\nCategory: {p.Category}\nPrice: PKR {p.FinalPrice:N0}\n_{(p.Description?.Length > 60 ? p.Description.Substring(0, 60) + "..." : p.Description)}_"));
                return $"Here are the details from our catalog:\n\n{list}";
            }

            // --- FALLBACK (Strict Catalog Rule) ---
            return "Sorry, we don't have that product available in our store right now. 😔 Can I help you find something else?";
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
