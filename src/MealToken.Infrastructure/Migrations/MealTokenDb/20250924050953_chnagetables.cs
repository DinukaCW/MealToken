using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class chnagetables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employee",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Visitor",
                schema: "dbo");

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

            migrationBuilder.CreateTable(
                name: "Person",
                schema: "dbo",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PersonType = table.Column<int>(type: "int", nullable: false),
                    PersonNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NICNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DesignationId = table.Column<int>(type: "int", nullable: true),
                    EmployeeGrade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    PersonSubType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MealGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MealEligibility = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    SpecialNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DepartmentDepartmnetId = table.Column<int>(type: "int", nullable: false),
                    DesignationId1 = table.Column<int>(type: "int", nullable: true),
                    TenantId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_Person_Department_DepartmentDepartmnetId",
                        column: x => x.DepartmentDepartmnetId,
                        principalSchema: "dbo",
                        principalTable: "Department",
                        principalColumn: "DepartmnetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Person_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "dbo",
                        principalTable: "Department",
                        principalColumn: "DepartmnetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Person_Designation_DesignationId",
                        column: x => x.DesignationId,
                        principalSchema: "dbo",
                        principalTable: "Designation",
                        principalColumn: "DesignationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Person_Designation_DesignationId1",
                        column: x => x.DesignationId1,
                        principalSchema: "dbo",
                        principalTable: "Designation",
                        principalColumn: "DesignationId");
                    table.ForeignKey(
                        name: "FK_Person_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "dbo",
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Person_TenantInfo_TenantId1",
                        column: x => x.TenantId1,
                        principalSchema: "dbo",
                        principalTable: "TenantInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Person_DepartmentDepartmnetId",
                schema: "dbo",
                table: "Person",
                column: "DepartmentDepartmnetId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_DepartmentId",
                schema: "dbo",
                table: "Person",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_DesignationId",
                schema: "dbo",
                table: "Person",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_DesignationId1",
                schema: "dbo",
                table: "Person",
                column: "DesignationId1");

            migrationBuilder.CreateIndex(
                name: "IX_Person_PersonType",
                schema: "dbo",
                table: "Person",
                column: "PersonType");

            migrationBuilder.CreateIndex(
                name: "IX_Person_TenantId_NICNumber",
                schema: "dbo",
                table: "Person",
                columns: new[] { "TenantId", "NICNumber" },
                unique: true,
                filter: "[NICNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Person_TenantId_PersonNumber",
                schema: "dbo",
                table: "Person",
                columns: new[] { "TenantId", "PersonNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Person_TenantId1",
                schema: "dbo",
                table: "Person",
                column: "TenantId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Person",
                schema: "dbo");

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

            migrationBuilder.CreateTable(
                name: "Employee",
                schema: "dbo",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    DesignationId = table.Column<int>(type: "int", nullable: false),
                    EmployeeGrade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MealEligibility = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MealGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NICNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employee_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmnetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employee_Designation_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designation",
                        principalColumn: "DesignationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employee_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Visitor",
                schema: "dbo",
                columns: table => new
                {
                    VisitorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MealEligibility = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SpecialNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    VisitorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitor", x => x.VisitorId);
                    table.ForeignKey(
                        name: "FK_Visitor_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmnetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visitor_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DepartmentId",
                schema: "dbo",
                table: "Employee",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DesignationId",
                schema: "dbo",
                table: "Employee",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_TenantId",
                schema: "dbo",
                table: "Employee",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitor_DepartmentId",
                schema: "dbo",
                table: "Visitor",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitor_TenantId",
                schema: "dbo",
                table: "Visitor",
                column: "TenantId");
        }
    }
}
