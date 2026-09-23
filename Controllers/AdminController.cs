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

        private void EnsureCategoriesViewBag()
        {
            ViewBag.Categories = _db.Categories.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
        }

        private static readonly string[] AllSizes = { "XS", "S", "M", "L", "XL", "XXL", "Unstitched" };

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
            EnsureCategoriesViewBag();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product, IFormFileCollection? imageFiles, bool IsFreeDelivery = true, decimal? DeliveryCharge = null)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            EnsureCategoriesViewBag();

            product.IsFreeDelivery = IsFreeDelivery;
            product.DeliveryCharge = IsFreeDelivery ? null : DeliveryCharge;

            var files = (imageFiles ?? new FormFileCollection())
                .Where(f => f != null && f.Length > 0)
                .Take(5)
                .ToList();

            if (files.Count > 0)
            {
                var first = await _imageService.UploadAsync(files[0]);
                product.ImagePath = first;
                for (int i = 0; i < files.Count; i++)
                {
                    var path = i == 0 ? first : await _imageService.UploadAsync(files[i]);
                    product.Images.Add(new ProductImage { ImagePath = path, SortOrder = i });
                }
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
            EnsureCategoriesViewBag();
            var product = _db.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product updated, IFormFileCollection? imageFiles, bool IsFreeDelivery = true, decimal? DeliveryCharge = null)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            EnsureCategoriesViewBag();
            var product = _db.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == updated.Id);
            if (product == null) return NotFound();

            product.Name = updated.Name;
            product.Description = updated.Description;
            product.Price = updated.Price;
            product.Discount = updated.Discount;
            product.Stock = updated.Stock;
            product.Category = updated.Category;
            product.Brand = updated.Brand;
            product.Season = updated.Season;
            product.Fabric = updated.Fabric;
            product.Sizes = updated.Sizes;
            product.Colors = updated.Colors;
            product.StitchedType = updated.StitchedType;
            product.Pieces = updated.Pieces;
            product.IsFreeDelivery = IsFreeDelivery;
            product.DeliveryCharge = IsFreeDelivery ? null : DeliveryCharge;

            var files = (imageFiles ?? new FormFileCollection())
                .Where(f => f != null && f.Length > 0)
                .ToList();

            var remaining = 5 - product.Images.Count;
            if (files.Count > 0 && remaining > 0)
            {
                var toUpload = files.Take(remaining).ToList();
                for (int i = 0; i < toUpload.Count; i++)
                {
                    var path = await _imageService.UploadAsync(toUpload[i]);
                    var sort = product.Images.Count == 0 && i == 0
                        ? 0
                        : (product.Images.Any() ? product.Images.Max(x => x.SortOrder) + 1 + i : i);
                    // normalize if no cover yet
                    if (!product.Images.Any(x => x.SortOrder == 0))
                        sort = 0;
                    product.Images.Add(new ProductImage { ImagePath = path, SortOrder = sort });
                }

                var cover = product.Images.OrderBy(i => i.SortOrder).FirstOrDefault();
                if (cover != null)
                    product.ImagePath = cover.ImagePath;
            }

            _db.SaveChanges();
            TempData["Success"] = "Product updated!";
            return RedirectToAction("Edit", new { id = product.Id });
        }

        // ─── Delete Product Image ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProductImage(int imageId)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");

            var img = _db.ProductImages.Include(i => i.Product).FirstOrDefault(i => i.Id == imageId);
            if (img == null || img.Product == null) return RedirectToAction("Products");

            var productId = img.ProductId;
            var wasCover = img.SortOrder == 0 || img.Product.ImagePath == img.ImagePath;

            _db.ProductImages.Remove(img);
            _db.SaveChanges();

            var product = _db.Products.Include(p => p.Images).First(p => p.Id == productId);
            var remaining = product.Images.OrderBy(i => i.SortOrder).ToList();
            if (wasCover)
            {
                product.ImagePath = remaining.FirstOrDefault()?.ImagePath;
                // promote first remaining to cover
                if (remaining.Any())
                {
                    foreach (var im in remaining)
                        if (im.Id != remaining[0].Id && im.SortOrder == 0) im.SortOrder = 1;
                    remaining[0].SortOrder = 0;
                }
            }
            _db.SaveChanges();

            TempData["Success"] = "Image removed.";
            return RedirectToAction("Edit", new { id = productId });
        }

        // ─── Delete Product ───────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var product = _db.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _db.ProductImages.RemoveRange(product.Images);
                _db.Products.Remove(product);
                _db.SaveChanges();
            }
            TempData["Success"] = "Product deleted.";
            return RedirectToAction("Products");
        }

        // ─── Categories CRUD ──────────────────────────────────────
        public IActionResult Categories()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var cats = _db.Categories.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
            ViewBag.ProductCounts = _db.Products
                .Where(p => p.Category != null)
                .GroupBy(p => p.Category!)
                .ToDictionary(g => g.Key, g => g.Count());
            return View(cats);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCategory(string name, int sortOrder = 0)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            name = (name ?? "").Trim();
            if (string.IsNullOrEmpty(name))
            {
                TempData["Error"] = "Category name is required.";
                return RedirectToAction("Categories");
            }
            if (_db.Categories.Any(c => c.Name.ToLower() == name.ToLower()))
            {
                TempData["Error"] = "Category already exists.";
                return RedirectToAction("Categories");
            }
            _db.Categories.Add(new Category { Name = name, SortOrder = sortOrder, IsActive = true });
            _db.SaveChanges();
            TempData["Success"] = "Category added.";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(int id, string name, int sortOrder = 0, bool isActive = true)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var cat = _db.Categories.FirstOrDefault(c => c.Id == id);
            if (cat == null) return RedirectToAction("Categories");

            var newName = (name ?? "").Trim();
            if (string.IsNullOrEmpty(newName))
            {
                TempData["Error"] = "Name required.";
                return RedirectToAction("Categories");
            }

            // If renaming, update products that used old name
            if (!string.Equals(cat.Name, newName, StringComparison.OrdinalIgnoreCase))
            {
                var old = cat.Name;
                var products = _db.Products.Where(p => p.Category == old).ToList();
                foreach (var p in products) p.Category = newName;
            }

            cat.Name = newName;
            cat.SortOrder = sortOrder;
            cat.IsActive = isActive;
            _db.SaveChanges();
            TempData["Success"] = "Category updated.";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var cat = _db.Categories.FirstOrDefault(c => c.Id == id);
            if (cat != null)
            {
                // Clear category from products using it
                var products = _db.Products.Where(p => p.Category == cat.Name).ToList();
                foreach (var p in products) p.Category = null;
                _db.Categories.Remove(cat);
                _db.SaveChanges();
                TempData["Success"] = "Category deleted.";
            }
            return RedirectToAction("Categories");
        }

        // ─── Site Settings ────────────────────────────────────────
        public IActionResult Settings()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var s = _db.SiteSettings.FirstOrDefault(x => x.Id == 1);
            if (s == null)
            {
                s = new SiteSettings { Id = 1 };
                _db.SiteSettings.Add(s);
                _db.SaveChanges();
            }
            return View(s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(SiteSettings model)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Auth");
            var s = _db.SiteSettings.FirstOrDefault(x => x.Id == 1);
            if (s == null)
            {
                s = new SiteSettings { Id = 1 };
                _db.SiteSettings.Add(s);
            }
            s.IsFreeDelivery = model.IsFreeDelivery;
            s.DeliveryCharge = model.DeliveryCharge;
            s.FreeDeliveryThreshold = model.FreeDeliveryThreshold;
            s.CodEnabled = model.CodEnabled;
            s.ContactPhone = model.ContactPhone;
            s.ContactWhatsapp = model.ContactWhatsapp;
            s.ContactEmail = model.ContactEmail;
            _db.SaveChanges();
            TempData["Success"] = "Settings saved.";
            return RedirectToAction("Settings");
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

            var allowed = new[] { "pending", "confirmed", "shipped", "delivered", "cancelled" };
            if (!allowed.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction("Orders");
            }

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
