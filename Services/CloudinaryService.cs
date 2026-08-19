using System.Text.Json;

namespace ShoppingApp.Services
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string UploadPreset { get; set; } = string.Empty;
    }

    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file, string prefix = "");
    }

    /// <summary>
    /// Uploads images to Cloudinary (free cloud storage).
    /// Works on Railway where the local filesystem is ephemeral.
    /// Falls back to local storage if Cloudinary is not configured.
    /// </summary>
    public class CloudinaryImageService : IImageService
    {
        private readonly CloudinarySettings _settings;
        private readonly HttpClient _http;

        public CloudinaryImageService(CloudinarySettings settings, HttpClient http)
        {
            _settings = settings;
            _http = http;
        }

        public async Task<string> UploadAsync(IFormFile file, string prefix = "")
        {
            var url = $"https://api.cloudinary.com/v1_1/{_settings.CloudName}/image/upload";

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);
            content.Add(new StringContent(_settings.UploadPreset), "upload_preset");

            if (!string.IsNullOrEmpty(prefix))
                content.Add(new StringContent(prefix), "folder");

            var response = await _http.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("secure_url").GetString() ?? "";
        }
    }

    /// <summary>
    /// Fallback: saves images to wwwroot/images/ (local storage).
    /// Only used when Cloudinary is not configured.
    /// </summary>
    public class LocalImageService : IImageService
    {
        public async Task<string> UploadAsync(IFormFile file, string prefix = "")
        {
            var fileName = (string.IsNullOrEmpty(prefix) ? "" : prefix + "_")
                         + Guid.NewGuid()
                         + Path.GetExtension(file.FileName);
            var path = Path.Combine("wwwroot", "images", fileName);
            using var stream = System.IO.File.Create(path);
            await file.CopyToAsync(stream);
            return "/images/" + fileName;
        }
    }
}
