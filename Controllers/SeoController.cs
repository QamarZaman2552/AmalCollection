using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;

namespace ShoppingApp.Controllers
{
    // robots.txt and sitemap.xml are generated instead of kept as static files so
    // they always use the current host and the live product/category list.
    public class SeoController : Controller
    {
        private readonly AppDbContext _db;

        public SeoController(AppDbContext db)
        {
            _db = db;
        }

        [Route("robots.txt")]
        public IActionResult Robots()
        {
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Allow: /");
            sb.AppendLine();
            sb.AppendLine("# Private areas: no value in search results");
            sb.AppendLine("Disallow: /Admin");
            sb.AppendLine("Disallow: /Auth");
            sb.AppendLine("Disallow: /Cart");
            sb.AppendLine("Disallow: /Profile");
            sb.AppendLine("Disallow: /Wishlist");
            sb.AppendLine();
            sb.AppendLine("Sitemap: " + BaseUrl() + "/sitemap.xml");

            Response.Headers.CacheControl = "public, max-age=86400";
            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }

        [Route("sitemap.xml")]
        public IActionResult Sitemap()
        {
            var baseUrl = BaseUrl();
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            void Add(string path, string? lastmod = null, string? changefreq = null, string? priority = null)
            {
                sb.Append("  <url><loc>").Append(Escape(baseUrl + path)).Append("</loc>");
                if (!string.IsNullOrEmpty(lastmod)) sb.Append("<lastmod>").Append(lastmod).Append("</lastmod>");
                if (!string.IsNullOrEmpty(changefreq)) sb.Append("<changefreq>").Append(changefreq).Append("</changefreq>");
                if (!string.IsNullOrEmpty(priority)) sb.Append("<priority>").Append(priority).Append("</priority>");
                sb.AppendLine("</url>");
            }

            // Landing + content pages
            Add("/", changefreq: "daily", priority: "1.0");
            Add("/About", changefreq: "monthly", priority: "0.4");
            Add("/Contact", changefreq: "monthly", priority: "0.4");

            // Category and season listings (the same URLs the nav and filters use)
            var categories = _db.Categories.AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => c.Name)
                .ToList();
            foreach (var name in categories)
                Add("/?category=" + Uri.EscapeDataString(name), changefreq: "weekly", priority: "0.8");

            var seasons = _db.Products.AsNoTracking()
                .Where(p => p.Season != null && p.Season != "")
                .Select(p => p.Season!)
                .Distinct()
                .ToList();
            foreach (var season in seasons)
                Add("/?season=" + Uri.EscapeDataString(season), changefreq: "weekly", priority: "0.6");

            // Products, newest first
            var products = _db.Products.AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new { p.Id, p.CreatedAt })
                .ToList();
            foreach (var product in products)
                Add("/Products/Detail/" + product.Id, product.CreatedAt.ToString("yyyy-MM-dd"), "weekly", "0.7");

            sb.AppendLine("</urlset>");

            Response.Headers.CacheControl = "public, max-age=3600";
            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }

        private string BaseUrl() => $"{Request.Scheme}://{Request.Host}";

        private static string Escape(string value) =>
            value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                 .Replace("\"", "&quot;").Replace("'", "&apos;");
    }
}
