using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class newtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "RequestMeal",
                newName: "RequestMeal",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Request",
                newName: "Request",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PayStatusByShiftPolicy",
                newName: "PayStatusByShiftPolicy",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MealConsumption",
                newName: "MealConsumption",
                newSchema: "dbo");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "MealType",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "MealSubType",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "MealCost",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "UserDepartment",
                schema: "dbo",
                columns: table => new
                {
                    UserDepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserRequestId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    RequestStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepartment", x => x.UserDepartmentId);
                    table.ForeignKey(
                        name: "FK_UserDepartment_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmnetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDepartment_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserDepartment_UserRequest_UserRequestId",
                        column: x => x.UserRequestId,
                        principalSchema: "dbo",
                        principalTable: "UserRequest",
                        principalColumn: "UserRequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDepartment_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_DepartmentId",
                schema: "dbo",
                table: "UserDepartment",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_TenantId",
                schema: "dbo",
                table: "UserDepartment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_UserId",
                schema: "dbo",
                table: "UserDepartment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_UserRequestId",
                schema: "dbo",
                table: "UserDepartment",
                column: "UserRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDepartment",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "MealType");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "MealSubType");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "MealCost");

            migrationBuilder.RenameTable(
                name: "RequestMeal",
                schema: "dbo",
                newName: "RequestMeal");

            migrationBuilder.RenameTable(
                name: "Request",
                schema: "dbo",
                newName: "Request");

            migrationBuilder.RenameTable(
                name: "PayStatusByShiftPolicy",
                schema: "dbo",
                newName: "PayStatusByShiftPolicy");

            migrationBuilder.RenameTable(
                name: "MealConsumption",
                schema: "dbo",
                newName: "MealConsumption");
        }
    }
}
