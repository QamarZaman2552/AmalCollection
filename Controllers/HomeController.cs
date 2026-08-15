using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Data;

namespace ShoppingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private static readonly Regex SanitizeRegex = new(@"[<>""'&]", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        [Route("About")]
        public IActionResult About()
        {
            return View();
        }

        [Route("Contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Contact/Submit")]
        public IActionResult SubmitContact(string Name, string Email, string Message)
        {
            // Sanitize inputs
            var sanitizedName = SanitizeInput(Name ?? "");
            var sanitizedEmail = SanitizeInput(Email ?? "");
            var sanitizedMessage = SanitizeInput(Message ?? "");

            // Validate email format
            if (!EmailRegex.IsMatch(sanitizedEmail))
            {
                TempData["Error"] = "Invalid email format.";
                return RedirectToAction("Contact");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(sanitizedName) || string.IsNullOrWhiteSpace(sanitizedMessage))
            {
                TempData["Error"] = "Name and message are required.";
                return RedirectToAction("Contact");
            }

            // Length limits
            if (sanitizedName.Length > 100 || sanitizedMessage.Length > 5000)
            {
                TempData["Error"] = "Input too long.";
                return RedirectToAction("Contact");
            }

            var msg = new ContactMessage 
            { 
                Name = sanitizedName,
                Email = sanitizedEmail, 
                Message = sanitizedMessage,
                CreatedAt = System.DateTime.UtcNow,
                IsRead = false
            };
            
            _db.ContactMessages.Add(msg);
            _db.SaveChanges();
            TempData["Success"] = "Thank you! Please check your email in one day for a response from the admin.";
            
            return RedirectToAction("Contact");
        }

        private static string SanitizeInput(string input)
        {
            return SanitizeRegex.Replace(input, string.Empty).Trim();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
