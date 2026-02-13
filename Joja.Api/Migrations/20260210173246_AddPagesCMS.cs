using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Joja.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPagesCMS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VariantsJson",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Subtitle = table.Column<string>(type: "TEXT", nullable: false),
                    LinkUrl = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CategoryTranslations",
                columns: new[] { "Id", "CategoryId", "Language", "Name" },
                values: new object[,]
                {
                    { 1, 1, "en", "Skin Care" },
                    { 2, 2, "en", "Hair Care" },
                    { 3, 3, "en", "Natural Oils" }
                });

            migrationBuilder.InsertData(
                table: "ProductTranslations",
                columns: new[] { "Id", "Description", "Language", "Name", "ProductId" },
                values: new object[,]
                {
                    { 1, "Pure 100% jojoba oil for skin and hair", "en", "Organic Jojoba Oil", 1 },
                    { 2, "Natural shea butter for deep moisturizing", "en", "Natural Shea Butter", 2 },
                    { 3, "Authentic argan oil from Morocco for hair", "en", "Moroccan Argan Oil", 3 },
                    { 4, "Natural aloe cream for skin moisturizing", "en", "Aloe Vera Cream", 4 }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "VariantsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "VariantsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "VariantsJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "VariantsJson",
                value: null);

            migrationBuilder.InsertData(
                table: "UiTranslations",
                columns: new[] { "Id", "Key", "Language", "Value" },
                values: new object[,]
                {
                    { 1, "HeroTitle", "ar", "جمال طبيعي من أجلك" },
                    { 2, "HeroSubtitle", "ar", "اكتشفي مجموعتنا العضوية" },
                    { 3, "ShopNow", "ar", "تسوقي الآن" },
                    { 4, "JojaMoments", "ar", "لحظات جوجا" },
                    { 5, "BestSellers", "ar", "الأكثر مبيعاً" },
                    { 6, "FilterAll", "ar", "الكل" },
                    { 7, "AddToCart", "ar", "أضف للسلة" },
                    { 8, "HeroTitle", "en", "Natural Beauty for You" },
                    { 9, "HeroSubtitle", "en", "Discover our organic collection" },
                    { 10, "ShopNow", "en", "Shop Now" },
                    { 11, "JojaMoments", "en", "Joja Moments" },
                    { 12, "BestSellers", "en", "Best Sellers" },
                    { 13, "FilterAll", "en", "All" },
                    { 14, "AddToCart", "en", "Add to Cart" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.DropTable(
                name: "ContentPages");

            migrationBuilder.DeleteData(
                table: "CategoryTranslations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CategoryTranslations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CategoryTranslations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "UiTranslations",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DropColumn(
                name: "VariantsJson",
                table: "Products");
        }
    }
}
