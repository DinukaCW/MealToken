using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class chnageconsumetbwd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobStatus",
                table: "MealConsumption",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobStatus",
                table: "MealConsumption");
        }
    }
}
