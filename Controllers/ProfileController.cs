using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _db;
        private readonly AuthService  _auth;

        public ProfileController(AppDbContext db, AuthService auth)
        {
            _db   = db;
            _auth = auth;
        }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        private bool IsAdmin => HttpContext.Session.GetString("UserRole") == "admin";

        // ─── View Profile ─────────────────────────────────────
        public IActionResult Index()
        {
            if (UserId == null)
                return RedirectToAction("Login", "Auth", new { returnUrl = "/Profile" });

            var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
            if (user == null) return NotFound();

            ViewBag.ReturnUrl = Request.Query["returnUrl"].ToString();

            // Stats
            ViewBag.TotalOrders    = _db.Orders.Count(o => o.UserId == UserId);
            ViewBag.TotalSpent     = _db.Orders.Where(o => o.UserId == UserId).Sum(o => (decimal?)o.TotalAmount) ?? 0;
            ViewBag.WishlistCount  = _db.WishlistItems.Count(w => w.UserId == UserId);
            ViewBag.ReviewCount    = _db.Reviews.Count(r => r.UserId == UserId);

            return View(user);
        }

        // ─── Update Profile ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(string fullName, string email, string? phone)
        {
            if (UserId == null) return Unauthorized();

            var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Name cannot be empty.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Email cannot be empty.";
                return RedirectToAction("Index");
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var emailTaken = _db.Users.Any(u => u.Id != user.Id && u.Email.ToLower() == normalizedEmail);
            if (emailTaken)
            {
                TempData["Error"] = "This email is already in use by another account.";
                return RedirectToAction("Index");
            }

            user.FullName = fullName.Trim();
            user.Email    = normalizedEmail;
            user.Phone    = phone?.Trim();
            _db.SaveChanges();

            // Update session name
            HttpContext.Session.SetString("UserName", user.FullName);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        // ─── Top up account balance ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TopUp(decimal amount, string? returnUrl)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");
            if (IsAdmin)
            {
                TempData["Error"] = "Admin accounts cannot top up balance.";
                return RedirectToAction("Index");
            }

            if (amount <= 0)
            {
                TempData["Error"] = "Top-up amount must be greater than zero.";
                return RedirectBack(returnUrl);
            }

            if (amount > 5_000_000m)
            {
                TempData["Error"] = "Maximum top-up per transaction is PKR 5,000,000.";
                return RedirectBack(returnUrl);
            }

            var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
            if (user == null) return NotFound();

            user.Balance += amount;
            _db.SaveChanges();

            TempData["Success"] = $"PKR {amount:N0} added to your account. New balance: PKR {user.Balance:N0}.";
            return RedirectBack(returnUrl);
        }

        private IActionResult RedirectBack(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        // Password changes are intentionally handled via /Auth/ForgotPassword
    }
}
