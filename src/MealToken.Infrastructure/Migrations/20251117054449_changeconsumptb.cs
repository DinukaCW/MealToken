using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changeconsumptb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                schema: "dbo",
                table: "MealConsumption",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                schema: "dbo",
                table: "MealConsumption",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
