using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations.MealTokenDb
{
    /// <inheritdoc />
    public partial class FixedTenantRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Department_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Designation_DesignationId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_MfaSettings_Users_UserID",
                schema: "dbo",
                table: "MfaSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_SendTokens_Users_UserID",
                schema: "dbo",
                table: "SendTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserID",
                schema: "dbo",
                table: "UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTokens",
                schema: "dbo",
                table: "UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SentNotifications",
                schema: "dbo",
                table: "SentNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SendTokens",
                schema: "dbo",
                table: "SendTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MfaSettings",
                schema: "dbo",
                table: "MfaSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                schema: "dbo",
                newName: "UserToken",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SentNotifications",
                schema: "dbo",
                newName: "SentNotification",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SendTokens",
                schema: "dbo",
                newName: "SendToken",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfaSettings",
                schema: "dbo",
                newName: "MfaSetting",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "Employee",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_UserTokens_UserID",
                schema: "dbo",
                table: "UserToken",
                newName: "IX_UserToken_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_SendTokens_UserID",
                schema: "dbo",
                table: "SendToken",
                newName: "IX_SendToken_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_MfaSettings_UserID",
                schema: "dbo",
                table: "MfaSetting",
                newName: "IX_MfaSetting_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_DesignationId",
                schema: "dbo",
                table: "Employee",
                newName: "IX_Employee_DesignationId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_DepartmentId",
                schema: "dbo",
                table: "Employee",
                newName: "IX_Employee_DepartmentId");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "dbo",
                table: "LoginTrack",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "dbo",
                table: "UserToken",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "dbo",
                table: "SentNotification",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "dbo",
                table: "SendToken",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "dbo",
                table: "MfaSetting",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "dbo",
                table: "Employee",
                type: "int",
                nullable: false,
                defaultValue: 1);

			migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM TenantInfo)
        BEGIN
            INSERT INTO TenantInfo (Name, Subdomain, SchemaName, ConnectionString, IsActive, CreatedAt)
            VALUES ('Default Tenant', 'default', 'dbo', '', 1, GETDATE())
        END
    ");

			// 2. Set all existing records to use the first tenant (instead of default 0)
			migrationBuilder.Sql(@"
        DECLARE @FirstTenantId int = (SELECT TOP 1 Id FROM TenantInfo ORDER BY Id);
        
        UPDATE LoginTrack SET TenantId = @FirstTenantId;
        UPDATE UserToken SET TenantId = @FirstTenantId;
        UPDATE SentNotification SET TenantId = @FirstTenantId;
        UPDATE SendToken SET TenantId = @FirstTenantId;
        UPDATE MfaSetting SET TenantId = @FirstTenantId;
        UPDATE Employee SET TenantId = @FirstTenantId;
        UPDATE ExternalLogin SET TenantId = @FirstTenantId;
        UPDATE UserRequest SET TenantId = @FirstTenantId;
        UPDATE Users SET TenantId = @FirstTenantId;
    ");

			migrationBuilder.AddPrimaryKey(
                name: "PK_UserToken",
                schema: "dbo",
                table: "UserToken",
                column: "TokenID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SentNotification",
                schema: "dbo",
                table: "SentNotification",
                column: "NotificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SendToken",
                schema: "dbo",
                table: "SendToken",
                column: "SendTokenID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MfaSetting",
                schema: "dbo",
                table: "MfaSetting",
                column: "MFASettingID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee",
                schema: "dbo",
                table: "Employee",
                column: "EmployeeId");

            migrationBuilder.CreateTable(
                name: "Visitor",
                schema: "dbo",
                columns: table => new
                {
                    VisitorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VisitorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CardName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    MealEligibility = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SpecialNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
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
                name: "IX_Users_TenantId",
                schema: "dbo",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequest_TenantId",
                schema: "dbo",
                table: "UserRequest",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginTrack_TenantId",
                schema: "dbo",
                table: "LoginTrack",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogin_TenantId",
                schema: "dbo",
                table: "ExternalLogin",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserToken_TenantId",
                schema: "dbo",
                table: "UserToken",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SentNotification_TenantId",
                schema: "dbo",
                table: "SentNotification",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SendToken_TenantId",
                schema: "dbo",
                table: "SendToken",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MfaSetting_TenantId",
                schema: "dbo",
                table: "MfaSetting",
                column: "TenantId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Department_DepartmentId",
                schema: "dbo",
                table: "Employee",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "DepartmnetId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Designation_DesignationId",
                schema: "dbo",
                table: "Employee",
                column: "DesignationId",
                principalTable: "Designation",
                principalColumn: "DesignationId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_TenantInfo_TenantId",
                schema: "dbo",
                table: "Employee",
                column: "TenantId",
                principalTable: "TenantInfo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalLogin_TenantInfo_TenantId",
                schema: "dbo",
                table: "ExternalLogin",
                column: "TenantId",
                principalTable: "TenantInfo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginTrack_TenantInfo_TenantId",
                schema: "dbo",
                table: "LoginTrack",
                column: "TenantId",
                principalTable: "TenantInfo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MfaSetting_TenantInfo_TenantId",
                schema: "dbo",
                table: "MfaSetting",
                column: "TenantId",
                principalTable: "TenantInfo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MfaSetting_Users_UserID",
                schema: "dbo",
                table: "MfaSetting",
                column: "UserID",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SendToken_TenantInfo_TenantId",
                schema: "dbo",
                table: "SendToken",
                column: "TenantId",
                principalTable: "TenantInfo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SendToken_Users_UserID",
                schema: "dbo",
                table: "SendToken",
                column: "UserID",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SentNotification_TenantInfo_TenantId",
                schema: "dbo",
                table: "SentNotification",
                column: "TenantId",
                principalTable: "TenantInfo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRequest_TenantInfo_TenantId",
                schema: "dbo",
                table: "UserRequest",
                column: "TenantId",
                principalTable: "TenantInfo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_TenantInfo_TenantId",
                schema: "dbo",
                table: "Users",
                column: "TenantId",
                principalTable: "TenantInfo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserToken_TenantInfo_TenantId",
                schema: "dbo",
                table: "UserToken",
                column: "TenantId",
                principalTable: "TenantInfo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserToken_Users_UserID",
                schema: "dbo",
                table: "UserToken",
                column: "UserID",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Department_DepartmentId",
                schema: "dbo",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Designation_DesignationId",
                schema: "dbo",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_TenantInfo_TenantId",
                schema: "dbo",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalLogin_TenantInfo_TenantId",
                schema: "dbo",
                table: "ExternalLogin");

            migrationBuilder.DropForeignKey(
                name: "FK_LoginTrack_TenantInfo_TenantId",
                schema: "dbo",
                table: "LoginTrack");

            migrationBuilder.DropForeignKey(
                name: "FK_MfaSetting_TenantInfo_TenantId",
                schema: "dbo",
                table: "MfaSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_MfaSetting_Users_UserID",
                schema: "dbo",
                table: "MfaSetting");

            migrationBuilder.DropForeignKey(
                name: "FK_SendToken_TenantInfo_TenantId",
                schema: "dbo",
                table: "SendToken");

            migrationBuilder.DropForeignKey(
                name: "FK_SendToken_Users_UserID",
                schema: "dbo",
                table: "SendToken");

            migrationBuilder.DropForeignKey(
                name: "FK_SentNotification_TenantInfo_TenantId",
                schema: "dbo",
                table: "SentNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRequest_TenantInfo_TenantId",
                schema: "dbo",
                table: "UserRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_TenantInfo_TenantId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_UserToken_TenantInfo_TenantId",
                schema: "dbo",
                table: "UserToken");

            migrationBuilder.DropForeignKey(
                name: "FK_UserToken_Users_UserID",
                schema: "dbo",
                table: "UserToken");

            migrationBuilder.DropTable(
                name: "Visitor",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserRequest_TenantId",
                schema: "dbo",
                table: "UserRequest");

            migrationBuilder.DropIndex(
                name: "IX_LoginTrack_TenantId",
                schema: "dbo",
                table: "LoginTrack");

            migrationBuilder.DropIndex(
                name: "IX_ExternalLogin_TenantId",
                schema: "dbo",
                table: "ExternalLogin");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserToken",
                schema: "dbo",
                table: "UserToken");

            migrationBuilder.DropIndex(
                name: "IX_UserToken_TenantId",
                schema: "dbo",
                table: "UserToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SentNotification",
                schema: "dbo",
                table: "SentNotification");

            migrationBuilder.DropIndex(
                name: "IX_SentNotification_TenantId",
                schema: "dbo",
                table: "SentNotification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SendToken",
                schema: "dbo",
                table: "SendToken");

            migrationBuilder.DropIndex(
                name: "IX_SendToken_TenantId",
                schema: "dbo",
                table: "SendToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MfaSetting",
                schema: "dbo",
                table: "MfaSetting");

            migrationBuilder.DropIndex(
                name: "IX_MfaSetting_TenantId",
                schema: "dbo",
                table: "MfaSetting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee",
                schema: "dbo",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_TenantId",
                schema: "dbo",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "LoginTrack");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "UserToken");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "SentNotification");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "SendToken");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "MfaSetting");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "Employee");

            migrationBuilder.RenameTable(
                name: "UserToken",
                schema: "dbo",
                newName: "UserTokens",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SentNotification",
                schema: "dbo",
                newName: "SentNotifications",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SendToken",
                schema: "dbo",
                newName: "SendTokens",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfaSetting",
                schema: "dbo",
                newName: "MfaSettings",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Employee",
                schema: "dbo",
                newName: "Employees");

            migrationBuilder.RenameIndex(
                name: "IX_UserToken_UserID",
                schema: "dbo",
                table: "UserTokens",
                newName: "IX_UserTokens_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_SendToken_UserID",
                schema: "dbo",
                table: "SendTokens",
                newName: "IX_SendTokens_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_MfaSetting_UserID",
                schema: "dbo",
                table: "MfaSettings",
                newName: "IX_MfaSettings_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_DesignationId",
                table: "Employees",
                newName: "IX_Employees_DesignationId");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_DepartmentId",
                table: "Employees",
                newName: "IX_Employees_DepartmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTokens",
                schema: "dbo",
                table: "UserTokens",
                column: "TokenID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SentNotifications",
                schema: "dbo",
                table: "SentNotifications",
                column: "NotificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SendTokens",
                schema: "dbo",
                table: "SendTokens",
                column: "SendTokenID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MfaSettings",
                schema: "dbo",
                table: "MfaSettings",
                column: "MFASettingID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Department_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "DepartmnetId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Designation_DesignationId",
                table: "Employees",
                column: "DesignationId",
                principalTable: "Designation",
                principalColumn: "DesignationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MfaSettings_Users_UserID",
                schema: "dbo",
                table: "MfaSettings",
                column: "UserID",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SendTokens_Users_UserID",
                schema: "dbo",
                table: "SendTokens",
                column: "UserID",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserID",
                schema: "dbo",
                table: "UserTokens",
                column: "UserID",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
