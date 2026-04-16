using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth)
        {
            _auth = auth;
        }

        // ─── Register ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string fullName, string email, string password, string confirmPassword, string? phone)
        {
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
            return RedirectToAction("Index", "Products");
        }

        // ─── Login ──────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var (user, error) = _auth.Login(email, password);
            if (user == null)
            { ViewBag.Error = error; return View(); }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

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
    }
}
