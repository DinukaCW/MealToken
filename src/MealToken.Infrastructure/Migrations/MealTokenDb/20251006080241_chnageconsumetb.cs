using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class chnageconsumetb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CompanyCost",
                table: "MealConsumption",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeviceSerialNo",
                table: "MealConsumption",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeeCost",
                table: "MealConsumption",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MealTypeName",
                table: "MealConsumption",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PersonName",
                table: "MealConsumption",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SchduleName",
                table: "MealConsumption",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "MealConsumption",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SubTypeName",
                table: "MealConsumption",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SupplierCost",
                table: "MealConsumption",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeviceShift",
                schema: "dbo",
                table: "ClientDevice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyCost",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "DeviceSerialNo",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "EmployeeCost",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "MealTypeName",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "PersonName",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "SchduleName",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "SubTypeName",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "SupplierCost",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "DeviceShift",
                schema: "dbo",
                table: "ClientDevice");
        }
    }
}
