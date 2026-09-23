using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShoppingApp.Migrations
{
    /// <inheritdoc />
    public partial class AmalSuitsAdminFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "Colors",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryCharge",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fabric",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFreeDelivery",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Pieces",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Season",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sizes",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StitchedType",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryCharge",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestPhone",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "CartItems",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "CartItems",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsFreeDelivery = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FreeDeliveryThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CodEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactWhatsapp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Summer Suit", 1 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Winter Suit", 2 }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType", "Stock" },
                values: new object[] { "Amal", "Summer Suit", "Sand,Charcoal", null, "Premium printed lawn 3-piece with dupatta. Perfect for summer.", "Lawn", "/images/suit-summer-1.jpg", true, "Printed Lawn 3-Piece Suit", 3, 2499m, "Summer", "XS,S,M,L,XL,XXL", "Stitched", 40 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Discount", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType", "Stock" },
                values: new object[] { "Amal", "Summer Suit", "White,Olive", null, "Hand-embroidered cotton 2-piece, breathable summer fabric.", 0m, "Cotton", "/images/suit-summer-2.jpg", true, "Embroidered Cotton Suit", 2, 2999m, "Summer", "S,M,L,XL", "Semi-Stitched", 30 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType", "Stock" },
                values: new object[] { "Amal", "Summer Suit", "Royal Blue,Rose", 200m, "Elegant chiffon formal with heavy embroidery. Wedding ready.", "Chiffon", "/images/suit-summer-3.jpg", false, "Chiffon Formal Suit", 3, 4999m, "Summer", "S,M,L,XL", "Stitched", 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Discount", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType" },
                values: new object[] { "Amal", "Winter Suit", "Charcoal,Brown", null, "Warm khaddar 3-piece for winter. Includes shawl.", 5m, "Khaddar", "/images/suit-winter-1.jpg", true, "Khaddar Winter Suit", 3, 3499m, "Winter", "XS,S,M,L,XL,XXL", "Stitched" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType", "Stock" },
                values: new object[] { "Amal", "Winter Suit", "Black,Maroon", null, "Premium wool suit with matching shawl. Cold weather essential.", "Wool", "/images/suit-winter-2.jpg", true, "Wool Shawl Suit", 3, 5999m, "Winter", "S,M,L,XL", "Stitched", 15 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType" },
                values: new object[] { "Amal", "Summer Suit", "Peach,Mint,Cream", null, "3-piece unstitched lawn. Customise your own fit.", "Lawn", "/images/suit-summer-4.jpg", true, "Unstitched Lawn Suit", 3, 1999m, "Summer", "Unstitched", "Unstitched" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType", "Stock" },
                values: new object[] { "Amal", "Winter Suit", "Teal,Beige", null, "Soft cambric 2-piece with printed dupatta.", "Cambric", "/images/suit-winter-3.jpg", true, "Cambric Winter Suit", 2, 2799m, "Winter", "XS,S,M,L,XL", "Semi-Stitched", 45 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType", "Stock" },
                values: new object[] { "Amal", "Winter Suit", "Gold,Navy", 250m, "Rich jacquard weave formal 3-piece. Festive collection.", "Jacquard", "/images/suit-winter-4.jpg", false, "Jacquard Formal Suit", 3, 6499m, "Winter", "S,M,L,XL,XXL", "Stitched", 12 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType", "Stock" },
                values: new object[] { "Amal", "Summer Suit", "Lavender,Ivory", null, "Lightweight cotton net with inner slip. Summer party wear.", "Cotton", "/images/suit-summer-5.jpg", true, "Cotton Net Summer Suit", 3, 3999m, "Summer", "S,M,L,XL", "Stitched", 25 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Brand", "Category", "Colors", "DeliveryCharge", "Description", "Fabric", "ImagePath", "IsFreeDelivery", "Name", "Pieces", "Price", "Season", "Sizes", "StitchedType", "Stock" },
                values: new object[] { "Amal", "Winter Suit", "Grey,Rust", null, "Unstitched khaddar 3-piece with warm shawl.", "Khaddar", "/images/suit-winter-5.jpg", true, "Khaddar Unstitched Winter", 3, 2599m, "Winter", "Unstitched", "Unstitched", 50 });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "CodEnabled", "ContactEmail", "ContactPhone", "ContactWhatsapp", "DeliveryCharge", "FreeDeliveryThreshold", "IsFreeDelivery" },
                values: new object[] { 1, true, "hello@amalcollection.pk", "0300-0000000", "", 0m, 0m, true });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Season",
                table: "Products",
                column: "Season");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId_ProductId_Size_Color",
                table: "CartItems",
                columns: new[] { "UserId", "ProductId", "Size", "Color" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_SortOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_Products_Season",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_UserId_ProductId_Size_Color",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Colors",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeliveryCharge",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Fabric",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFreeDelivery",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Pieces",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Season",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Sizes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StitchedType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeliveryCharge",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestPhone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "CartItems");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Brand", "Category", "Description", "ImagePath", "Name", "Price", "Stock" },
                values: new object[] { "Asus", "Laptops", "High-performance gaming laptop with RTX 4070, 16GB RAM, 512GB SSD.", "/images/laptop1.jpg", "Gaming Laptop Pro", 189999m, 15 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Brand", "Category", "Description", "Discount", "ImagePath", "Name", "Price", "Stock" },
                values: new object[] { "Dell", "Laptops", "Lightweight business laptop, Intel Core i7, 16GB RAM, 1TB SSD.", 5m, "/images/laptop2.jpg", "UltraBook Slim 14", 149999m, 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Brand", "Category", "Description", "ImagePath", "Name", "Price", "Stock" },
                values: new object[] { "Sony", "Audio", "Premium sound, 30hr battery, ANC technology.", "/images/headphones1.jpg", "Wireless Noise-Cancelling Headphones", 29999m, 50 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Brand", "Category", "Description", "Discount", "ImagePath", "Name", "Price" },
                values: new object[] { "Logitech", "Accessories", "RGB backlit, Cherry MX switches, USB-C.", 0m, "/images/keyboard1.jpg", "Mechanical Gaming Keyboard", 12999m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Brand", "Category", "Description", "ImagePath", "Name", "Price", "Stock" },
                values: new object[] { "Samsung", "Monitors", "144Hz refresh rate, 1ms response, HDR400.", "/images/monitor1.jpg", "4K Curved Monitor 27\"", 74999m, 10 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Brand", "Category", "Description", "ImagePath", "Name", "Price" },
                values: new object[] { "Razer", "Accessories", "25,000 DPI, 70hr battery, ultra-lightweight.", "/images/mouse1.jpg", "Wireless Gaming Mouse", 9999m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Brand", "Category", "Description", "ImagePath", "Name", "Price", "Stock" },
                values: new object[] { "Samsung", "Phones", "6.7\" AMOLED, 200MP camera, 5000mAh, Snapdragon 8 Gen 3.", "/images/phone1.jpg", "Smartphone X15 Pro", 109999m, 25 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Brand", "Category", "Description", "ImagePath", "Name", "Price", "Stock" },
                values: new object[] { "Apple", "Audio", "ANC, 36hr total battery, IPX5 waterproof.", "/images/earbuds1.jpg", "True Wireless Earbuds", 14999m, 80 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Brand", "Category", "Description", "ImagePath", "Name", "Price", "Stock" },
                values: new object[] { "Anker", "Accessories", "HDMI 4K, 100W PD, SD/MicroSD, 3x USB-A.", "/images/hub1.jpg", "USB-C Hub 7-in-1", 4999m, 100 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Brand", "Category", "Description", "ImagePath", "Name", "Price", "Stock" },
                values: new object[] { "Samsung", "Storage", "Read 1050MB/s, USB 3.2 Gen 2, shock-proof.", "/images/ssd1.jpg", "Portable SSD 1TB", 19999m, 40 });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
