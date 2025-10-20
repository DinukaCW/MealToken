using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class tenanttb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "TenantInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableFunctionKeys",
                table: "TenantInfo",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableNotifications",
                table: "TenantInfo",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "TenantInfo");

            migrationBuilder.DropColumn(
                name: "EnableFunctionKeys",
                table: "TenantInfo");

            migrationBuilder.DropColumn(
                name: "EnableNotifications",
                table: "TenantInfo");
        }
    }
}
