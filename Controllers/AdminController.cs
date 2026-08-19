using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;
        public AdminController(AppDbContext db, IImageService imageService) { _db = db; _imageService = imageService; }

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product, IFormFile? imageFile)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            if (imageFile != null && imageFile.Length > 0)
            {
                product.ImagePath = await _imageService.UploadAsync(imageFile);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product updated, IFormFile? imageFile)
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
                product.ImagePath = await _imageService.UploadAsync(imageFile);
            }

            _db.SaveChanges();
            TempData["Success"] = "Product updated!";
            return RedirectToAction("Products");
        }

        // ─── Delete Product ───────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
            ViewBag.Products = _db.Products.OrderBy(p => p.Name).ToList();
            return View(new HeroSlide());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSlide(HeroSlide slide, IFormFile? imageFile)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            if (imageFile != null && imageFile.Length > 0)
            {
                slide.ImagePath = await _imageService.UploadAsync(imageFile, "hero");
            }

            // If a product is linked, auto-fill price from that product
            if (slide.ProductId.HasValue && !slide.DisplayPrice.HasValue)
            {
                var prod = _db.Products.FirstOrDefault(p => p.Id == slide.ProductId.Value);
                if (prod != null)
                    slide.DisplayPrice = prod.Price * (1 - prod.Discount / 100m);
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
            ViewBag.Products = _db.Products.OrderBy(p => p.Name).ToList();
            return View(slide);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSlide(HeroSlide updated, IFormFile? imageFile)
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
            slide.ProductId     = updated.ProductId;
            slide.DisplayPrice  = updated.DisplayPrice;

            if (imageFile != null && imageFile.Length > 0)
            {
                slide.ImagePath = await _imageService.UploadAsync(imageFile, "hero");
            }

            _db.SaveChanges();
            TempData["Success"] = "Slide updated!";
            return RedirectToAction("HeroSlides");
        }

        // ─── Delete Hero Slide ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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

        // ─── Users Management (Active/Inactive) ─────────────────
        public IActionResult Users()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var users = _db.Users.OrderByDescending(u => u.Role == "admin").ThenBy(u => u.Id).ToList();
            return View(users);
        }

        // ─── Toggle User Active/Inactive ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleUser(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Users");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId.HasValue && currentUserId.Value == id)
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction("Users");
            }

            user.IsLocked = !user.IsLocked;
            user.FailedAttempts = 0;
            _db.SaveChanges();
            TempData["Success"] = user.IsLocked
                ? $"{user.FullName} has been deactivated."
                : $"{user.FullName} has been activated.";
            return RedirectToAction("Users");
        }

        // ─── Delete User (with all related data) ──────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Users");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId.HasValue && currentUserId.Value == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            var userId = user.Id;

            _db.PasswordResetTokens.RemoveRange(_db.PasswordResetTokens.Where(t => t.UserId == userId));
            _db.CartItems.RemoveRange(_db.CartItems.Where(c => c.UserId == userId));
            _db.WishlistItems.RemoveRange(_db.WishlistItems.Where(w => w.UserId == userId));
            _db.BrowseHistories.RemoveRange(_db.BrowseHistories.Where(b => b.UserId == userId));
            _db.Reviews.RemoveRange(_db.Reviews.Where(r => r.UserId == userId));
            _db.ChatbotLogs.RemoveRange(_db.ChatbotLogs.Where(l => l.UserId == userId));

            var orderIds = _db.Orders.Where(o => o.UserId == userId).Select(o => o.Id).ToList();
            _db.OrderItems.RemoveRange(_db.OrderItems.Where(oi => orderIds.Contains(oi.OrderId)));
            _db.Orders.RemoveRange(_db.Orders.Where(o => o.UserId == userId));

            _db.Users.Remove(user);
            _db.SaveChanges();
            TempData["Success"] = $"{user.FullName} deleted permanently.";
            return RedirectToAction("Users");
        }

        // ─── Contact Messages ─────────────────────────────────────
        public IActionResult Messages()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var messages = _db.ContactMessages.OrderByDescending(m => m.CreatedAt).ToList();
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMessage(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var message = _db.ContactMessages.FirstOrDefault(m => m.Id == id);
            if (message != null)
            {
                _db.ContactMessages.Remove(message);
                _db.SaveChanges();
                TempData["Success"] = "Message deleted successfully!";
            }
            return RedirectToAction("Messages");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkMessageRead(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var message = _db.ContactMessages.FirstOrDefault(m => m.Id == id);
            if (message != null)
            {
                message.IsRead = true;
                _db.SaveChanges();
                TempData["Success"] = "Message marked as read!";
            }
            return RedirectToAction("Messages");
        }
    }
}
