using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class requesttables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Request",
                columns: table => new
                {
                    MealRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NoofAttendees = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    ApproverOrRejectedId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Request", x => x.MealRequestId);
                    table.ForeignKey(
                        name: "FK_Request_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Request_Users_ApproverOrRejectedId",
                        column: x => x.ApproverOrRejectedId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Request_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "RequestMeal",
                columns: table => new
                {
                    RequestMealId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    SubTypeId = table.Column<int>(type: "int", nullable: false),
                    MealCostId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestMeal", x => x.RequestMealId);
                    table.ForeignKey(
                        name: "FK_RequestMeal_MealCost_MealCostId",
                        column: x => x.MealCostId,
                        principalSchema: "dbo",
                        principalTable: "MealCost",
                        principalColumn: "MealCostId");
                    table.ForeignKey(
                        name: "FK_RequestMeal_MealSubType_SubTypeId",
                        column: x => x.SubTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealSubType",
                        principalColumn: "MealSubTypeId");
                    table.ForeignKey(
                        name: "FK_RequestMeal_MealType_MealTypeId",
                        column: x => x.MealTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealType",
                        principalColumn: "MealTypeId");
                    table.ForeignKey(
                        name: "FK_RequestMeal_Request_RequestMealId",
                        column: x => x.RequestMealId,
                        principalTable: "Request",
                        principalColumn: "MealRequestId");
                    table.ForeignKey(
                        name: "FK_RequestMeal_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Request_ApproverOrRejectedId",
                table: "Request",
                column: "ApproverOrRejectedId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_RequesterId",
                table: "Request",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_TenantId",
                table: "Request",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_MealCostId",
                table: "RequestMeal",
                column: "MealCostId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_MealTypeId",
                table: "RequestMeal",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_SubTypeId",
                table: "RequestMeal",
                column: "SubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_TenantId",
                table: "RequestMeal",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestMeal");

            migrationBuilder.DropTable(
                name: "Request");
        }
    }
}
