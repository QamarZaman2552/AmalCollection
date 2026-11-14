using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    /// <summary>
    /// Serves resized, WebP-optimised copies of local product images.
    /// URLs look like /img/480/images/suit-summer-1.jpg and are immutable per hash,
    /// so they can be cached by the browser for a year.
    /// </summary>
    public class ImageController : Controller
    {
        private readonly IImageResizer _resizer;

        public ImageController(IImageResizer resizer) => _resizer = resizer;

        [HttpGet("/img/{width:int}/{*path}")]
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept")]
        public async Task<IActionResult> Resize(int width, string? path, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(path)) return NotFound();

            var imagePath = "/" + path.TrimStart('/');
            var wantsWebp = Request.Headers.Accept.Any(v => v is not null && v.Contains("image/webp", StringComparison.OrdinalIgnoreCase));

            var file = await _resizer.ResizeAsync(imagePath, width, wantsWebp, ct);
            if (file is null) return NotFound();

            Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            var contentType = Path.GetExtension(file).ToLowerInvariant() switch
            {
                ".webp" => "image/webp",
                ".png" => "image/png",
                _ => "image/jpeg"
            };

            return PhysicalFile(file, contentType);
        }
    }
}
