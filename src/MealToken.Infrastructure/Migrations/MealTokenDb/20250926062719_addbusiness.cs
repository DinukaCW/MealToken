using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class addbusiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealAddOn_MealType_MealTypeId",
                schema: "dbo",
                table: "MealAddOn");

            migrationBuilder.DropForeignKey(
                name: "FK_MealCost_MealType_MealTypeId",
                schema: "dbo",
                table: "MealCost");

            migrationBuilder.DropForeignKey(
                name: "FK_MealCost_Supplier_SupplierId",
                schema: "dbo",
                table: "MealCost");

            migrationBuilder.CreateTable(
                name: "ClientDevice",
                schema: "dbo",
                columns: table => new
                {
                    ClientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    MachineNumber = table.Column<int>(type: "int", nullable: false),
                    SerialNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrinterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReceiptHeightPixels = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReceiptWidthPixels = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientDevice", x => x.ClientID);
                    table.ForeignKey(
                        name: "FK_ClientDevice_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientDevice_TenantId",
                schema: "dbo",
                table: "ClientDevice",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_MealAddOn_MealType_MealTypeId",
                schema: "dbo",
                table: "MealAddOn",
                column: "MealTypeId",
                principalSchema: "dbo",
                principalTable: "MealType",
                principalColumn: "MealTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MealCost_MealType_MealTypeId",
                schema: "dbo",
                table: "MealCost",
                column: "MealTypeId",
                principalSchema: "dbo",
                principalTable: "MealType",
                principalColumn: "MealTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MealCost_Supplier_SupplierId",
                schema: "dbo",
                table: "MealCost",
                column: "SupplierId",
                principalSchema: "dbo",
                principalTable: "Supplier",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealAddOn_MealType_MealTypeId",
                schema: "dbo",
                table: "MealAddOn");

            migrationBuilder.DropForeignKey(
                name: "FK_MealCost_MealType_MealTypeId",
                schema: "dbo",
                table: "MealCost");

            migrationBuilder.DropForeignKey(
                name: "FK_MealCost_Supplier_SupplierId",
                schema: "dbo",
                table: "MealCost");

            migrationBuilder.DropTable(
                name: "ClientDevice",
                schema: "dbo");

            migrationBuilder.AddForeignKey(
                name: "FK_MealAddOn_MealType_MealTypeId",
                schema: "dbo",
                table: "MealAddOn",
                column: "MealTypeId",
                principalSchema: "dbo",
                principalTable: "MealType",
                principalColumn: "MealTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MealCost_MealType_MealTypeId",
                schema: "dbo",
                table: "MealCost",
                column: "MealTypeId",
                principalSchema: "dbo",
                principalTable: "MealType",
                principalColumn: "MealTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MealCost_Supplier_SupplierId",
                schema: "dbo",
                table: "MealCost",
                column: "SupplierId",
                principalSchema: "dbo",
                principalTable: "Supplier",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
