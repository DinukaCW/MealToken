using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class addnewtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_Designation_DesignationId",
                schema: "dbo",
                table: "Person");

            migrationBuilder.CreateTable(
                name: "MealType",
                schema: "dbo",
                columns: table => new
                {
                    MealTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TokenIssueStartDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TokenIssueEndDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TokenIssueStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    TokenIssueEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    MealTimeStartDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MealTimeEndDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MealTimeStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    MealTimeEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    IsFunctionKeysEnable = table.Column<bool>(type: "bit", nullable: true),
                    IsAddOnsEnable = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealType", x => x.MealTypeId);
                    table.ForeignKey(
                        name: "FK_MealType_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                schema: "dbo",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SupplierRating = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.SupplierId);
                    table.ForeignKey(
                        name: "FK_Supplier_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MealAddOn",
                schema: "dbo",
                columns: table => new
                {
                    MealTypeAddOnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    AddOnName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealAddOn", x => x.MealTypeAddOnId);
                    table.ForeignKey(
                        name: "FK_MealAddOn_MealType_MealTypeId",
                        column: x => x.MealTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealType",
                        principalColumn: "MealTypeId");
                    table.ForeignKey(
                        name: "FK_MealAddOn_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MealSubType",
                schema: "dbo",
                columns: table => new
                {
                    MealSubTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    SubTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Functionkey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealSubType", x => x.MealSubTypeId);
                    table.ForeignKey(
                        name: "FK_MealSubType_MealType_MealTypeId",
                        column: x => x.MealTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealType",
                        principalColumn: "MealTypeId");
                    table.ForeignKey(
                        name: "FK_MealSubType_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealAddOn_MealTypeId",
                schema: "dbo",
                table: "MealAddOn",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealAddOn_TenantId",
                schema: "dbo",
                table: "MealAddOn",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MealSubType_MealTypeId",
                schema: "dbo",
                table: "MealSubType",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealSubType_TenantId",
                schema: "dbo",
                table: "MealSubType",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MealType_TenantId",
                schema: "dbo",
                table: "MealType",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_TenantId",
                schema: "dbo",
                table: "Supplier",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Designation_DesignationId",
                schema: "dbo",
                table: "Person",
                column: "DesignationId",
                principalTable: "Designation",
                principalColumn: "DesignationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_Designation_DesignationId",
                schema: "dbo",
                table: "Person");

            migrationBuilder.DropTable(
                name: "MealAddOn",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MealSubType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Supplier",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MealType",
                schema: "dbo");

            migrationBuilder.AddForeignKey(
                name: "FK_Person_Designation_DesignationId",
                schema: "dbo",
                table: "Person",
                column: "DesignationId",
                principalTable: "Designation",
                principalColumn: "DesignationId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
