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
            var boundary = "----BaazWixBoundary" + Guid.NewGuid().ToString("N");

            using var body = new MemoryStream();
            void Write(string text)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                body.Write(bytes, 0, bytes.Length);
            }

            var safeName = Path.GetFileName(file.FileName)
                .Replace("\"", "")
                .Replace("\r", "")
                .Replace("\n", "");
            var fileType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType.Replace("\"", "");

            Write("--" + boundary + "\r\n");
            Write($"Content-Disposition: form-data; name=\"file\"; filename=\"{safeName}\"\r\n");
            Write($"Content-Type: {fileType}\r\n\r\n");

            using (var stream = file.OpenReadStream())
                await stream.CopyToAsync(body);

            Write("\r\n--" + boundary + "\r\n");
            Write("Content-Disposition: form-data; name=\"upload_preset\"\r\n\r\n");
            Write(_settings.UploadPreset);
            Write("\r\n--" + boundary + "--\r\n");

            var content = new ByteArrayContent(body.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
            content.Headers.ContentType.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue("boundary", boundary));

            var response = await _http.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Cloudinary upload failed ({(int)response.StatusCode}): {errBody}");
            }

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
