using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class changetables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchduleId",
                table: "MealConsumption",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchduleId",
                table: "MealConsumption");
        }
    }
}
