using System.Security.Cryptography;
using System.Text;
using SkiaSharp;

namespace ShoppingApp.Services
{
    public interface IImageResizer
    {
        /// <summary>
        /// Returns the absolute path of a cached, resized copy of a local /images/ file.
        /// Null when the path is remote, missing or not a supported raster image.
        /// </summary>
        Task<string?> ResizeAsync(string imagePath, int width, bool preferWebp, CancellationToken ct = default);
    }

    /// <summary>
    /// On-demand image resizing for locally stored uploads. Generated files are cached
    /// under wwwroot/imgcache so each (image, width, format) combination is encoded once.
    /// Uses SkiaSharp (MIT) — no native dependency beyond the Skia binary.
    /// </summary>
    public class ImageResizer : IImageResizer
    {
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".bmp" };

        private readonly IWebHostEnvironment _env;

        public ImageResizer(IWebHostEnvironment env) => _env = env;

        public Task<string?> ResizeAsync(string imagePath, int width, bool preferWebp, CancellationToken ct = default)
            => Task.Run(() => Resize(imagePath, width, preferWebp), ct);

        private string? Resize(string imagePath, int width, bool preferWebp)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return null;
            if (!imagePath.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)) return null;

            var ext = Path.GetExtension(imagePath);
            if (!AllowedExtensions.Contains(ext)) return null;

            var webRoot = Path.GetFullPath(_env.WebRootPath);
            var imagesRoot = Path.Combine(webRoot, "images");
            var source = Path.GetFullPath(Path.Combine(
                webRoot, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            // Never read outside wwwroot/images.
            if (!source.StartsWith(imagesRoot, StringComparison.OrdinalIgnoreCase)) return null;
            if (!File.Exists(source)) return null;

            width = Math.Clamp(width, 64, 2400);

            var isPng = ext.Equals(".png", StringComparison.OrdinalIgnoreCase);
            var format = preferWebp || ext.Equals(".webp", StringComparison.OrdinalIgnoreCase)
                ? SKEncodedImageFormat.Webp
                : isPng ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg;
            var outExt = "." + format.ToString().ToLowerInvariant();

            var cacheDir = Path.Combine(webRoot, "imgcache");
            var stamp = File.GetLastWriteTimeUtc(source).Ticks;
            var key = Hash($"{imagePath}|{stamp}|{width}|{format}");
            var target = Path.Combine(cacheDir, key + outExt);
            if (File.Exists(target)) return target;

            Directory.CreateDirectory(cacheDir);

            using var input = File.OpenRead(source);
            using var codec = SKCodec.Create(input);
            if (codec is null) return null;

            var info = codec.Info;
            using var decoded = SKBitmap.Decode(codec);
            if (decoded is null || decoded.Width == 0 || decoded.Height == 0) return null;

            // Only the four rotations that real cameras produce; mirrored EXIF values
            // are vanishingly rare and fall back to no transform.
            var origin = codec.EncodedOrigin;
            var angle = origin switch
            {
                SKEncodedOrigin.BottomRight => 180,
                SKEncodedOrigin.RightTop => 90,
                SKEncodedOrigin.LeftBottom => 270,
                _ => 0
            };
            var swap = angle is 90 or 270;

            var scale = decoded.Width > width ? width / (double)decoded.Width : 1.0;
            var scaledW = Math.Max(1, (int)Math.Round(decoded.Width * scale));
            var scaledH = Math.Max(1, (int)Math.Round(decoded.Height * scale));
            var outW = swap ? scaledH : scaledW;
            var outH = swap ? scaledW : scaledH;

            using var result = new SKBitmap(new SKImageInfo(outW, outH, SKColorType.Bgra8888, SKAlphaType.Premul));
            using (var canvas = new SKCanvas(result))
            {
                canvas.Clear(format == SKEncodedImageFormat.Jpeg ? SKColors.White : SKColors.Transparent);
                switch (angle)
                {
                    case 90: canvas.Translate(outW, 0); canvas.RotateDegrees(90); break;
                    case 180: canvas.Translate(outW, outH); canvas.RotateDegrees(180); break;
                    case 270: canvas.Translate(0, outH); canvas.RotateDegrees(270); break;
                }
                canvas.Scale((float)scale, (float)scale);
                canvas.DrawBitmap(decoded, 0, 0, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            }

            // Encode to a temp file first so concurrent requests never serve a partial image.
            var temp = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var image = SKImage.FromBitmap(result))
                using (var data = image.Encode(format, format == SKEncodedImageFormat.Png ? 100 : 78))
                {
                    if (data is null) return null;
                    using var output = File.Create(temp);
                    data.SaveTo(output);
                }
                File.Move(temp, target, overwrite: true);
            }
            finally
            {
                if (File.Exists(temp)) File.Delete(temp);
            }

            return target;
        }

        private static string Hash(string value)
        {
            var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes).ToLowerInvariant()[..20];
        }
    }
}
