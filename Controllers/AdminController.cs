using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) { _db = db; }

        private bool IsAdmin => HttpContext.Session.GetString("UserRole") == "admin";

        // ─── Product List ─────────────────────────────────────────
        public IActionResult Products()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var products = _db.Products.OrderByDescending(p => p.CreatedAt).ToList();
            return View(products);
        }

        // ─── Add Product ──────────────────────────────────────────
        [HttpGet]
        public IActionResult Add()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            return View(new Product());
        }

        [HttpPost]
        public IActionResult Add(Product product, IFormFile? imageFile)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine("wwwroot", "images", fileName);
                using var stream = System.IO.File.Create(path);
                imageFile.CopyTo(stream);
                product.ImagePath = "/images/" + fileName;
            }

            _db.Products.Add(product);
            _db.SaveChanges();
            TempData["Success"] = "Product added successfully!";
            return RedirectToAction("Products");
        }

        // ─── Edit Product ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product updated, IFormFile? imageFile)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var product = _db.Products.FirstOrDefault(p => p.Id == updated.Id);
            if (product == null) return NotFound();

            product.Name = updated.Name;
            product.Description = updated.Description;
            product.Price = updated.Price;
            product.Discount = updated.Discount;
            product.Stock = updated.Stock;
            product.Category = updated.Category;
            product.Brand = updated.Brand;

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine("wwwroot", "images", fileName);
                using var stream = System.IO.File.Create(path);
                imageFile.CopyTo(stream);
                product.ImagePath = "/images/" + fileName;
            }

            _db.SaveChanges();
            TempData["Success"] = "Product updated!";
            return RedirectToAction("Products");
        }

        // ─── Delete Product ───────────────────────────────────────
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product != null) { _db.Products.Remove(product); _db.SaveChanges(); }
            TempData["Success"] = "Product deleted.";
            return RedirectToAction("Products");
        }

        // ─── View Chatbot Logs ────────────────────────────────────
        public IActionResult ChatLogs()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var logs = _db.ChatbotLogs.OrderByDescending(l => l.Timestamp).Take(200).ToList();
            return View(logs);
        }

        // ─── Orders Management (Shipping Details) ─────────────────
        public IActionResult Orders()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            var orders = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        // ─── Update Order Status ──────────────────────────────────
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            var order = _db.Orders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.Status = status;
                _db.SaveChanges();
                TempData["Success"] = $"Order #{id} status updated to {status}.";
            }
            return RedirectToAction("Orders");
        }

        // ─── Shop Analytics ───────────────────────────────────────
        public IActionResult Analytics()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            var allOrders = _db.Orders.ToList();

            // ── Monthly (last 12 months) ──────────────────────────
            var now = DateTime.UtcNow;
            var monthlyLabels  = new List<string>();
            var monthlySales   = new List<decimal>();
            var monthlyOrders  = new List<int>();

            for (int i = 11; i >= 0; i--)
            {
                var date  = now.AddMonths(-i);
                var label = date.ToString("MMM yyyy");
                var sales = allOrders
                    .Where(o => o.CreatedAt.Year == date.Year && o.CreatedAt.Month == date.Month)
                    .Sum(o => o.TotalAmount);
                var count = allOrders
                    .Count(o => o.CreatedAt.Year == date.Year && o.CreatedAt.Month == date.Month);

                monthlyLabels.Add(label);
                monthlySales.Add(sales);
                monthlyOrders.Add(count);
            }

            // ── Weekly (last 8 weeks) ─────────────────────────────
            var weeklyLabels  = new List<string>();
            var weeklySales   = new List<decimal>();
            var weeklyOrders  = new List<int>();

            for (int w = 7; w >= 0; w--)
            {
                var weekStart = now.AddDays(-(w * 7) - (int)now.DayOfWeek).Date;
                var weekEnd   = weekStart.AddDays(7);
                var label     = $"Week of {weekStart:dd MMM}";

                var sales = allOrders
                    .Where(o => o.CreatedAt >= weekStart && o.CreatedAt < weekEnd)
                    .Sum(o => o.TotalAmount);
                var count = allOrders
                    .Count(o => o.CreatedAt >= weekStart && o.CreatedAt < weekEnd);

                weeklyLabels.Add(label);
                weeklySales.Add(sales);
                weeklyOrders.Add(count);
            }

            // ── Summary Stats ─────────────────────────────────────
            ViewBag.TotalRevenue    = allOrders.Sum(o => o.TotalAmount);
            ViewBag.TotalOrders     = allOrders.Count;
            ViewBag.PendingOrders   = allOrders.Count(o => o.Status == "pending");
            ViewBag.DeliveredOrders = allOrders.Count(o => o.Status == "delivered");

            ViewBag.MonthlyLabels  = monthlyLabels;
            ViewBag.MonthlySales   = monthlySales;
            ViewBag.MonthlyOrders  = monthlyOrders;

            ViewBag.WeeklyLabels  = weeklyLabels;
            ViewBag.WeeklySales   = weeklySales;
            ViewBag.WeeklyOrders  = weeklyOrders;

            return View();
        }

        // ─── Hero Slides List ─────────────────────────────────────
        public IActionResult HeroSlides()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var slides = _db.HeroSlides.OrderBy(s => s.SortOrder).ToList();
            return View(slides);
        }

        // ─── Add Hero Slide ───────────────────────────────────────
        [HttpGet]
        public IActionResult AddSlide()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            return View(new HeroSlide());
        }

        [HttpPost]
        public IActionResult AddSlide(HeroSlide slide, IFormFile? imageFile)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = "hero_" + Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine("wwwroot", "images", fileName);
                using var stream = System.IO.File.Create(path);
                imageFile.CopyTo(stream);
                slide.ImagePath = "/images/" + fileName;
            }

            _db.HeroSlides.Add(slide);
            _db.SaveChanges();
            TempData["Success"] = "Hero slide added!";
            return RedirectToAction("HeroSlides");
        }

        // ─── Edit Hero Slide ──────────────────────────────────────
        [HttpGet]
        public IActionResult EditSlide(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var slide = _db.HeroSlides.FirstOrDefault(s => s.Id == id);
            if (slide == null) return NotFound();
            return View(slide);
        }

        [HttpPost]
        public IActionResult EditSlide(HeroSlide updated, IFormFile? imageFile)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var slide = _db.HeroSlides.FirstOrDefault(s => s.Id == updated.Id);
            if (slide == null) return NotFound();

            slide.Tagline       = updated.Tagline;
            slide.Title         = updated.Title;
            slide.CategoryLabel = updated.CategoryLabel;
            slide.Hashtag       = updated.Hashtag;
            slide.LinkUrl       = updated.LinkUrl;
            slide.SortOrder     = updated.SortOrder;
            slide.IsActive      = updated.IsActive;

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = "hero_" + Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine("wwwroot", "images", fileName);
                using var stream = System.IO.File.Create(path);
                imageFile.CopyTo(stream);
                slide.ImagePath = "/images/" + fileName;
            }

            _db.SaveChanges();
            TempData["Success"] = "Slide updated!";
            return RedirectToAction("HeroSlides");
        }

        // ─── Delete Hero Slide ────────────────────────────────────
        [HttpPost]
        public IActionResult DeleteSlide(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var slide = _db.HeroSlides.FirstOrDefault(s => s.Id == id);
            if (slide != null) { _db.HeroSlides.Remove(slide); _db.SaveChanges(); }
            TempData["Success"] = "Slide deleted.";
            return RedirectToAction("HeroSlides");
        }

        // ─── Toggle Active ────────────────────────────────────────
        [HttpPost]
        public IActionResult ToggleSlide(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var slide = _db.HeroSlides.FirstOrDefault(s => s.Id == id);
            if (slide != null)
            {
                slide.IsActive = !slide.IsActive;
                _db.SaveChanges();
            }
            return RedirectToAction("HeroSlides");
        }
    }
}
