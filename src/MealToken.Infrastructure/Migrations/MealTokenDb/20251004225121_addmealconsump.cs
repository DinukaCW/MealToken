using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class addmealconsump : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MealConsumption",
                columns: table => new
                {
                    MealConsumptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time", nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    SubTypeId = table.Column<int>(type: "int", nullable: true),
                    MealCostId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PayStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TockenIssued = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealConsumption", x => x.MealConsumptionId);
                    table.ForeignKey(
                        name: "FK_MealConsumption_ClientDevice_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "dbo",
                        principalTable: "ClientDevice",
                        principalColumn: "ClientDeviceId");
                    table.ForeignKey(
                        name: "FK_MealConsumption_MealCost_MealCostId",
                        column: x => x.MealCostId,
                        principalSchema: "dbo",
                        principalTable: "MealCost",
                        principalColumn: "MealCostId");
                    table.ForeignKey(
                        name: "FK_MealConsumption_MealSubType_SubTypeId",
                        column: x => x.SubTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealSubType",
                        principalColumn: "MealSubTypeId");
                    table.ForeignKey(
                        name: "FK_MealConsumption_MealType_MealTypeId",
                        column: x => x.MealTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealType",
                        principalColumn: "MealTypeId");
                    table.ForeignKey(
                        name: "FK_MealConsumption_Person_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "dbo",
                        principalTable: "Person",
                        principalColumn: "PersonId");
                    table.ForeignKey(
                        name: "FK_MealConsumption_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayStatusByShiftPolicy",
                columns: table => new
                {
                    PolicyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    IsMalePaid = table.Column<bool>(type: "bit", nullable: false),
                    IsFemalePaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayStatusByShiftPolicy", x => x.PolicyId);
                    table.ForeignKey(
                        name: "FK_PayStatusByShiftPolicy_MealType_MealTypeId",
                        column: x => x.MealTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealType",
                        principalColumn: "MealTypeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_DeviceId",
                table: "MealConsumption",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_MealCostId",
                table: "MealConsumption",
                column: "MealCostId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_MealTypeId",
                table: "MealConsumption",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_PersonId",
                table: "MealConsumption",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_SubTypeId",
                table: "MealConsumption",
                column: "SubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_TenantId",
                table: "MealConsumption",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStatusByShiftPolicy_MealTypeId",
                table: "PayStatusByShiftPolicy",
                column: "MealTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealConsumption");

            migrationBuilder.DropTable(
                name: "PayStatusByShiftPolicy");
        }
    }
}
