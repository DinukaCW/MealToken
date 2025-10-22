using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealToken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    DepartmnetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.DepartmnetId);
                });

            migrationBuilder.CreateTable(
                name: "Designation",
                columns: table => new
                {
                    DesignationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designation", x => x.DesignationId);
                });

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
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.SheduleId);
                });

            migrationBuilder.CreateTable(
                name: "TenantInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subdomain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchemaName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnableNotifications = table.Column<bool>(type: "bit", nullable: true),
                    EnableFunctionKeys = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserRoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserRoleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.UserRoleID);
                });

            migrationBuilder.CreateTable(
                name: "ClientDevice",
                schema: "dbo",
                columns: table => new
                {
                    ClientDeviceId = table.Column<int>(type: "int", nullable: false)
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
                    ReceiptWidthPixels = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DeviceShift = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientDevice", x => x.ClientDeviceId);
                    table.ForeignKey(
                        name: "FK_ClientDevice_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MealType",
                schema: "dbo",
                columns: table => new
                {
                    MealTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TokenIssueStartDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TokenIssueEndDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TokenIssueStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    TokenIssueEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    MealTimeStartDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MealTimeEndDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MealTimeStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    MealTimeEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    IsFunctionKeysEnable = table.Column<bool>(type: "bit", nullable: false),
                    IsAddOnsEnable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
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
                name: "Person",
                schema: "dbo",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PersonType = table.Column<int>(type: "int", nullable: false),
                    PersonNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NICNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DesignationId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    EmployeeGrade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PersonSubType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    WhatsappNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MealGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MealEligibility = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    SpecialNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_Person_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmnetId");
                    table.ForeignKey(
                        name: "FK_Person_Designation_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designation",
                        principalColumn: "DesignationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Person_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
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
                name: "SentNotification",
                schema: "dbo",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentNotification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_SentNotification_TenantInfo_TenantId",
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
                    ContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserRoleId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsPhoneNumberVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RememberMe = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LoginCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_UserRole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRole",
                        principalColumn: "UserRoleID",
                        onDelete: ReferentialAction.Restrict);
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
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

            migrationBuilder.CreateTable(
                name: "PayStatusByShiftPolicy",
                schema: "dbo",
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

            migrationBuilder.CreateTable(
                name: "ExternalLogin",
                schema: "dbo",
                columns: table => new
                {
                    ExternalLoginID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogin", x => x.ExternalLoginID);
                    table.ForeignKey(
                        name: "FK_ExternalLogin_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalLogin_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoginTrack",
                schema: "dbo",
                columns: table => new
                {
                    LoginTrackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    LoginMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    MFAUsed = table.Column<bool>(type: "bit", nullable: true),
                    MFAMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SessionID = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginTrack", x => x.LoginTrackID);
                    table.ForeignKey(
                        name: "FK_LoginTrack_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LoginTrack_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MfaSetting",
                schema: "dbo",
                columns: table => new
                {
                    MFASettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsMFAEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PreferredMFAType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfaSetting", x => x.MFASettingID);
                    table.ForeignKey(
                        name: "FK_MfaSetting_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MfaSetting_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Request",
                schema: "dbo",
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
                name: "SendToken",
                schema: "dbo",
                columns: table => new
                {
                    SendTokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MFADeviceID = table.Column<int>(type: "int", nullable: false),
                    UserTokenID = table.Column<int>(type: "int", nullable: false),
                    SendAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendSuccessful = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendToken", x => x.SendTokenID);
                    table.ForeignKey(
                        name: "FK_SendToken_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SendToken_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDepartment",
                schema: "dbo",
                columns: table => new
                {
                    UserDepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserRequestId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    RequestStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepartment", x => x.UserDepartmentId);
                    table.ForeignKey(
                        name: "FK_UserDepartment_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmnetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDepartment_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserDepartment_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserHistory",
                schema: "dbo",
                columns: table => new
                {
                    UserHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistory", x => x.UserHistoryId);
                    table.ForeignKey(
                        name: "FK_UserHistory_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserHistory_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "UserRequest",
                schema: "dbo",
                columns: table => new
                {
                    UserRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserRoleId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequest", x => x.UserRequestId);
                    table.ForeignKey(
                        name: "FK_UserRequest_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRequest_UserRole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRole",
                        principalColumn: "UserRoleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRequest_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserToken",
                schema: "dbo",
                columns: table => new
                {
                    TokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TokenType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsUsed = table.Column<bool>(type: "bit", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToken", x => x.TokenID);
                    table.ForeignKey(
                        name: "FK_UserToken_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserToken_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
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
                    AddOnSubTypeId = table.Column<int>(type: "int", nullable: false),
                    AddOnName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AddOnType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealAddOn", x => x.MealTypeAddOnId);
                    table.ForeignKey(
                        name: "FK_MealAddOn_MealSubType_AddOnSubTypeId",
                        column: x => x.AddOnSubTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealSubType",
                        principalColumn: "MealSubTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealAddOn_MealType_MealTypeId",
                        column: x => x.MealTypeId,
                        principalSchema: "dbo",
                        principalTable: "MealType",
                        principalColumn: "MealTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealAddOn_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

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
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealCost_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "dbo",
                        principalTable: "Supplier",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealCost_TenantInfo_TenantId",
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
                    FunctionKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                name: "MealConsumption",
                schema: "dbo",
                columns: table => new
                {
                    MealConsumptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    PersonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    ScheduleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddOnMeal = table.Column<bool>(type: "bit", nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    MealTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubTypeId = table.Column<int>(type: "int", nullable: true),
                    SubTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MealCostId = table.Column<int>(type: "int", nullable: false),
                    SupplierCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CompanyCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    EmployeeCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    DeviceSerialNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PayStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TockenIssued = table.Column<bool>(type: "bit", nullable: false),
                    JobStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "RequestMeal",
                schema: "dbo",
                columns: table => new
                {
                    RequestMealId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    MealTypeId = table.Column<int>(type: "int", nullable: false),
                    SubTypeId = table.Column<int>(type: "int", nullable: true),
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
                        name: "FK_RequestMeal_Request_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "dbo",
                        principalTable: "Request",
                        principalColumn: "MealRequestId");
                    table.ForeignKey(
                        name: "FK_RequestMeal_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientDevice_TenantId",
                schema: "dbo",
                table: "ClientDevice",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogin_TenantId",
                schema: "dbo",
                table: "ExternalLogin",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogin_UserID",
                schema: "dbo",
                table: "ExternalLogin",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_LoginTrack_TenantId",
                schema: "dbo",
                table: "LoginTrack",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginTrack_UserID",
                schema: "dbo",
                table: "LoginTrack",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_MealAddOn_AddOnSubTypeId",
                schema: "dbo",
                table: "MealAddOn",
                column: "AddOnSubTypeId");

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
                name: "IX_MealConsumption_DeviceId",
                schema: "dbo",
                table: "MealConsumption",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_MealCostId",
                schema: "dbo",
                table: "MealConsumption",
                column: "MealCostId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_MealTypeId",
                schema: "dbo",
                table: "MealConsumption",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_PersonId",
                schema: "dbo",
                table: "MealConsumption",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_SubTypeId",
                schema: "dbo",
                table: "MealConsumption",
                column: "SubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealConsumption_TenantId",
                schema: "dbo",
                table: "MealConsumption",
                column: "TenantId");

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
                name: "IX_MfaSetting_TenantId",
                schema: "dbo",
                table: "MfaSetting",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MfaSetting_UserID",
                schema: "dbo",
                table: "MfaSetting",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PayStatusByShiftPolicy_MealTypeId",
                schema: "dbo",
                table: "PayStatusByShiftPolicy",
                column: "MealTypeId");

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
                name: "IX_Request_ApproverOrRejectedId",
                schema: "dbo",
                table: "Request",
                column: "ApproverOrRejectedId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_RequesterId",
                schema: "dbo",
                table: "Request",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_TenantId",
                schema: "dbo",
                table: "Request",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_MealCostId",
                schema: "dbo",
                table: "RequestMeal",
                column: "MealCostId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_MealTypeId",
                schema: "dbo",
                table: "RequestMeal",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_RequestId",
                schema: "dbo",
                table: "RequestMeal",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_SubTypeId",
                schema: "dbo",
                table: "RequestMeal",
                column: "SubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMeal_TenantId",
                schema: "dbo",
                table: "RequestMeal",
                column: "TenantId");

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

            migrationBuilder.CreateIndex(
                name: "IX_SendToken_TenantId",
                schema: "dbo",
                table: "SendToken",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SendToken_UserID",
                schema: "dbo",
                table: "SendToken",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SentNotification_TenantId",
                schema: "dbo",
                table: "SentNotification",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_TenantId",
                schema: "dbo",
                table: "Supplier",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_DepartmentId",
                schema: "dbo",
                table: "UserDepartment",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_TenantId",
                schema: "dbo",
                table: "UserDepartment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_UserId",
                schema: "dbo",
                table: "UserDepartment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_TenantId",
                schema: "dbo",
                table: "UserHistory",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_UserId",
                schema: "dbo",
                table: "UserHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequest_ReviewedBy",
                schema: "dbo",
                table: "UserRequest",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequest_TenantId",
                schema: "dbo",
                table: "UserRequest",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequest_UserRoleId",
                schema: "dbo",
                table: "UserRequest",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "dbo",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                schema: "dbo",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                schema: "dbo",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserRoleId",
                schema: "dbo",
                table: "Users",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserToken_TenantId",
                schema: "dbo",
                table: "UserToken",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserToken_UserID",
                schema: "dbo",
                table: "UserToken",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalLogin",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LoginTrack",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MealAddOn",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MealConsumption",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MfaSetting",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PayStatusByShiftPolicy",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RequestMeal",
                schema: "dbo");

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
                name: "SendToken",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SentNotification",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserDepartment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserHistory",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserRequest",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserToken",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ClientDevice",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MealCost",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Request",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Person",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Schedule",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MealSubType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Supplier",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Designation");

            migrationBuilder.DropTable(
                name: "MealType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "TenantInfo");
        }
    }
}
