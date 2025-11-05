using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addrequestconsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestMealConsumption",
                schema: "dbo",
                columns: table => new
                {
                    RequestMealConsumptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    SubTypeId = table.Column<int>(type: "int", nullable: true),
                    MealCostId = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    TotalEmployeeContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCompanyContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSupplierCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestMealConsumption", x => x.RequestMealConsumptionId);
                    table.ForeignKey(
                        name: "FK_RequestMealConsumption_MealCost_MealCostId",
                        column: x => x.MealCostId,
                        principalSchema: "dbo",
                        principalTable: "MealCost",
                        principalColumn: "MealCostId");
                    table.ForeignKey(
                        name: "FK_RequestMealConsumption_MealSubType_SubTypeId",
                        column: x => x.SubTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealSubType",
                        principalColumn: "MealSubTypeId");
                    table.ForeignKey(
                        name: "FK_RequestMealConsumption_MealType_MealTypeId",
                        column: x => x.MealTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealType",
                        principalColumn: "MealTypeId");
                    table.ForeignKey(
                        name: "FK_RequestMealConsumption_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "dbo",
                        principalTable: "Supplier",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_RequestMealConsumption_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestMealConsumption_MealCostId",
                schema: "dbo",
                table: "RequestMealConsumption",
                column: "MealCostId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMealConsumption_MealTypeId",
                schema: "dbo",
                table: "RequestMealConsumption",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMealConsumption_SubTypeId",
                schema: "dbo",
                table: "RequestMealConsumption",
                column: "SubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMealConsumption_SupplierId",
                schema: "dbo",
                table: "RequestMealConsumption",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMealConsumption_TenantId",
                schema: "dbo",
                table: "RequestMealConsumption",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestMealConsumption",
                schema: "dbo");
        }
    }
}
