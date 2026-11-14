namespace ShoppingApp.Services
{
    /// <summary>
    /// Builds responsive image URLs. Local uploads are served through /img/{width}/…
    /// (resized + WebP on demand); remote Cloudinary URLs get f_auto,q_auto transforms.
    /// </summary>
    public static class ImageUrl
    {
        /// <summary>Widths for product cards and small thumbnails.</summary>
        public static readonly int[] CardWidths = { 240, 360, 480, 720 };

        /// <summary>Widths for the product detail gallery.</summary>
        public static readonly int[] DetailWidths = { 480, 800, 1200 };

        /// <summary>Widths for wide banners (hero slides, promo cards).</summary>
        public static readonly int[] BannerWidths = { 640, 960, 1440, 1920 };

        public const string Placeholder = "/images/placeholder.jpg";

        public static string Src(string? path, int width = 640)
        {
            path = string.IsNullOrWhiteSpace(path) ? Placeholder : path;
            if (!IsLocalRaster(path)) return OptimizeRemote(path, width);
            return $"/img/{width}{path}";
        }

        /// <summary>srcset value for a local upload; empty string when the image cannot be resized.</summary>
        public static string SrcSet(string? path, int[]? widths = null)
        {
            path = string.IsNullOrWhiteSpace(path) ? Placeholder : path;
            if (!IsLocalRaster(path)) return "";
            widths ??= CardWidths;
            return string.Join(", ", System.Linq.Enumerable.Select(widths, w => $"/img/{w}{path} {w}w"));
        }

        /// <summary>Adds format/quality (and optional width) transforms to a Cloudinary URL.</summary>
        public static string OptimizeRemote(string? url, int width = 0)
        {
            if (string.IsNullOrWhiteSpace(url)) return Placeholder;
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;
            var marker = "/image/upload/";
            var idx = url.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return url;

            var head = url[..(idx + marker.Length)];
            var tail = url[(idx + marker.Length)..];
            // Do not stack transforms on URLs that already carry one.
            if (tail.Contains("f_auto") || tail.Contains("q_auto")) return url;

            var transform = width > 0
                ? $"f_auto,q_auto,c_limit,w_{width}/"
                : "f_auto,q_auto/";
            return head + transform + tail;
        }

        private static bool IsLocalRaster(string path)
        {
            if (!path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)) return false;
            var ext = System.IO.Path.GetExtension(path);
            return ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".webp", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase);
        }
    }
}
