using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _auth;
        private readonly IEmailSender _emailSender;

        public AuthController(AuthService auth, IEmailSender emailSender)
        {
            _auth = auth;
            _emailSender = emailSender;
        }

        private string SiteUrl =>
            $"{Request.Scheme}://{Request.Host}";

        private string Logo => EmailTemplates.GetLogoUrl(SiteUrl);

        // ─── Register ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword, string? phone, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            { ViewBag.Error = "All required fields must be filled."; return View(); }

            if (password != confirmPassword)
            { ViewBag.Error = "Passwords do not match."; return View(); }

            if (!IsStrongPassword(password))
            { ViewBag.Error = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a symbol (e.g. !@#$%)."; return View(); }

            var user = _auth.Register(fullName, email, password, phone);
            if (user == null)
            { ViewBag.Error = "An account with this email already exists."; return View(); }

            try
            {
                await _emailSender.SendAsync(
                    user.Email,
                    "Welcome to BaazWix!",
                    EmailTemplates.Welcome(user.FullName, Logo, SiteUrl)
                );
            }
            catch
            {
                // Never block registration if email fails
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Products");
        }

        // ─── Login ──────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            // Rate limiting: check IP-based lockout
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var lockoutKey = $"LoginLockout_{ip}";
            var attemptKey = $"LoginAttempts_{ip}";
            var lockoutUntil = HttpContext.Session.GetString(lockoutKey);
            if (lockoutUntil != null && DateTime.TryParse(lockoutUntil, out var until) && until > DateTime.UtcNow)
            {
                var remainingSec = (int)(until - DateTime.UtcNow).TotalSeconds;
                ViewBag.Error = $"Too many failed attempts. Please try again in {remainingSec} seconds.";
                return View();
            }

            var (user, error) = _auth.Login(email, password);
            if (user == null)
            {
                // Track failed attempts per IP
                var attemptsStr = HttpContext.Session.GetString(attemptKey) ?? "0";
                int attempts = int.TryParse(attemptsStr, out var a) ? a : 0;
                attempts++;
                HttpContext.Session.SetString(attemptKey, attempts.ToString());

                if (attempts >= 5)
                {
                    HttpContext.Session.SetString(lockoutKey, DateTime.UtcNow.AddMinutes(5).ToString("o"));
                    HttpContext.Session.SetString(attemptKey, "0");
                    ViewBag.Error = "Too many failed attempts. Locked out for 5 minutes.";
                }
                else
                {
                    int remaining = 5 - attempts;
                    ViewBag.Error = $"{error} ({remaining} attempt(s) remaining)";
                }
                return View();
            }

            // Success - clear lockout tracking and old session data
            HttpContext.Session.Remove(lockoutKey);
            HttpContext.Session.Remove(attemptKey);
            HttpContext.Session.Clear();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (user.Role == "admin")
                return RedirectToAction("Products", "Admin");

            return RedirectToAction("Index", "Products");
        }

        // ─── Logout ─────────────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ─── Forgot Password (Step 1: Request OTP) ──────────────
        [HttpGet]
        public IActionResult ForgotPassword(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Email is required.";
                return View();
            }

            var (otp, error) = _auth.CreateResetOtp(email.Trim());
            if (otp == null)
            {
                ViewBag.Error = error;
                return View();
            }

            var user = _auth.GetByEmail(email.Trim());
            var userName = user?.FullName ?? email.Trim();

            try
            {
                await _emailSender.SendAsync(
                    email.Trim(),
                    "BaazWix Password Reset Verification Code",
                    EmailTemplates.ResetOtp(userName, otp, Logo, SiteUrl)
                );
            }
            catch
            {
                ViewBag.Error = "Could not send the verification email. Please try again later.";
                return View();
            }

            TempData["ResetEmail"] = email.Trim();
            TempData["Success"] = $"A 6-digit verification code has been sent to {email.Trim()}. It expires in 10 minutes.";
            return RedirectToAction("VerifyOtp", new { returnUrl });
        }

        // ─── Verify OTP (Step 2) ────────────────────────────────
        [HttpGet]
        public IActionResult VerifyOtp(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Email = TempData["ResetEmail"] as string;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(string email, string otp, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Email = email;

            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Email is required.";
                return View();
            }

            var (user, error) = _auth.VerifyResetOtp(email.Trim(), otp?.Trim() ?? "");
            if (user == null)
            {
                ViewBag.Error = error;
                return View();
            }

            TempData["ResetUserId"] = user.Id;
            TempData["Success"] = "Verification successful. Now set your new password.";
            return RedirectToAction("ResetPassword", new { returnUrl });
        }

        // ─── Set New Password (Step 3) ──────────────────────────
        [HttpGet]
        public IActionResult ResetPassword(string? returnUrl = null)
        {
            if (TempData["ResetUserId"] == null)
                return RedirectToAction("ForgotPassword", new { returnUrl });

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (TempData["ResetUserId"] == null)
                return RedirectToAction("ForgotPassword", new { returnUrl });

            var userId = (int)TempData["ResetUserId"];

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            if (!IsStrongPassword(newPassword))
            { ViewBag.Error = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a number, and a symbol (e.g. !@#$%)."; return View(); }

            var (ok, error) = _auth.ResetPasswordForUser(userId, newPassword);
            if (!ok)
            {
                ViewBag.Error = error;
                return View();
            }

            var user = _auth.GetById(userId);
            var userEmail = user?.Email ?? "";
            var userName = user?.FullName ?? userEmail;

            try
            {
                await _emailSender.SendAsync(
                    userEmail,
                    "BaazWix Password Reset Confirmation",
                    EmailTemplates.PasswordResetConfirmation(userName, Logo, SiteUrl)
                );
            }
            catch
            {
                // Do not block user flow if email provider is down.
            }

            TempData["Success"] = "Password updated. You can sign in now.";
            return RedirectToAction("Login", new { returnUrl });
        }

        private static bool IsStrongPassword(string pw)
        {
            if (string.IsNullOrWhiteSpace(pw) || pw.Length < 8) return false;
            if (!pw.Any(char.IsLower)) return false;
            if (!pw.Any(char.IsUpper)) return false;
            if (!pw.Any(char.IsDigit)) return false;
            if (!pw.Any(c => !char.IsLetterOrDigit(c))) return false;
            return true;
        }
    }
}
