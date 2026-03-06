using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Joja.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHeaderAnnouncementText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeaderAnnouncementText",
                table: "SiteSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeaderAnnouncementText",
                table: "SiteSettings");
        }
    }
}
