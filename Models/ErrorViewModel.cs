namespace ShoppingApp.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public int? StatusCode { get; set; }
        public bool IsNotFound => StatusCode == 404;

        // A reference number only helps when support has to look the failure up.
        // For a plain 404 the URL itself is the information.
        public bool ShowRequestId => !IsNotFound && !string.IsNullOrEmpty(RequestId);

        // Support details shown on the error page, with sensible fallbacks.
        public string SupportPhone { get; set; } = "0321-6068091";
        public string SupportEmail { get; set; } = "hello@amalcollection.pk";
        public string SupportWhatsapp { get; set; } = "";
        public bool HasWhatsapp => !string.IsNullOrEmpty(SupportWhatsapp);
    }
}
