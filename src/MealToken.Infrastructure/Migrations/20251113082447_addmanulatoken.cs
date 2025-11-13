using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addmanulatoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManualTokenPrinted",
                schema: "dbo",
                columns: table => new
                {
                    ManualTokenPrintedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    PrintedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MealConsumptionId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenIssued = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualTokenPrinted", x => x.ManualTokenPrintedId);
                    table.ForeignKey(
                        name: "FK_ManualTokenPrinted_MealConsumption_MealConsumptionId",
                        column: x => x.MealConsumptionId,
                        principalSchema: "dbo",
                        principalTable: "MealConsumption",
                        principalColumn: "MealConsumptionId");
                    table.ForeignKey(
                        name: "FK_ManualTokenPrinted_Person_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "dbo",
                        principalTable: "Person",
                        principalColumn: "PersonId");
                    table.ForeignKey(
                        name: "FK_ManualTokenPrinted_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManualTokenPrinted_MealConsumptionId",
                schema: "dbo",
                table: "ManualTokenPrinted",
                column: "MealConsumptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualTokenPrinted_PersonId",
                schema: "dbo",
                table: "ManualTokenPrinted",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualTokenPrinted_TenantId",
                schema: "dbo",
                table: "ManualTokenPrinted",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManualTokenPrinted",
                schema: "dbo");
        }
    }
}
