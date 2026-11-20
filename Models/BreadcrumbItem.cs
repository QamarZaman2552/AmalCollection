namespace ShoppingApp.Models
{
    /// <summary>
    /// One step in the breadcrumb trail rendered by Views/Shared/_Breadcrumb.cshtml.
    /// The last item is treated as the current page, so its Url can stay null.
    /// </summary>
    public class BreadcrumbItem
    {
        public string Label { get; set; } = string.Empty;
        public string? Url { get; set; }

        public BreadcrumbItem() { }

        public BreadcrumbItem(string label, string? url = null)
        {
            Label = label;
            Url = url;
        }
    }
}
