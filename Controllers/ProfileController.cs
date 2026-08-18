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
        private readonly IEmailSender _emailSender;

        public ProfileController(AppDbContext db, AuthService auth, IEmailSender emailSender)
        {
            _db   = db;
            _auth = auth;
            _emailSender = emailSender;
        }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        private bool IsAdmin => HttpContext.Session.GetString("UserRole") == "admin";

        private string SiteUrl =>
            $"{Request.Scheme}://{Request.Host}";

        private string Logo => EmailTemplates.GetLogoUrl(SiteUrl);

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

        // ─── Send OTP for password change (logged-in user) ──────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPasswordOtp()
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
            if (user == null) return NotFound();

            var (otp, error) = _auth.CreateResetOtp(user.Email);
            if (otp == null)
            {
                TempData["Error"] = error ?? "Could not create verification code.";
                return RedirectToAction("Index");
            }

            try
            {
                await _emailSender.SendAsync(
                    user.Email,
                    "BaazWix Password Change Verification Code",
                    EmailTemplates.ResetOtp(user.FullName, otp, Logo, SiteUrl)
                );
            }
            catch
            {
                TempData["Error"] = "Could not send the verification email. Please check your email settings and try again.";
                return RedirectToAction("Index");
            }

            TempData["OtpSent"] = true;
            TempData["Success"] = $"A 6-digit verification code has been sent to {user.Email}. It expires in 10 minutes.";
            return RedirectToAction("Index");
        }

        // ─── Verify OTP + Change Password (logged-in user) ──────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string otp, string newPassword, string confirmPassword)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
            if (user == null) return NotFound();

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8 ||
                !newPassword.Any(char.IsLower) || !newPassword.Any(char.IsUpper) ||
                !newPassword.Any(char.IsDigit) || !newPassword.Any(c => !char.IsLetterOrDigit(c)))
            {
                TempData["Error"] = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a symbol (e.g. !@#$%).";
                return RedirectToAction("Index");
            }

            var (verified, verifyError) = _auth.VerifyResetOtp(user.Email, otp?.Trim() ?? "");
            if (verified == null)
            {
                TempData["Error"] = verifyError ?? "Invalid verification code.";
                return RedirectToAction("Index");
            }

            var (ok, error) = _auth.ResetPasswordForUser(user.Id, newPassword);
            if (!ok)
            {
                TempData["Error"] = error ?? "Could not update password.";
                return RedirectToAction("Index");
            }

            try
            {
                await _emailSender.SendAsync(
                    user.Email,
                    "BaazWix Password Change Confirmation",
                    EmailTemplates.PasswordResetConfirmation(user.FullName, Logo, SiteUrl)
                );
            }
            catch
            {
                // Never block if confirmation email fails
            }

            TempData["OtpSent"] = null;
            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction("Index");
        }

        // Password changes are intentionally handled via /Auth/ForgotPassword
    }
}
