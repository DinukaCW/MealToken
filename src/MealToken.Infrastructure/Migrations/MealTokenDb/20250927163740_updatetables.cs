using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class updatetables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedule",
                schema: "dbo",
                columns: table => new
                {
                    SheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SheduleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShedulePeriod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.SheduleId);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleDate",
                schema: "dbo",
                columns: table => new
                {
                    SheduleDateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SheduleId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleDate", x => x.SheduleDateId);
                    table.ForeignKey(
                        name: "FK_ScheduleDate_Schedule_SheduleId",
                        column: x => x.SheduleId,
                        principalSchema: "dbo",
                        principalTable: "Schedule",
                        principalColumn: "SheduleId");
                    table.ForeignKey(
                        name: "FK_ScheduleDate_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScheduleMeal",
                schema: "dbo",
                columns: table => new
                {
                    SheduleMealSubTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SheduleId = table.Column<int>(type: "int", nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    MealSubTypeId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    IsFunctionKeysEnable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FunctionKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokenIssueStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    TokenIssueEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleMeal", x => x.SheduleMealSubTypeId);
                    table.ForeignKey(
                        name: "FK_ScheduleMeal_MealSubType_MealSubTypeId",
                        column: x => x.MealSubTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealSubType",
                        principalColumn: "MealSubTypeId");
                    table.ForeignKey(
                        name: "FK_ScheduleMeal_MealType_MealTypeId",
                        column: x => x.MealTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealType",
                        principalColumn: "MealTypeId");
                    table.ForeignKey(
                        name: "FK_ScheduleMeal_Schedule_SheduleId",
                        column: x => x.SheduleId,
                        principalSchema: "dbo",
                        principalTable: "Schedule",
                        principalColumn: "SheduleId");
                    table.ForeignKey(
                        name: "FK_ScheduleMeal_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "dbo",
                        principalTable: "Supplier",
                        principalColumn: "SupplierId");
                    table.ForeignKey(
                        name: "FK_ScheduleMeal_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SchedulePerson",
                schema: "dbo",
                columns: table => new
                {
                    ShedulePersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    SheduleId = table.Column<int>(type: "int", nullable: false),
                    ScheduleSheduleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulePerson", x => x.ShedulePersonId);
                    table.ForeignKey(
                        name: "FK_SchedulePerson_Person_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "dbo",
                        principalTable: "Person",
                        principalColumn: "PersonId");
                    table.ForeignKey(
                        name: "FK_SchedulePerson_Schedule_ScheduleSheduleId",
                        column: x => x.ScheduleSheduleId,
                        principalSchema: "dbo",
                        principalTable: "Schedule",
                        principalColumn: "SheduleId");
                    table.ForeignKey(
                        name: "FK_SchedulePerson_Schedule_SheduleId",
                        column: x => x.SheduleId,
                        principalSchema: "dbo",
                        principalTable: "Schedule",
                        principalColumn: "SheduleId");
                    table.ForeignKey(
                        name: "FK_SchedulePerson_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDate_SheduleId",
                schema: "dbo",
                table: "ScheduleDate",
                column: "SheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDate_TenantId",
                schema: "dbo",
                table: "ScheduleDate",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMeal_MealSubTypeId",
                schema: "dbo",
                table: "ScheduleMeal",
                column: "MealSubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMeal_MealTypeId",
                schema: "dbo",
                table: "ScheduleMeal",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMeal_SheduleId",
                schema: "dbo",
                table: "ScheduleMeal",
                column: "SheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMeal_SupplierId",
                schema: "dbo",
                table: "ScheduleMeal",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMeal_TenantId",
                schema: "dbo",
                table: "ScheduleMeal",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePerson_PersonId",
                schema: "dbo",
                table: "SchedulePerson",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePerson_ScheduleSheduleId",
                schema: "dbo",
                table: "SchedulePerson",
                column: "ScheduleSheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePerson_SheduleId",
                schema: "dbo",
                table: "SchedulePerson",
                column: "SheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePerson_TenantId",
                schema: "dbo",
                table: "SchedulePerson",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleDate",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ScheduleMeal",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SchedulePerson",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Schedule",
                schema: "dbo");
        }
    }
}
