namespace ShoppingApp.Services
{
    public interface IGeminiChatService
    {
        Task<string> GetReplyAsync(string userMessage, string siteContext);
    }
}
