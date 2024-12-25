using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductCatalogApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateconfigurtion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "Products",
                newName: "LastUpdatedDateTime");

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedByUserId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdatedByUserId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedDateTime",
                table: "Products",
                newName: "LastUpdated");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
