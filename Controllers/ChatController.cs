using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Data;
using ShoppingApp.Models;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly IGeminiChatService _geminiService;
        private readonly SiteContextService _siteContext;
        private readonly ChatbotService _fallback;
        private readonly AppDbContext _db;

        public ChatController(IGeminiChatService geminiService, SiteContextService siteContext,
            ChatbotService fallback, AppDbContext db)
        {
            _geminiService = geminiService;
            _siteContext = siteContext;
            _fallback = fallback;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest req)
        {
            var message = req?.Message ?? string.Empty;
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { reply = "Please type a message." });

            var userId = HttpContext.Session.GetInt32("UserId");

            // Log the incoming query for admin analytics
            try
            {
                _db.ChatbotLogs.Add(new ChatbotLog
                {
                    UserId = userId,
                    Query = message,
                    Response = "..."
                });
                await _db.SaveChangesAsync();
            }
            catch { /* logging must never break chat */ }

            try
            {
                var siteContext = await _siteContext.BuildSiteContextAsync(message);
                var reply = await _geminiService.GetReplyAsync(message, siteContext);

                // If Gemini returned an error-ish/empty reply, fall back to rule-based
                if (string.IsNullOrWhiteSpace(reply) ||
                    reply.StartsWith("Sorry, I'm having trouble") ||
                    reply.StartsWith("The request timed out") ||
                    reply.StartsWith("Sorry, something went wrong") ||
                    reply.StartsWith("Authentication failed") ||
                    reply.StartsWith("Rate limit reached") ||
                    reply.StartsWith("The AI service is busy") ||
                    reply.StartsWith("The AI model is not available") ||
                    reply.StartsWith("I'm having trouble right now") ||
                    reply == "I received a response but couldn't process it. Please try again." ||
                    reply == "Please type a message.")
                {
                    reply = _fallback.GetResponse(message, userId);
                }

                // Update the log with the final response
                try
                {
                    var lastLog = _db.ChatbotLogs.OrderByDescending(l => l.Id).FirstOrDefault(l => l.Query == message);
                    if (lastLog != null)
                    {
                        lastLog.Response = reply;
                        await _db.SaveChangesAsync();
                    }
                }
                catch { /* best effort */ }

                return Json(new { reply });
            }
            catch (Exception)
            {
                var reply = _fallback.GetResponse(message, userId);
                return Json(new { reply });
            }
        }
    }
}