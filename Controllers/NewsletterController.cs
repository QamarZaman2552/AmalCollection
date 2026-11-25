using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly AppDbContext _db;

        public NewsletterController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string? email, string? returnUrl = null)
        {
            var wantsJson = Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                || Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

            email = email?.Trim();
            if (string.IsNullOrEmpty(email) || email.Length > 254 || !new EmailAddressAttribute().IsValid(email))
            {
                return Respond(false, "Enter a valid email address.", wantsJson, returnUrl);
            }

            // Store one canonical row per address; the unique index is the final guard.
            email = email.ToLowerInvariant();
            if (await _db.NewsletterSubscribers.AnyAsync(s => s.Email == email))
            {
                return Respond(true, "You're already on the list — thanks!", wantsJson, returnUrl);
            }

            try
            {
                _db.NewsletterSubscribers.Add(new NewsletterSubscriber
                {
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // A simultaneous signup won the race; the address is on the list either way.
            }

            return Respond(true, "Thanks! You're on the list.", wantsJson, returnUrl);
        }

        private IActionResult Respond(bool ok, string message, bool wantsJson, string? returnUrl)
        {
            if (wantsJson)
            {
                return Json(new { ok, message });
            }

            // No-JS fallback: show the message after the redirect.
            TempData["NewsletterMessage"] = message;
            TempData["NewsletterOk"] = ok;

            var target = Url.IsLocalUrl(returnUrl) ? returnUrl! : "/";
            return Redirect(target + "#newsletter");
        }
    }
}
