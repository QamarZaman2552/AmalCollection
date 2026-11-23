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
        public IActionResult Error(int? statusCode = null)
        {
            var code = statusCode is >= 400 and <= 599 ? statusCode.Value : 500;
            if (Response.StatusCode < 400) Response.StatusCode = code;

            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = code
            };

            // Support details so a stuck visitor can reach a human from the error page.
            // Never let a broken database turn the error page into another error.
            try
            {
                var settings = _db.SiteSettings.FirstOrDefault(s => s.Id == 1);
                if (settings != null)
                {
                    if (!string.IsNullOrWhiteSpace(settings.ContactPhone) && !settings.ContactPhone.Contains("0000000"))
                        model.SupportPhone = settings.ContactPhone;
                    if (!string.IsNullOrWhiteSpace(settings.ContactEmail))
                        model.SupportEmail = settings.ContactEmail;

                    var digits = new string((settings.ContactWhatsapp ?? "").Where(char.IsDigit).ToArray());
                    if (digits.Length == 0) digits = new string(model.SupportPhone.Where(char.IsDigit).ToArray());
                    if (digits.StartsWith("0") && !digits.StartsWith("92")) digits = "92" + digits[1..];
                    model.SupportWhatsapp = digits;
                }
            }
            catch
            {
                // Keep the default contact details.
            }

            return View(model);
        }
    }
}
