using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SneakerApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class VerifiedMapped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Verified",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Verified",
                table: "Products");
        }
    }
}
