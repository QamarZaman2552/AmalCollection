namespace ShoppingApp.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public int SortOrder { get; set; } = 0; // 0 = cover

        public Product? Product { get; set; }
    }
}
