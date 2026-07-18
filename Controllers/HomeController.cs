using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Data;

namespace ShoppingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

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
            var msg = new ContactMessage 
            { 
               Name = Name ?? "", 
               Email = Email ?? "", 
               Message = Message ?? "",
               CreatedAt = System.DateTime.UtcNow,
               IsRead = false
            };
            
            _db.ContactMessages.Add(msg);
            _db.SaveChanges();
            TempData["Success"] = "Thank you! Please check your email in one day for a response from the admin.";
            
            return RedirectToAction("Contact");
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
