using ShoppingApp.Data;
using ShoppingApp.Models;
using System.Text.RegularExpressions;

namespace ShoppingApp.Services
{
    public class ChatbotService
    {
        private readonly AppDbContext _db;
        private static readonly Regex SanitizeRegex = new(@"[<>""'&]", RegexOptions.Compiled);
        private static readonly Regex SqlInjectionRegex = new(@"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|EXEC|DECLARE|ALTER|CREATE)\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Dictionary<int, int> _lastProductId = new();

        public ChatbotService(AppDbContext db)
        {
            _db = db;
        }

        public string GetResponse(string message, int? userId)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Please provide a message.";

            var sanitized = SanitizeInput(message);
            if (IsPotentialInjection(sanitized))
                return "I can't process that request. Please try a different query.";

            string reply = ProcessIntent(sanitized.Trim().ToLowerInvariant(), userId);
            LogChat(userId, sanitized, reply);
            return reply;
        }

        private static string SanitizeInput(string input)
        {
            return SanitizeRegex.Replace(input, string.Empty).Trim();
        }

        private static bool IsPotentialInjection(string input)
        {
            return SqlInjectionRegex.IsMatch(input);
        }

        private string ProcessIntent(string msg, int? userId)
        {
            // --- CASUAL CONVERSATION ---
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

            if (ContainsAny(msg, "yes", "no", "ok", "okay"))
                return "Got it! Let me know if you need help finding any products from our catalog. 😊";

            // --- CONTACT / SUPPORT ---
            if (ContainsAny(msg, "contact", "support", "admin", "reach", "email", "phone", "call", "help me contact"))
                return "You can reach our team at:\n📧 qamarbaloch2023@gmail.com\n📞 +92-300-1234567\n\nOr visit our Contact page: /Home/Contact";

            // --- FOLLOW-UP CONTEXT (refers to last discussed product) ---
            var lastPid = GetLastProductId(userId);
            if (lastPid.HasValue && ContainsAnyWord(msg, "this one", "that", "it", "its", "this product", "the link", "link", "buy it", "get it"))
            {
                var prod = _db.Products.Find(lastPid.Value);
                if (prod != null)
                {
                    if (ContainsAnyWord(msg, "price", "cost", "amount"))
                        return FormatProduct(prod);
                    if (ContainsAnyWord(msg, "link", "url", "page", "buy", "get"))
                        return $"Here's the direct link:\n[View {prod.Name}](/Products/Detail/{prod.Id})";
                    return FormatProduct(prod);
                }
            }

            // --- PRICE QUERY ---
            if (ContainsAnyWord(msg, "price", "cost", "amount"))
            {
                var words = msg.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3 && !ContainsAnyWord(w, "price", "cost", "amount")).ToList();
                var prod = FindProductByKeywords(words);
                if (prod != null) return FormatProduct(prod);
            }

            // --- LINK/URL QUERY ---
            if (ContainsAnyWord(msg, "link", "url", "page", "buy", "get"))
            {
                var words = msg.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2 && !ContainsAnyWord(w, "link", "url", "page", "buy", "get", "please")).ToList();
                var prod = FindProductByKeywords(words);
                if (prod != null) return $"[View {prod.Name}](/Products/Detail/{prod.Id}) — PKR {prod.FinalPrice:N0}";
            }

            // --- LIST QUERIES ---
            if (ContainsAny(msg, "show all", "all products", "catalog", "list all", "everything"))
            {
                var allProds = _db.Products.Where(p => p.Stock > 0).Take(10).ToList();
                if (!allProds.Any()) return "Sorry, we don't have any products available in our store right now. 😔";
                var list = string.Join("\n", allProds.Select(FormatProductListItem));
                return $"Here is our currently available catalog:\n\n{list}\n\nSearch our site for more details!";
            }

            // --- BUDGET/PREMIUM ---
            if (ContainsAny(msg, "cheapest", "sasta", "budget", "lowest", "cheap", "affordable"))
            {
                var budgetProds = _db.Products.Where(p => p.Stock > 0).OrderBy(p => p.Price).Take(3).ToList();
                if (!budgetProds.Any()) return "Sorry, we don't have that product available in our store right now. 😔";
                var list = string.Join("\n", budgetProds.Select(FormatProductListItem));
                return $"Here are our 3 most budget-friendly options:\n\n{list}";
            }

            if (ContainsAny(msg, "expensive", "premium", "top", "highest", "mahanga", "best"))
            {
                var premiumProds = _db.Products.Where(p => p.Stock > 0).OrderByDescending(p => p.Price).Take(3).ToList();
                if (!premiumProds.Any()) return "Sorry, we don't have that product available in our store right now. 😔";
                var list = string.Join("\n", premiumProds.Select(FormatProductListItem));
                return $"Here are our 3 premium high-end products:\n\n{list}";
            }

            // --- NEW / LATEST ---
            if (ContainsAny(msg, "new", "latest", "recent", "newest", "fresh", "arrival"))
            {
                var newProds = _db.Products.Where(p => p.Stock > 0).OrderByDescending(p => p.CreatedAt).Take(5).ToList();
                if (!newProds.Any()) return "Sorry, we don't have any new products right now. 😔";
                var list = string.Join("\n", newProds.Select(FormatProductListItem));
                return $"Here are our latest arrivals:\n\n{list}";
            }

            // --- COMPARE ---
            if (ContainsAny(msg, "compare", "vs ", "versus"))
                return "To compare items, please provide the exact names of the two products. I'll find them from our catalog! ⚖️";

            // --- RECOMMEND ---
            if (ContainsAny(msg, "recommend", "suggest", "which one"))
                return "I'd love to help! What is your budget and what are you looking for? I'll suggest the best match from our catalog. 🛒";

            // --- VAGUE PRODUCT QUERY ---
            if (ContainsAny(msg, "product details", "show details", "product info", "details", "show me detail"))
                return "Please tell me the specific product name you'd like details for, or type 'show all' to see our full catalog! 🛍️";

            // --- SPECIFIC PRODUCT (show ONLY one exact match) ---
            var queryWords = msg.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2).ToList();
            var exact = FindProductByKeywords(queryWords);
            if (exact != null)
            {
                SetLastProductId(userId, exact.Id);
                return FormatProduct(exact);
            }

            return "I couldn't find that in our catalog. 😔 Can I help you find something else?";
        }

        private Product? FindProductByKeywords(List<string> words)
        {
            var candidates = _db.Products.Where(p => p.Stock > 0).ToList();
            foreach (var word in words)
            {
                candidates = candidates
                    .Where(p => p.Name.ToLower().Contains(word) ||
                                (p.Category ?? string.Empty).ToLower().Contains(word))
                    .ToList();
                if (!candidates.Any()) break;
            }
            if (candidates.Any()) return candidates.First();
            return null;
        }

        private string FormatProduct(Product p)
        {
            var desc = p.Description ?? "No description available.";
            if (desc.Length > 80) desc = desc.Substring(0, 80) + "...";
            return $"**{p.Name}**\nCategory: {p.Category}\nPrice: PKR {p.FinalPrice:N0}\n_Description: {desc}_\n[View Product →](/Products/Detail/{p.Id})";
        }

        private string FormatProductListItem(Product p)
        {
            return $"• **{p.Name}** ({p.Category}) – PKR {p.FinalPrice:N0}\n  [View Product →](/Products/Detail/{p.Id})";
        }

        private int? GetLastProductId(int? userId)
        {
            if (!userId.HasValue) return null;
            return _lastProductId.TryGetValue(userId.Value, out var pid) ? pid : null;
        }

        private void SetLastProductId(int? userId, int productId)
        {
            if (!userId.HasValue) return;
            _lastProductId[userId.Value] = productId;
        }

        private static bool ContainsAny(string msg, params string[] keywords)
            => keywords.Any(k => msg.Contains(k));

        private static bool ContainsAnyWord(string msg, params string[] keywords)
            => keywords.Any(k => Regex.IsMatch(msg, $@"\b{Regex.Escape(k)}\b", RegexOptions.IgnoreCase));

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