using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changetenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantInfo_TenantId",
                table: "TenantInfo");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TenantInfo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "TenantInfo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TenantInfo_TenantId",
                table: "TenantInfo",
                column: "TenantId",
                unique: true);
        }
    }
}
