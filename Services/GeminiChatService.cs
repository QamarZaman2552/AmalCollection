using Microsoft.Extensions.Options;
using ShoppingApp.Models;
using System.Text;
using System.Text.Json;

namespace ShoppingApp.Services
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiChatService> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public GeminiChatService(HttpClient httpClient, IOptions<GeminiSettings> settings, ILogger<GeminiChatService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> GetReplyAsync(string userMessage, string siteContext)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Please type a message.";

            try
            {
                var url = $"{_settings.BaseUrl}/{_settings.Model}:generateContent?key={_settings.ApiKey}";

                var systemInstruction = new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new() { Text = $@"
You are a website assistant for an e-commerce store called BaazWix. 
Only answer questions related to this website — products, categories, prices, availability, recommendations, and general site info (shipping, policies, FAQs) based on the data provided below.

If the user asks something unrelated to the website (general knowledge, coding, news, math problems, etc.), politely decline: ""Sorry, I can only help with questions about our website and products.""

Whenever you mention a specific product, always include its link in markdown format: [Product Name](/Products/Detail/{{id}})

Site Data:
{siteContext}" }
                    }
                };

                var requestBody = new GeminiRequest
                {
                    SystemInstruction = systemInstruction,
                    Contents = new List<GeminiContent>
                    {
                        new()
                        {
                            Role = "user",
                            Parts = new List<GeminiPart>
                            {
                                new() { Text = userMessage }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody, JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);

                    return response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden =>
                            "Authentication failed. Please check your API key configuration.",
                        System.Net.HttpStatusCode.TooManyRequests =>
                            "Rate limit reached. Please try again in a moment.",
                        System.Net.HttpStatusCode.BadRequest =>
                            "Sorry, I couldn't process that request.",
                        System.Net.HttpStatusCode.NotFound =>
                            "The AI model is not available.",
                        System.Net.HttpStatusCode.ServiceUnavailable =>
                            "The AI service is busy right now. Please try again in a moment.",
                        _ => "I'm having trouble right now, please try again shortly."
                    };
                }

                var responseStream = await response.Content.ReadAsStreamAsync();
                var geminiResponse = await JsonSerializer.DeserializeAsync<GeminiResponse>(responseStream, JsonOptions);

                var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                return string.IsNullOrWhiteSpace(text)
                    ? "I received a response but couldn't process it. Please try again."
                    : text;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error when calling Gemini API");
                return "Sorry, I'm having trouble connecting to the AI service. Please check your network connection and try again.";
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Gemini API request timed out");
                return "The request timed out. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Gemini API");
                return "Sorry, something went wrong. Please try again later.";
            }
        }
    }
}
