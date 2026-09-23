using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;
using System.Text;

namespace ShoppingApp.Services
{
    public class SiteContextService
    {
        private readonly AppDbContext _db;

        public SiteContextService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<string> BuildSiteContextAsync(string userMessage)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== PRODUCT CATALOG (active products) ===");

            var products = await _db.Products
                .Where(p => p.Stock > 0)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Category,
                    p.Brand,
                    Price = p.FinalPrice,
                    p.Description,
                    Stock = p.Stock,
                    Url = $"/Products/Detail/{p.Id}"
                })
                .ToListAsync();

            // If there are many products, filter by keyword relevance or cap to 50
            var keyword = ExtractKeyword(userMessage);
            var filtered = products;

            if (products.Count > 100 && !string.IsNullOrEmpty(keyword))
            {
                filtered = products
                    .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                (p.Category ?? string.Empty).Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                (p.Brand ?? string.Empty).Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                (p.Description?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();

                // If filter yields nothing, fall back to all products (capped)
                if (!filtered.Any())
                    filtered = products.Take(50).ToList();
            }
            else if (products.Count > 100)
            {
                filtered = products.Take(50).ToList();
            }

            foreach (var p in filtered)
            {
                sb.AppendLine($"{p.Name} | Category: {p.Category} | Brand: {p.Brand} | Price: PKR {p.Price:N0} | Stock: {p.Stock} | Link: {p.Url} | Desc: {p.Description?.Substring(0, Math.Min(p.Description.Length, 100))}");
            }

            // Add categories section
            sb.AppendLine("\n=== CATEGORIES ===");
            var categories = await _db.Products
                .Where(p => p.Stock > 0 && !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            if (categories.Any())
                sb.AppendLine(string.Join(", ", categories));

            // Add brands section
            sb.AppendLine("\n=== BRANDS ===");
            var brands = await _db.Products
                .Where(p => p.Stock > 0 && !string.IsNullOrEmpty(p.Brand))
                .Select(p => p.Brand)
                .Distinct()
                .ToListAsync();

            if (brands.Any())
                sb.AppendLine(string.Join(", ", brands));

            // Static site info (shipping, returns, policies, features)
            sb.AppendLine("\n=== SITE INFO ===");
            sb.AppendLine("Store name: Amal Collection - ladies summer and winter suits online store (Pakistan).");
            sb.AppendLine("Shipping: We ship nationwide across Pakistan. Standard delivery takes 3-5 business days. Express delivery available in major cities.");
            sb.AppendLine("Returns: 7-day return policy on all items. Items must be in original packaging.");
            sb.AppendLine("Payment: Cash on Delivery (COD) available nationwide. Credit/debit card payments accepted.");
            sb.AppendLine("Contact: Email us at qamarbaloch2023@gmail.com or use the Contact page (/Home/Contact).");
            sb.AppendLine("About: Visit /Home/About for more info about our store.");
            sb.AppendLine("Accounts: Users can register (sign up) and login on the website. Password reset is available via Forgot Password.");
            sb.AppendLine("Shopping: Add products to cart, then checkout to place an order. Users can also add products to their wishlist.");
            sb.AppendLine("Admin: The admin panel (/Admin) is used by store staff to add/edit/delete products and manage orders.");
            sb.AppendLine("Discounts: Some products show a discount percentage. The final (discounted) price is what customers pay.");
            sb.AppendLine("Recommendations: The homepage shows AI-powered product recommendations based on browsing history.");
            sb.AppendLine("Reviews: Registered users who purchased a product can submit a rating and review on the product detail page.");

            return sb.ToString();
        }

        private static string ExtractKeyword(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return string.Empty;

            // Skip common stop words so the relevance filter picks a real product word
            var stopWords = new[] { "what", "which", "where", "when", "how", "who", "why", "the", "and", "for", "you", "your", "our", "show", "tell", "about", "with", "this", "that", "please", "price", "cost", "buy", "get", "give", "want", "need", "recommend", "recommendation", "best", "cheap", "cheapest", "expensive", "any", "some", "have", "has", "there", "here" };

            var words = userMessage.Split(new[] { ' ', ',', '.', '?', '!', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2)
                .Where(w => !stopWords.Contains(w.ToLowerInvariant()))
                .Select(w => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(w.ToLower()))
                .ToList();

            return words.FirstOrDefault() ?? string.Empty;
        }
    }
}