using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class chnagevalues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_Department_DepartmentDepartmnetId",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropForeignKey(
                name: "FK_Person_Designation_DesignationId1",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropForeignKey(
                name: "FK_Person_TenantInfo_TenantId1",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_DepartmentDepartmnetId",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_DesignationId1",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_TenantId1",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "DepartmentDepartmnetId",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "DesignationId1",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                schema: "dbo",
                table: "Person");

            migrationBuilder.RenameTable(
                name: "TenantInfo",
                schema: "dbo",
                newName: "TenantInfo");

            migrationBuilder.RenameTable(
                name: "Designation",
                schema: "dbo",
                newName: "Designation");

            migrationBuilder.RenameTable(
                name: "Department",
                schema: "dbo",
                newName: "Department");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TenantInfo",
                newName: "TenantInfo",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Designation",
                newName: "Designation",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Department",
                newName: "Department",
                newSchema: "dbo");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentDepartmnetId",
                schema: "dbo",
                table: "Person",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DesignationId1",
                schema: "dbo",
                table: "Person",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId1",
                schema: "dbo",
                table: "Person",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Person_DepartmentDepartmnetId",
                schema: "dbo",
                table: "Person",
                column: "DepartmentDepartmnetId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_DesignationId1",
                schema: "dbo",
                table: "Person",
                column: "DesignationId1");

            migrationBuilder.CreateIndex(
                name: "IX_Person_TenantId1",
                schema: "dbo",
                table: "Person",
                column: "TenantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Department_DepartmentDepartmnetId",
                schema: "dbo",
                table: "Person",
                column: "DepartmentDepartmnetId",
                principalSchema: "dbo",
                principalTable: "Department",
                principalColumn: "DepartmnetId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Designation_DesignationId1",
                schema: "dbo",
                table: "Person",
                column: "DesignationId1",
                principalSchema: "dbo",
                principalTable: "Designation",
                principalColumn: "DesignationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Person_TenantInfo_TenantId1",
                schema: "dbo",
                table: "Person",
                column: "TenantId1",
                principalSchema: "dbo",
                principalTable: "TenantInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
