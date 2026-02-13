using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Joja.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedVariantsJson",
                table: "OrderItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedVariantsJson",
                table: "OrderItems");
        }
    }
}
