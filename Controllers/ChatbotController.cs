using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly ChatbotService _chatbot;
        public ChatbotController(ChatbotService chatbot) { _chatbot = chatbot; }

        [HttpPost]
        public IActionResult Chat([FromBody] ChatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Message))
                return Json(new { reply = "Please type a message." });

            var userId = HttpContext.Session.GetInt32("UserId");
            var reply = _chatbot.GetResponse(req.Message, userId);
            return Json(new { reply });
        }
    }

    public class ChatRequest
    {
        public string? Message { get; set; }
    }
}
