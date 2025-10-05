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
            migrationBuilder.CreateTable(
                name: "MealCost",
                schema: "dbo",
                columns: table => new
                {
                    MealCostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    MealSubTypeId = table.Column<int>(type: "int", nullable: true),
                    SupplierCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompanyCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmployeeCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealCost", x => x.MealCostId);
                    table.ForeignKey(
                        name: "FK_MealCost_MealSubType_MealSubTypeId",
                        column: x => x.MealSubTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealSubType",
                        principalColumn: "MealSubTypeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MealCost_MealType_MealTypeId",
                        column: x => x.MealTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealType",
                        principalColumn: "MealTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealCost_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "dbo",
                        principalTable: "Supplier",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealCost_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealCost_MealSubTypeId",
                schema: "dbo",
                table: "MealCost",
                column: "MealSubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealCost_MealTypeId",
                schema: "dbo",
                table: "MealCost",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealCost_SupplierId",
                schema: "dbo",
                table: "MealCost",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_MealCost_TenantId_MealTypeId_MealSubTypeId_SupplierId",
                schema: "dbo",
                table: "MealCost",
                columns: new[] { "TenantId", "MealTypeId", "MealSubTypeId", "SupplierId" },
                unique: true,
                filter: "[MealSubTypeId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealCost",
                schema: "dbo");
        }
    }
}
