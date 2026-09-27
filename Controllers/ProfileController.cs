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
            // Keep OTP form visible across refresh / failed change-password attempts
            if (TempData["OtpSent"] != null)
            {
                ViewBag.OtpSent = true;
                TempData.Keep("OtpSent");
            }

            // Pending email change (if OTP step is in progress)
            ViewBag.PendingEmail = HttpContext.Session.GetString(PendingEmailKey);

            // Stats
            ViewBag.TotalOrders    = _db.Orders.Count(o => o.UserId == UserId);
            ViewBag.TotalSpent     = _db.Orders.Where(o => o.UserId == UserId).Sum(o => (decimal?)o.TotalAmount) ?? 0;
            ViewBag.WishlistCount  = _db.WishlistItems.Count(w => w.UserId == UserId);
            ViewBag.ReviewCount    = _db.Reviews.Count(r => r.UserId == UserId);

            return View(user);
        }

        // ─── Update Profile (name + phone only; email change is OTP-verified) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(string fullName, string? phone)
        {
            if (UserId == null) return Unauthorized();

            var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Name cannot be empty.";
                return RedirectToAction("Index");
            }

            user.FullName = fullName.Trim();
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
                    "Amal Collection Password Change Verification Code",
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
                TempData.Keep("OtpSent");
                TempData["Error"] = "Passwords do not match.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8 ||
                !newPassword.Any(char.IsLower) || !newPassword.Any(char.IsUpper) ||
                !newPassword.Any(char.IsDigit) || !newPassword.Any(c => !char.IsLetterOrDigit(c)))
            {
                TempData.Keep("OtpSent");
                TempData["Error"] = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a symbol (e.g. !@#$%).";
                return RedirectToAction("Index");
            }

            var (verified, verifyError) = _auth.VerifyResetOtp(user.Email, otp?.Trim() ?? "");
            if (verified == null)
            {
                TempData.Keep("OtpSent");
                TempData["Error"] = verifyError ?? "Invalid verification code.";
                return RedirectToAction("Index");
            }

            var (ok, error) = _auth.ResetPasswordForUser(user.Id, newPassword);
            if (!ok)
            {
                TempData.Keep("OtpSent");
                TempData["Error"] = error ?? "Could not update password.";
                return RedirectToAction("Index");
            }

            try
            {
                await _emailSender.SendAsync(
                    user.Email,
                    "Amal Collection Password Change Confirmation",
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

        // ─── Change Email (OTP verification to CURRENT email) ──
        private const string PendingEmailKey = "EmailChangePending";

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254) return false;
            try
            {
                var e = email.Trim();
                var addr = new System.Net.Mail.MailAddress(e);
                if (addr.Address != e) return false;
                var at  = e.IndexOf('@');
                var dot = e.LastIndexOf('.');
                if (at <= 0 || dot <= at + 1 || dot >= e.Length - 1) return false;
                if (e.Contains("..") || e.Contains(" ") || e.Contains(",")) return false;
                return true;
            }
            catch { return false; }
        }

        // Step 1: send a 6-digit code to the CURRENT email before allowing a change
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailChangeOtp(string newEmail)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
            if (user == null) return NotFound();

            var pending = (newEmail ?? "").Trim().ToLowerInvariant();

            if (!IsValidEmail(pending))
            {
                TempData["Error"] = "Please enter a valid new email address.";
                return RedirectToAction("Index");
            }
            if (pending == user.Email.ToLowerInvariant())
            {
                TempData["Error"] = "That is already your current email address.";
                return RedirectToAction("Index");
            }
            if (_db.Users.Any(u => u.Id != user.Id && u.Email.ToLower() == pending))
            {
                TempData["Error"] = "This email is already in use by another account.";
                return RedirectToAction("Index");
            }

            var (otp, error) = _auth.CreateResetOtp(user.Email); // code goes to CURRENT email
            if (otp == null)
            {
                TempData["Error"] = error ?? "Could not create verification code.";
                return RedirectToAction("Index");
            }

            try
            {
                await _emailSender.SendAsync(
                    user.Email,
                    "Amal Collection Email Change Verification Code",
                    EmailTemplates.EmailChangeOtp(user.FullName, otp, Logo, SiteUrl)
                );
            }
            catch
            {
                TempData["Error"] = "Could not send the verification email. Please try again.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetString(PendingEmailKey, pending);
            TempData["Success"] = $"A 6-digit verification code has been sent to your current email {user.Email}. It expires in 10 minutes.";
            return RedirectToAction("Index");
        }

        // Step 2: verify the OTP (against CURRENT email) and apply the new email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(string otp)
        {
            if (UserId == null) return RedirectToAction("Login", "Auth");

            var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
            if (user == null) return NotFound();

            var pending = HttpContext.Session.GetString(PendingEmailKey);
            if (string.IsNullOrEmpty(pending))
            {
                TempData["Error"] = "No email change is in progress. Please start again.";
                return RedirectToAction("Index");
            }

            var (verified, verifyError) = _auth.VerifyResetOtp(user.Email, otp?.Trim() ?? "");
            if (verified == null)
            {
                TempData["Error"] = verifyError ?? "Invalid verification code.";
                return RedirectToAction("Index");
            }

            // Re-validate at apply time (availability may have changed)
            if (!IsValidEmail(pending) ||
                pending == user.Email.ToLowerInvariant() ||
                _db.Users.Any(u => u.Id != user.Id && u.Email.ToLower() == pending))
            {
                HttpContext.Session.Remove(PendingEmailKey);
                TempData["Error"] = "The new email is no longer available. Please start again.";
                return RedirectToAction("Index");
            }

            var oldEmail = user.Email;
            user.Email = pending;
            _db.SaveChanges();
            HttpContext.Session.Remove(PendingEmailKey);

            // Security notice to the OLD address
            try
            {
                await _emailSender.SendAsync(
                    oldEmail,
                    "Amal Collection Email Address Changed",
                    EmailTemplates.EmailChangedNotice(user.FullName, pending, Logo, SiteUrl)
                );
            }
            catch
            {
                // Never block the change if the notice email fails
            }

            TempData["Success"] = $"Email address changed to {pending}. Use it next time you log in.";
            return RedirectToAction("Index");
        }

        // Cancel a pending email change
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelEmailChange()
        {
            HttpContext.Session.Remove(PendingEmailKey);
            TempData["Success"] = "Email change cancelled.";
            return RedirectToAction("Index");
        }

        // Password changes are intentionally handled via /Auth/ForgotPassword
    }
}
