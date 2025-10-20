using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class changeperson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "dbo",
                table: "MealAddOn");

            migrationBuilder.RenameColumn(
                name: "SchduleName",
                schema: "dbo",
                table: "MealConsumption",
                newName: "ScheduleName");

            migrationBuilder.RenameColumn(
                name: "SchduleId",
                schema: "dbo",
                table: "MealConsumption",
                newName: "ScheduleId");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "Person",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappNumber",
                schema: "dbo",
                table: "Person",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AddOnMeal",
                schema: "dbo",
                table: "MealConsumption",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AddOnSubTypeId",
                schema: "dbo",
                table: "MealAddOn",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MealAddOn_AddOnSubTypeId",
                schema: "dbo",
                table: "MealAddOn",
                column: "AddOnSubTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MealAddOn_MealSubType_AddOnSubTypeId",
                schema: "dbo",
                table: "MealAddOn",
                column: "AddOnSubTypeId",
                principalSchema: "dbo",
                principalTable: "MealSubType",
                principalColumn: "MealSubTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealAddOn_MealSubType_AddOnSubTypeId",
                schema: "dbo",
                table: "MealAddOn");

            migrationBuilder.DropIndex(
                name: "IX_MealAddOn_AddOnSubTypeId",
                schema: "dbo",
                table: "MealAddOn");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "WhatsappNumber",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "AddOnMeal",
                schema: "dbo",
                table: "MealConsumption");

            migrationBuilder.DropColumn(
                name: "AddOnSubTypeId",
                schema: "dbo",
                table: "MealAddOn");

            migrationBuilder.RenameColumn(
                name: "ScheduleName",
                schema: "dbo",
                table: "MealConsumption",
                newName: "SchduleName");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                schema: "dbo",
                table: "MealConsumption",
                newName: "SchduleId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "MealAddOn",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
