using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Services;
using System.IO;
using System.Text.Json;

namespace ShoppingApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly IGeminiChatService _geminiService;
        private readonly SiteContextService _siteContext;

        public ChatController(IGeminiChatService geminiService, SiteContextService siteContext)
        {
            _geminiService = geminiService;
            _siteContext = siteContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage()
        {
            string message = string.Empty;
            
            // Read raw body (with buffering enabled in Program.cs)
            Request.EnableBuffering();
            Request.Body.Position = 0;
            using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
                
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(body);
                        if (jsonDoc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            if (jsonDoc.RootElement.TryGetProperty("message", out var msgProp))
                            {
                                message = msgProp.GetString() ?? string.Empty;
                            }
                        }
                    }
                    catch
                    {
                        // If parsing fails, try treating body as plain string
                        message = body.Trim('"');
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(message))
                return Json(new { reply = "Please type a message." });

            try
            {
                var siteContext = await _siteContext.BuildSiteContextAsync(message);
                var reply = await _geminiService.GetReplyAsync(message, siteContext);
                return Json(new { reply });
            }
            catch (Exception)
            {
                return Json(new { reply = "Sorry, something went wrong. Please try again." });
            }
        }
    }
}