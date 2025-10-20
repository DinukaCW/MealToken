using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class changedepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDepartment_UserRequest_UserRequestId",
                schema: "dbo",
                table: "UserDepartment");

            migrationBuilder.DropIndex(
                name: "IX_UserDepartment_UserRequestId",
                schema: "dbo",
                table: "UserDepartment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_UserRequestId",
                schema: "dbo",
                table: "UserDepartment",
                column: "UserRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDepartment_UserRequest_UserRequestId",
                schema: "dbo",
                table: "UserDepartment",
                column: "UserRequestId",
                principalSchema: "dbo",
                principalTable: "UserRequest",
                principalColumn: "UserRequestId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
