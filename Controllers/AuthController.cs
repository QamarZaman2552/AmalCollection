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

        // ─── Register ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string fullName, string email, string password, string confirmPassword, string? phone, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            { ViewBag.Error = "All required fields must be filled."; return View(); }

            if (password != confirmPassword)
            { ViewBag.Error = "Passwords do not match."; return View(); }

            if (password.Length < 6)
            { ViewBag.Error = "Password must be at least 6 characters."; return View(); }

            var user = _auth.Register(fullName, email, password, phone);
            if (user == null)
            { ViewBag.Error = "An account with this email already exists."; return View(); }

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

        // ─── Forgot Password ───────────────────────────────────
        [HttpGet]
        public IActionResult ForgotPassword(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email, string newPassword, string confirmPassword, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            var (ok, error) = _auth.ResetPasswordByEmail(email, newPassword);
            if (!ok)
            {
                ViewBag.Error = error;
                return View();
            }

            try
            {
                await _emailSender.SendAsync(
                    email,
                    "BaazWix Password Reset Confirmation",
                    "Hello,\n\nThis is to confirm that your BaazWix account password was successfully changed.\n\nIf you made this change, no further action is required.\n\nIf you did not request this change, please secure your account immediately by resetting your password and contacting our support team.\n\nRegards,\nBaazWix Security Team"
                );
            }
            catch
            {
                // Do not block user flow if email provider is down.
            }

            TempData["Success"] = "Password updated. You can sign in now.";
            return RedirectToAction("Login", new { returnUrl });
        }
    }
}
