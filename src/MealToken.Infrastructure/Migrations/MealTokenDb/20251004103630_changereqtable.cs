using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class changereqtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old foreign key if exists
            migrationBuilder.DropForeignKey(
                name: "FK_RequestMeal_Request_RequestMealId",
                table: "RequestMeal");

            // Drop the primary key if exists
            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestMeal",
                table: "RequestMeal");

            // Drop the old column
            migrationBuilder.DropColumn(
                name: "RequestMealId",
                table: "RequestMeal");

            // Add new column with IDENTITY
            migrationBuilder.AddColumn<int>(
                name: "RequestMealId",
                table: "RequestMeal",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Add primary key again
            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestMeal",
                table: "RequestMeal",
                column: "RequestMealId");

            // Recreate foreign key (using RequestId, as you already fixed)
            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_RequestId",
                table: "RequestMeal",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestMeal_Request_RequestId",
                table: "RequestMeal",
                column: "RequestId",
                principalTable: "Request",
                principalColumn: "MealRequestId");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestMeal_Request_RequestId",
                table: "RequestMeal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestMeal",
                table: "RequestMeal");

            migrationBuilder.DropIndex(
                name: "IX_RequestMeal_RequestId",
                table: "RequestMeal");

            // Drop new column
            migrationBuilder.DropColumn(
                name: "RequestMealId",
                table: "RequestMeal");

            // Add back without IDENTITY
            migrationBuilder.AddColumn<int>(
                name: "RequestMealId",
                table: "RequestMeal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestMeal",
                table: "RequestMeal",
                column: "RequestMealId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestMeal_Request_RequestMealId",
                table: "RequestMeal",
                column: "RequestMealId",
                principalTable: "Request",
                principalColumn: "MealRequestId");
        }

    }
}
