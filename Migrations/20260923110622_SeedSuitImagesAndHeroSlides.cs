using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShoppingApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedSuitImagesAndHeroSlides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "ImagePath", "ProductId", "SortOrder" },
                values: new object[,]
                {
                    { 1, "/images/suit-summer-1.jpg", 1, 0 },
                    { 2, "/images/suit-summer-2.jpg", 2, 0 },
                    { 3, "/images/suit-summer-3.jpg", 3, 0 },
                    { 4, "/images/suit-winter-1.jpg", 4, 0 },
                    { 5, "/images/suit-winter-2.jpg", 5, 0 },
                    { 6, "/images/suit-summer-4.jpg", 6, 0 },
                    { 7, "/images/suit-winter-3.jpg", 7, 0 },
                    { 8, "/images/suit-winter-4.jpg", 8, 0 },
                    { 9, "/images/suit-summer-5.jpg", 9, 0 },
                    { 10, "/images/suit-winter-5.jpg", 10, 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
