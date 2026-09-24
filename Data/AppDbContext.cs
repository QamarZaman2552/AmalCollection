using Microsoft.EntityFrameworkCore;
using ShoppingApp.Models;

namespace ShoppingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ChatbotLog> ChatbotLogs { get; set; }
        public DbSet<BrowseHistory> BrowseHistories { get; set; }
        public DbSet<HeroSlide> HeroSlides { get; set; } = null!;
        public DbSet<PromotionalCard> PromotionalCards { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique indexes & constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Category)
                .HasDatabaseName("IX_Products_Category");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Brand)
                .HasDatabaseName("IX_Products_Brand");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Season)
                .HasDatabaseName("IX_Products_Season");

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name");

            modelBuilder.Entity<ProductImage>()
                .HasIndex(i => new { i.ProductId, i.SortOrder })
                .HasDatabaseName("IX_ProductImages_ProductId_SortOrder");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Orders_UserId");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IX_Orders_CreatedAt");

            // Unique cart row per user+product+size+color (variants allowed)
            modelBuilder.Entity<CartItem>()
                .Property(c => c.Size)
                .HasDefaultValue("");
            modelBuilder.Entity<CartItem>()
                .Property(c => c.Color)
                .HasDefaultValue("");
            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.ProductId, c.Size, c.Color })
                .IsUnique()
                .HasDatabaseName("IX_CartItems_UserId_ProductId_Size_Color");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Size)
                .HasDefaultValue("");
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Color)
                .HasDefaultValue("");

            modelBuilder.Entity<WishlistItem>()
                .HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_WishlistItems_UserId_ProductId");

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_Reviews_UserId_ProductId");

            modelBuilder.Entity<ChatbotLog>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("IX_ChatbotLogs_UserId");

            modelBuilder.Entity<BrowseHistory>()
                .HasIndex(b => new { b.UserId, b.ProductId, b.ViewedAt })
                .HasDatabaseName("IX_BrowseHistory_UserId_ProductId_ViewedAt");

            modelBuilder.Entity<ContactMessage>()
                .HasIndex(c => c.CreatedAt)
                .HasDatabaseName("IX_ContactMessages_CreatedAt");

            modelBuilder.Entity<HeroSlide>()
                .HasIndex(h => new { h.SortOrder, h.IsActive })
                .HasDatabaseName("IX_HeroSlides_SortOrder_IsActive");

            modelBuilder.Entity<PromotionalCard>()
                .HasIndex(c => new { c.SortOrder, c.IsActive })
                .HasDatabaseName("IX_PromotionalCards_SortOrder_IsActive");

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(t => new { t.UserId, t.Token })
                .HasDatabaseName("IX_PasswordResetTokens_UserId_Token");

            // Seed admin user (password: Admin@123)
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "Admin",
                Email = "admin@shop.com",
                PasswordHash = "$2a$11$LX99SXQPAaze0rMQSZTxGuPW0GuKLzFfSWd287Vdcr66oy3xFXLYm", // BCrypt hash of "Admin@123"
                Role = "admin",
                Phone = "0300-0000000",
                CreatedAt = new DateTime(2026, 3, 1)
            });

            // Seed categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Summer Suit", SortOrder = 1, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Category { Id = 2, Name = "Winter Suit", SortOrder = 2, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) }
            );

            // Seed site settings (single row)
            modelBuilder.Entity<SiteSettings>().HasData(new SiteSettings
            {
                Id = 1,
                IsFreeDelivery = true,
                DeliveryCharge = 0,
                FreeDeliveryThreshold = 0,
                CodEnabled = true,
                ContactPhone = "0300-0000000",
                ContactWhatsapp = "0321-6068091",
                ContactEmail = "hello@amalcollection.pk"
            });

            // Seed sample ladies-suit products (same Ids 1-10 as before)
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Printed Lawn 3-Piece Suit", Description = "Premium printed lawn 3-piece with dupatta. Perfect for summer.", Price = 2499, Discount = 10, Stock = 40, Category = "Summer Suit", Brand = "Amal", ImagePath = "/images/suit-summer-1.jpg", Season = "Summer", Fabric = "Lawn", Sizes = "XS,S,M,L,XL,XXL", Colors = "Sand,Charcoal", StitchedType = "Stitched", Pieces = 3, IsFreeDelivery = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 2, Name = "Embroidered Cotton Suit", Description = "Hand-embroidered cotton 2-piece, breathable summer fabric.", Price = 2999, Discount = 0, Stock = 30, Category = "Summer Suit", Brand = "Amal", ImagePath = "/images/suit-summer-2.jpg", Season = "Summer", Fabric = "Cotton", Sizes = "S,M,L,XL", Colors = "White,Olive", StitchedType = "Semi-Stitched", Pieces = 2, IsFreeDelivery = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 3, Name = "Chiffon Formal Suit", Description = "Elegant chiffon formal with heavy embroidery. Wedding ready.", Price = 4999, Discount = 15, Stock = 20, Category = "Summer Suit", Brand = "Amal", ImagePath = "/images/suit-summer-3.jpg", Season = "Summer", Fabric = "Chiffon", Sizes = "S,M,L,XL", Colors = "Royal Blue,Rose", StitchedType = "Stitched", Pieces = 3, IsFreeDelivery = false, DeliveryCharge = 200, CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 4, Name = "Khaddar Winter Suit", Description = "Warm khaddar 3-piece for winter. Includes shawl.", Price = 3499, Discount = 5, Stock = 35, Category = "Winter Suit", Brand = "Amal", ImagePath = "/images/suit-winter-1.jpg", Season = "Winter", Fabric = "Khaddar", Sizes = "XS,S,M,L,XL,XXL", Colors = "Charcoal,Brown", StitchedType = "Stitched", Pieces = 3, IsFreeDelivery = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 5, Name = "Wool Shawl Suit", Description = "Premium wool suit with matching shawl. Cold weather essential.", Price = 5999, Discount = 8, Stock = 15, Category = "Winter Suit", Brand = "Amal", ImagePath = "/images/suit-winter-2.jpg", Season = "Winter", Fabric = "Wool", Sizes = "S,M,L,XL", Colors = "Black,Maroon", StitchedType = "Stitched", Pieces = 3, IsFreeDelivery = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 6, Name = "Unstitched Lawn Suit", Description = "3-piece unstitched lawn. Customise your own fit.", Price = 1999, Discount = 0, Stock = 60, Category = "Summer Suit", Brand = "Amal", ImagePath = "/images/suit-summer-4.jpg", Season = "Summer", Fabric = "Lawn", Sizes = "Unstitched", Colors = "Peach,Mint,Cream", StitchedType = "Unstitched", Pieces = 3, IsFreeDelivery = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 7, Name = "Cambric Winter Suit", Description = "Soft cambric 2-piece with printed dupatta.", Price = 2799, Discount = 5, Stock = 45, Category = "Winter Suit", Brand = "Amal", ImagePath = "/images/suit-winter-3.jpg", Season = "Winter", Fabric = "Cambric", Sizes = "XS,S,M,L,XL", Colors = "Teal,Beige", StitchedType = "Semi-Stitched", Pieces = 2, IsFreeDelivery = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 8, Name = "Jacquard Formal Suit", Description = "Rich jacquard weave formal 3-piece. Festive collection.", Price = 6499, Discount = 20, Stock = 12, Category = "Winter Suit", Brand = "Amal", ImagePath = "/images/suit-winter-4.jpg", Season = "Winter", Fabric = "Jacquard", Sizes = "S,M,L,XL,XXL", Colors = "Gold,Navy", StitchedType = "Stitched", Pieces = 3, IsFreeDelivery = false, DeliveryCharge = 250, CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 9, Name = "Cotton Net Summer Suit", Description = "Lightweight cotton net with inner slip. Summer party wear.", Price = 3999, Discount = 0, Stock = 25, Category = "Summer Suit", Brand = "Amal", ImagePath = "/images/suit-summer-5.jpg", Season = "Summer", Fabric = "Cotton", Sizes = "S,M,L,XL", Colors = "Lavender,Ivory", StitchedType = "Stitched", Pieces = 3, IsFreeDelivery = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 10, Name = "Khaddar Unstitched Winter", Description = "Unstitched khaddar 3-piece with warm shawl.", Price = 2599, Discount = 10, Stock = 50, Category = "Winter Suit", Brand = "Amal", ImagePath = "/images/suit-winter-5.jpg", Season = "Winter", Fabric = "Khaddar", Sizes = "Unstitched", Colors = "Grey,Rust", StitchedType = "Unstitched", Pieces = 3, IsFreeDelivery = true, CreatedAt = new DateTime(2024, 1, 1) }
            );

            // Cover gallery rows (SortOrder 0) for product detail thumbs
            modelBuilder.Entity<ProductImage>().HasData(
                new ProductImage { Id = 1,  ProductId = 1,  ImagePath = "/images/suit-summer-1.jpg", SortOrder = 0 },
                new ProductImage { Id = 2,  ProductId = 2,  ImagePath = "/images/suit-summer-2.jpg", SortOrder = 0 },
                new ProductImage { Id = 3,  ProductId = 3,  ImagePath = "/images/suit-summer-3.jpg", SortOrder = 0 },
                new ProductImage { Id = 4,  ProductId = 4,  ImagePath = "/images/suit-winter-1.jpg", SortOrder = 0 },
                new ProductImage { Id = 5,  ProductId = 5,  ImagePath = "/images/suit-winter-2.jpg", SortOrder = 0 },
                new ProductImage { Id = 6,  ProductId = 6,  ImagePath = "/images/suit-summer-4.jpg", SortOrder = 0 },
                new ProductImage { Id = 7,  ProductId = 7,  ImagePath = "/images/suit-winter-3.jpg", SortOrder = 0 },
                new ProductImage { Id = 8,  ProductId = 8,  ImagePath = "/images/suit-winter-4.jpg", SortOrder = 0 },
                new ProductImage { Id = 9,  ProductId = 9,  ImagePath = "/images/suit-summer-5.jpg", SortOrder = 0 },
                new ProductImage { Id = 10, ProductId = 10, ImagePath = "/images/suit-winter-5.jpg", SortOrder = 0 }
            );

            // Homepage hero slides are installed at startup (Program.cs) to avoid PK conflicts
            // with any leftover BaazWix slides still in the live database.
        }
    }
}
