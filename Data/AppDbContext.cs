using Microsoft.EntityFrameworkCore;
using ShoppingApp.Models;

namespace ShoppingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ChatbotLog> ChatbotLogs { get; set; }
        public DbSet<BrowseHistory> BrowseHistories { get; set; }
        public DbSet<HeroSlide> HeroSlides { get; set; } = null!;
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

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Orders_UserId");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IX_Orders_CreatedAt");

            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_CartItems_UserId_ProductId");

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

            // Seed sample products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Gaming Laptop Pro", Description = "High-performance gaming laptop with RTX 4070, 16GB RAM, 512GB SSD.", Price = 189999, Discount = 10, Stock = 15, Category = "Laptops", Brand = "Asus", ImagePath = "/images/laptop1.jpg", CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 2, Name = "UltraBook Slim 14", Description = "Lightweight business laptop, Intel Core i7, 16GB RAM, 1TB SSD.", Price = 149999, Discount = 5, Stock = 20, Category = "Laptops", Brand = "Dell", ImagePath = "/images/laptop2.jpg", CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 3, Name = "Wireless Noise-Cancelling Headphones", Description = "Premium sound, 30hr battery, ANC technology.", Price = 29999, Discount = 15, Stock = 50, Category = "Audio", Brand = "Sony", ImagePath = "/images/headphones1.jpg", CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 4, Name = "Mechanical Gaming Keyboard", Description = "RGB backlit, Cherry MX switches, USB-C.", Price = 12999, Discount = 0, Stock = 35, Category = "Accessories", Brand = "Logitech", ImagePath = "/images/keyboard1.jpg", CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 5, Name = "4K Curved Monitor 27\"", Description = "144Hz refresh rate, 1ms response, HDR400.", Price = 74999, Discount = 8, Stock = 10, Category = "Monitors", Brand = "Samsung", ImagePath = "/images/monitor1.jpg", CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 6, Name = "Wireless Gaming Mouse", Description = "25,000 DPI, 70hr battery, ultra-lightweight.", Price = 9999, Discount = 0, Stock = 60, Category = "Accessories", Brand = "Razer", ImagePath = "/images/mouse1.jpg", CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 7, Name = "Smartphone X15 Pro", Description = "6.7\" AMOLED, 200MP camera, 5000mAh, Snapdragon 8 Gen 3.", Price = 109999, Discount = 5, Stock = 25, Category = "Phones", Brand = "Samsung", ImagePath = "/images/phone1.jpg", CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 8, Name = "True Wireless Earbuds", Description = "ANC, 36hr total battery, IPX5 waterproof.", Price = 14999, Discount = 20, Stock = 80, Category = "Audio", Brand = "Apple", ImagePath = "/images/earbuds1.jpg", CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 9, Name = "USB-C Hub 7-in-1", Description = "HDMI 4K, 100W PD, SD/MicroSD, 3x USB-A.", Price = 4999, Discount = 0, Stock = 100, Category = "Accessories", Brand = "Anker", ImagePath = "/images/hub1.jpg", CreatedAt = new DateTime(2024, 1, 1) },
                new Product { Id = 10, Name = "Portable SSD 1TB", Description = "Read 1050MB/s, USB 3.2 Gen 2, shock-proof.", Price = 19999, Discount = 10, Stock = 40, Category = "Storage", Brand = "Samsung", ImagePath = "/images/ssd1.jpg", CreatedAt = new DateTime(2024, 1, 1) }
            );
        }
    }
}
