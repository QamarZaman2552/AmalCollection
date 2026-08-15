using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaleAnalytics");

            migrationBuilder.DropIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_UserId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_BrowseHistories_UserId",
                table: "BrowseHistories");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId_ProductId",
                table: "WishlistItems",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Brand",
                table: "Products",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HeroSlides_SortOrder_IsActive",
                table: "HeroSlides",
                columns: new[] { "SortOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_CreatedAt",
                table: "ContactMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrowseHistory_UserId_ProductId_ViewedAt",
                table: "BrowseHistories",
                columns: new[] { "UserId", "ProductId", "ViewedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WishlistItems_UserId_ProductId",
                table: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Products_Brand",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Category",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_HeroSlides_SortOrder_IsActive",
                table: "HeroSlides");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_CreatedAt",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_UserId_ProductId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_BrowseHistory_UserId_ProductId_ViewedAt",
                table: "BrowseHistories");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SaleAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderCount = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalSales = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleAnalytics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId",
                table: "CartItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BrowseHistories_UserId",
                table: "BrowseHistories",
                column: "UserId");
        }
    }
}
