using Authentication.Models.Entities;
using MealToken.Application.Interfaces;
using MealToken.Application.Services;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAParser;

namespace MealToken.Infrastructure.Persistence
{
	public class MealTokenDbContext : DbContext 
	{
		private readonly ITenantContext _tenantContext;
		private readonly string _schema;

		public MealTokenDbContext(DbContextOptions<MealTokenDbContext> options, ITenantContext tenantContext)
			: base(options)
		{
			_tenantContext = tenantContext;
			_schema = _tenantContext.SchemaName ?? "dbo";
		}

		// Your existing DbSets
		public DbSet<User> Users { get; set; }
		public DbSet<ExternalLogin> ExternalLogin { get; set; }
		public DbSet<LoginTrack> LoginTrack { get; set; }
		public DbSet<MfaSetting> MfaSetting { get; set; }
		public DbSet<SendToken> SendToken { get; set; }
		public DbSet<SentNotification> SentNotification { get; set; }
		public DbSet<UserToken> UserToken { get; set; }
		public DbSet<UserRequest> UserRequest { get; set; }
		public DbSet<Person> Person { get; set; }
		public DbSet<Supplier> Supplier { get; set; }
		public DbSet<MealType> MealType { get; set; }
		public DbSet<MealSubType> MealSubType { get; set; }
		public DbSet<MealAddOn> MealAddOn { get; set; }
		public DbSet<MealCost> MealCost { get; set; }
		public DbSet<ClientDevice> ClientDevice { get; set; }
		public DbSet<Schedule> Schedule { get; set; }
		public DbSet<ScheduleMeal> ScheduleMeal { get; set; }
		public DbSet<ScheduleDate> ScheduleDate { get; set; }
		public DbSet<SchedulePerson> SchedulePerson { get; set; }
		public DbSet<Request> Request { get; set; }
		public DbSet<RequestMeal> RequestMeal { get; set; }
        public DbSet<MealConsumption> MealConsumption { get; set; }
        public DbSet<PayStatusByShift> PayStatusByShiftPolicy { get; set; }
        public DbSet<UserDepartment> UserDepartment { get; set; }
		public DbSet<UserHistory> UserHistory { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Apply schema to all entities based on current tenant
			if (!string.IsNullOrEmpty(_tenantContext.SchemaName))
			{
				var schema = _tenantContext.SchemaName;

				// Apply schema to all entity types
				foreach (var entityType in modelBuilder.Model.GetEntityTypes())
				{
					modelBuilder.Entity(entityType.ClrType).ToTable(entityType.GetTableName(), schema);
				}
			}

			// Configure entities
			ConfigureEntities(modelBuilder);
		}

		private void ConfigureEntities(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>(entity =>
			{
				entity.ToTable("Users", _schema);
				// Primary Key
				entity.HasKey(e => e.UserID);

				entity.Property(e => e.TenantId)
					  .IsRequired();

				// Properties
				entity.Property(e => e.Username)
					  .HasMaxLength(100)
					  .IsRequired();

				entity.Property(e => e.FullName)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.PasswordHash)
					  .HasMaxLength(255);

				entity.Property(e => e.UserRoleId)
					  .IsRequired();

				entity.Property(e => e.Email)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.PhoneNumber)
					  .HasMaxLength(50);

				entity.Property(e => e.IsPhoneNumberVerified)
					  .IsRequired()
					  .HasDefaultValue(false);

				entity.Property(e => e.IsEmailVerified)
					  .IsRequired()
					  .HasDefaultValue(false);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.IsLocked)
					  .IsRequired()
					  .HasDefaultValue(false);

				entity.Property(e => e.RememberMe)
					  .IsRequired()
					  .HasDefaultValue(false);

				entity.Property(e => e.FailedLoginCount)
					  .IsRequired()
					  .HasDefaultValue(0);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("GETDATE()");

				entity.Property(e => e.LoginCount)
					  .IsRequired()
					  .HasDefaultValue(0);

				entity.Property(e => e.LastLoginAt);

				// Foreign key relationship (if you have a UserRole table)
				entity.HasOne<UserRole>()
					  .WithMany()
					  .HasForeignKey(e => e.UserRoleId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne<TenantInfo>()
				  .WithMany()
				  .HasForeignKey(e => e.TenantId)
				  .OnDelete(DeleteBehavior.NoAction);
				// Indexes
				entity.HasIndex(e => e.Email).IsUnique();
				entity.HasIndex(e => e.Username).IsUnique();
			});
			modelBuilder.Entity<ExternalLogin>(entity =>
			{
				entity.ToTable("ExternalLogin", _schema);
				entity.HasKey(e => e.ExternalLoginID);

				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.UserID)
					  .IsRequired();

				entity.Property(e => e.ProviderName)
					  .HasMaxLength(100)
					  .IsRequired();

				entity.Property(e => e.ProviderKey)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.ProviderDisplayName)
					  .HasMaxLength(255);

				// Foreign key to User
				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserID)
					  .OnDelete(DeleteBehavior.Cascade);
				entity.HasOne<TenantInfo>()
				  .WithMany()
				  .HasForeignKey(e => e.TenantId)
				  .OnDelete(DeleteBehavior.NoAction);
			});
			modelBuilder.Entity<LoginTrack>(entity =>
			{
				entity.ToTable("LoginTrack", _schema);
				entity.HasKey(e => e.LoginTrackID);
				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.UserID)
					  .IsRequired();

				entity.Property(e => e.LoginMethod)
					  .HasMaxLength(50)
					  .IsRequired();

				entity.Property(e => e.LoginTime)
					  .IsRequired();

				entity.Property(e => e.IPAddress)
					  .HasMaxLength(50);

				entity.Property(e => e.DeviceType)
					  .HasMaxLength(50);

				entity.Property(e => e.OperatingSystem)
					  .HasMaxLength(50);

				entity.Property(e => e.Browser)
					  .HasMaxLength(100);

				entity.Property(e => e.Country)
					  .HasMaxLength(50);

				entity.Property(e => e.City)
					  .HasMaxLength(50);

				entity.Property(e => e.IsSuccessful)
					  .IsRequired();

				entity.Property(e => e.MFAUsed);

				entity.Property(e => e.MFAMethod)
					  .HasMaxLength(50);

				entity.Property(e => e.SessionID)
					  .HasMaxLength(255);

				entity.Property(e => e.FailureReason)
					  .HasMaxLength(255);

				// FK to User
				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserID)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne<TenantInfo>()
				  .WithMany()
				  .HasForeignKey(e => e.TenantId)
				  .OnDelete(DeleteBehavior.NoAction);
			});
			modelBuilder.Entity<MfaSetting>(entity =>
			{
				entity.ToTable("MfaSetting", _schema);
				entity.HasKey(e => e.MFASettingID);
				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.UserID)
					  .IsRequired();

				entity.Property(e => e.IsMFAEnabled)
					  .IsRequired();

				entity.Property(e => e.PreferredMFAType)
					  .HasMaxLength(50);

				// FK to User
				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserID)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne<TenantInfo>()
				  .WithMany()
				  .HasForeignKey(e => e.TenantId)
				  .OnDelete(DeleteBehavior.NoAction);
			});
			modelBuilder.Entity<SendToken>(entity =>
			{
				entity.ToTable("SendToken", _schema);
				entity.HasKey(e => e.SendTokenID);
				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.UserID)
					  .IsRequired();

				entity.Property(e => e.MFADeviceID)
					  .IsRequired();

				entity.Property(e => e.UserTokenID)
					  .IsRequired();

				entity.Property(e => e.SendAt)
					  .IsRequired();

				entity.Property(e => e.SendSuccessful);

				// FK to User
				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserID)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne<TenantInfo>()
				  .WithMany()
				  .HasForeignKey(e => e.TenantId)
				  .OnDelete(DeleteBehavior.NoAction);
			});

			// SentNotification
			modelBuilder.Entity<SentNotification>(entity =>
			{
				entity.ToTable("SentNotification", _schema);
				entity.HasKey(e => e.NotificationId);
				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.Recipient)
					  .IsRequired();

				entity.Property(e => e.NotificationType)
					  .IsRequired();

				entity.Property(e => e.Subject)
					  .IsRequired();

				entity.Property(e => e.Message)
					  .IsRequired();

				entity.Property(e => e.SentAt)
					  .IsRequired()
					  .HasDefaultValueSql("GETDATE()");

				entity.Property(e => e.IsSuccess)
					  .IsRequired();

				entity.HasOne<TenantInfo>()
				  .WithMany()
				  .HasForeignKey(e => e.TenantId)
				  .OnDelete(DeleteBehavior.NoAction);
			});
			modelBuilder.Entity<UserToken>(entity =>
			{
				entity.ToTable("UserToken", _schema);
				entity.HasKey(e => e.TokenID);
				entity.Property(e => e.TenantId)
					  .IsRequired();	

				entity.Property(e => e.UserID)
					  .IsRequired();

				entity.Property(e => e.Token)
					  .HasMaxLength(1000)
					  .IsRequired();

				entity.Property(e => e.TokenType)
					  .HasMaxLength(50)
					  .IsRequired();

				entity.Property(e => e.CreatedAt);

				entity.Property(e => e.ExpiresAt);

				entity.Property(e => e.IsUsed);

				entity.Property(e => e.IsRevoked);

				entity.Property(e => e.Purpose)
					  .HasMaxLength(255);

				entity.Property(e => e.LastUsedAt);

				// Foreign key relationship to User
				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserID)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne<TenantInfo>()
				  .WithMany()
				  .HasForeignKey(e => e.TenantId)
				  .OnDelete(DeleteBehavior.NoAction);
			});

			modelBuilder.Entity<UserRequest>(entity =>
			{
				entity.ToTable("UserRequest", _schema);
				entity.HasKey(e => e.UserRequestId);

				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.Username)
					  .IsRequired();

				entity.Property(e => e.FullName)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.PasswordHash);

				entity.Property(e => e.UserRoleId)
					  .IsRequired();

				entity.Property(e => e.Email)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.PhoneNumber)
					  .HasMaxLength(50);

				entity.Property(e => e.Status)
					  .HasConversion<int>() // enum <-> int
					  .IsRequired()
					  .HasDefaultValue(UserRequestStatus.Pending);


				entity.Property(e => e.CreatedAt)
					  .HasDefaultValueSql("GETUTCDATE()");

				entity.Property(e => e.ReviewedAt);

				entity.Property(e => e.ReviewedBy);

				entity.Property(e => e.ReviewComments);

				entity.Property(e => e.RejectionReason);

				
				// Relationships (if you have UserRole or Admin User tables)
				entity.HasOne<UserRole>()
					  .WithMany()
					  .HasForeignKey(e => e.UserRoleId)
					  .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.ReviewedBy)
					  .OnDelete(DeleteBehavior.SetNull);

				entity.HasOne<TenantInfo>()
				  .WithMany()
				  .HasForeignKey(e => e.TenantId)
				  .OnDelete(DeleteBehavior.NoAction);
			});
			modelBuilder.Entity<Person>(entity =>
			{
				entity.ToTable("Person", _schema);
				entity.HasKey(e => e.PersonId);

				entity.Property(e => e.TenantId).IsRequired();
				entity.Property(e => e.PersonType).HasConversion<int>().IsRequired();
				entity.Property(e => e.PersonNumber).IsRequired().HasMaxLength(50);
				entity.Property(e => e.Name).HasMaxLength(200);
				entity.Property(e => e.NICNumber).HasMaxLength(50); // Can be null
				entity.Property(e => e.JoinedDate);
				entity.Property(e => e.DepartmentId).IsRequired(); 
				entity.Property(e => e.DesignationId); // Already nullable
				entity.Property(e => e.EmployeeGrade).HasMaxLength(50);
				entity.Property(e => e.PersonSubType).HasMaxLength(50);
				entity.Property(e => e.Gender).HasMaxLength(10);
				entity.Property(e => e.WhatsappNumber).HasMaxLength(50);
				entity.Property(e => e.Email).HasMaxLength(100);
				entity.Property(e => e.MealGroup).HasMaxLength(50);
				entity.Property(e => e.MealEligibility).HasDefaultValue(false);
				entity.Property(e => e.IsActive).HasDefaultValue(true);
				entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
				entity.Property(e => e.SpecialNote).HasMaxLength(500);

				// Foreign key relationships
				entity.HasOne<Department>()
					  .WithMany()
					  .HasForeignKey(e => e.DepartmentId)
					  .OnDelete(DeleteBehavior.NoAction); // Because DepartmentId can now be null

				entity.HasOne<Designation>()
					  .WithMany()
					  .HasForeignKey(e => e.DesignationId)
					  .OnDelete(DeleteBehavior.SetNull); // Keeps nullable

				entity.HasOne<TenantInfo>()
					  .WithMany()
					  .HasForeignKey(e => e.TenantId)
					  .OnDelete(DeleteBehavior.NoAction);

				// Indexes
				entity.HasIndex(e => new { e.TenantId, e.PersonNumber }).IsUnique();
				entity.HasIndex(e => new { e.TenantId, e.NICNumber })
					  .IsUnique()
					  .HasFilter("[NICNumber] IS NOT NULL");
				entity.HasIndex(e => e.PersonType);
			});


			modelBuilder.Entity<Supplier>(entity =>
			{
				entity.ToTable("Supplier", _schema);

				entity.HasKey(s => s.SupplierId);

				entity.Property(s => s.TenantId)
					  .IsRequired();

				entity.Property(s => s.SupplierName)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(s => s.ContactNumber)
					  .HasMaxLength(50);

				entity.Property(s => s.Email)
					  .HasMaxLength(150);

				entity.Property(s => s.Address)
					  .HasMaxLength(300);

				entity.Property(s => s.SupplierRating)
					  .HasDefaultValue(1); // Default rating if not provided

				entity.Property(s => s.IsActive)
					  .HasDefaultValue(true);

				entity.Property(s => s.CreatedAt)
					  .HasDefaultValueSql("GETDATE()");

				entity.HasOne<TenantInfo>()
					  .WithMany()
					  .HasForeignKey(e => e.TenantId)
					  .OnDelete(DeleteBehavior.NoAction);
			});
			modelBuilder.Entity<MealType>(entity =>
			{
				entity.ToTable("MealType", _schema);
				entity.HasKey(e => e.MealTypeId);
				entity.Property(e => e.TenantId)
					  .IsRequired();
				entity.Property(e => e.TypeName)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.Description)
					  .HasMaxLength(500);

				entity.Property(e => e.TokenIssueStartDate)
					  .HasMaxLength(10);

				entity.Property(e => e.TokenIssueEndDate)
					  .HasMaxLength(10);

				entity.Property(e => e.MealTimeStartDate)
					  .HasMaxLength(10);

				entity.Property(e => e.MealTimeEndDate)
					  .HasMaxLength(10);

				entity.Property(e => e.CreatedAt)
					  .HasDefaultValueSql("GETUTCDATE()");

				entity.Property(e => e.UpdatedAt)
					  .IsRequired(false);

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);

                entity.HasOne<TenantInfo>()
					  .WithMany()
					  .HasForeignKey(e => e.TenantId)
					  .OnDelete(DeleteBehavior.NoAction);
			});

			modelBuilder.Entity<MealSubType>(entity =>
			{
				entity.ToTable("MealSubType", _schema);
				entity.HasKey(e => e.MealSubTypeId);
				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.MealTypeId)
					.IsRequired();

				entity.Property(e => e.SubTypeName)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.Description)
					  .HasMaxLength(500);

				entity.Property(e => e.Functionkey)
					  .HasMaxLength(50);

				entity.Property(e => e.CreatedAt)
					  .HasDefaultValueSql("GETUTCDATE()");
				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);

                entity.HasOne<TenantInfo>()
					  .WithMany()
					  .HasForeignKey(e => e.TenantId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<MealType>()
					  .WithMany()
					  .HasForeignKey(e => e.MealTypeId)
					  .OnDelete(DeleteBehavior.NoAction);
			});
			modelBuilder.Entity<MealAddOn>(entity =>
			{
				entity.ToTable("MealAddOn", _schema);
				entity.HasKey(e => e.MealTypeAddOnId);
				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.MealTypeId)
					.IsRequired();

                entity.Property(e => e.AddOnSubTypeId)
                    .IsRequired();
                entity.Property(e => e.AddOnName)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(e => e.AddOnType)
					  .HasConversion<string>() // saves enum as string
					  .IsRequired();

				entity.HasOne<TenantInfo>()
					  .WithMany()
					  .HasForeignKey(e => e.TenantId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<MealType>()
					  .WithMany()
					  .HasForeignKey(e => e.MealTypeId)
					  .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<MealSubType>()
                      .WithMany()
                      .HasForeignKey(e => e.AddOnSubTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

			modelBuilder.Entity<MealCost>(entity =>
			{
				entity.ToTable("MealCost", _schema);

				entity.HasKey(e => e.MealCostId);
				entity.Property(e => e.TenantId)
						.IsRequired();
				entity.Property(e => e.SupplierId)
						.IsRequired();
				entity.Property(e => e.MealTypeId)
						.IsRequired();
				entity.Property(e => e.MealSubTypeId);

				entity.Property(e => e.SupplierCost)
					.HasColumnType("decimal(18,2)").IsRequired();
				entity.Property(e => e.SellingPrice)
					.HasColumnType("decimal(18,2)").IsRequired();
				entity.Property(e => e.CompanyCost)
					.HasColumnType("decimal(18,2)").IsRequired();
				entity.Property(e => e.EmployeeCost)
					.HasColumnType("decimal(18,2)").IsRequired();

				entity.Property(e => e.Description).HasMaxLength(500);
				entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
				
                entity.HasOne<Supplier>() 
					  .WithMany()
					  .HasForeignKey(e => e.SupplierId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne<MealType>() 
					  .WithMany()
					  .HasForeignKey(e => e.MealTypeId)
					  .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne<MealSubType>()
					  .WithMany()
					  .HasForeignKey(e => e.MealSubTypeId)
					  .OnDelete(DeleteBehavior.SetNull);

				entity.HasOne<TenantInfo>() 
					  .WithMany()
					  .HasForeignKey(e => e.TenantId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasIndex(e => new { e.TenantId, e.MealTypeId, e.MealSubTypeId, e.SupplierId })
					  .IsUnique(); 
			});
			modelBuilder.Entity<ClientDevice>(entity =>
			{
				entity.ToTable("ClientDevice", _schema);
				entity.HasKey(e => e.ClientDeviceId);
				entity.Property(e => e.TenantId)
					  .IsRequired();

				// Configure properties
				entity.Property(e => e.DeviceName)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.IpAddress)
					.IsRequired()
					.HasMaxLength(45);

				entity.Property(e => e.SerialNo)
					.HasMaxLength(50);

				entity.Property(e => e.PrinterName)
					.HasMaxLength(100);

				entity.Property(e => e.IsActive)
					.HasDefaultValue(true);

				entity.Property(e => e.Port)
					.IsRequired();

				entity.Property(e => e.MachineNumber)
					.IsRequired();

				entity.Property(e => e.ReceiptHeightPixels)
					.HasDefaultValue(0);

				entity.Property(e => e.ReceiptWidthPixels)
					.HasDefaultValue(0);
				entity.Property(e => e.DeviceShift)
					.HasConversion<string>()
					.HasMaxLength(50);


                entity.HasOne<TenantInfo>()
				  .WithMany()
				  .HasForeignKey(e => e.TenantId)
				  .OnDelete(DeleteBehavior.NoAction);

			});
			modelBuilder.Entity<Schedule>(entity =>
			{
				entity.ToTable("Schedule", _schema);
				entity.HasKey(e => e.SheduleId);

				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.SheduleName)
					  .IsRequired()
					  .HasMaxLength(200);

				entity.Property(e => e.ShedulePeriod)
					  .HasMaxLength(100);

				entity.Property(e => e.Note)
					  .HasMaxLength(500);

				entity.Property(e => e.IsActive)
					  .HasDefaultValue(true);
			});

			modelBuilder.Entity<ScheduleMeal>(entity =>
			{
				entity.ToTable("ScheduleMeal", _schema);
				entity.HasKey(e => e.SheduleMealSubTypeId);

				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.SheduleId)
					  .IsRequired();

				entity.Property(e => e.MealTypeId)
					  .IsRequired();

				entity.Property(e => e.MealSubTypeId)
					  .IsRequired(false);

				entity.Property(e => e.SupplierId)
					  .IsRequired();

				entity.Property(e => e.FunctionKey)
					  .HasMaxLength(50);

				entity.Property(e => e.IsFunctionKeysEnable)
					  .HasDefaultValue(false);

				entity.Property(e => e.IsAvailable)
					  .HasDefaultValue(true);

				entity.HasOne<Schedule>()
					  .WithMany(s => s.SheduleMeals)
					  .HasForeignKey(e => e.SheduleId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<TenantInfo>()  
					  .WithMany()
					  .HasForeignKey(e => e.TenantId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<MealType>()
					  .WithMany()
					  .HasForeignKey(e => e.MealTypeId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<MealSubType>()
					  .WithMany()
					  .HasForeignKey(e => e.MealSubTypeId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<Supplier>()
					  .WithMany()
					  .HasForeignKey(e => e.SupplierId)
					  .OnDelete(DeleteBehavior.NoAction);
			});
			modelBuilder.Entity<ScheduleDate>(entity =>
			{
				entity.ToTable("ScheduleDate", _schema);
				entity.HasKey(e => e.SheduleDateId);

				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.SheduleId)
					  .IsRequired();

				entity.Property(e => e.Date)
					  .IsRequired();

				// 🔹 Relationships
				entity.HasOne<TenantInfo>()
					  .WithMany()
					  .HasForeignKey(e => e.TenantId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<Schedule>()
					  .WithMany(s => s.SheduleDates)
					  .HasForeignKey(e => e.SheduleId)
					  .OnDelete(DeleteBehavior.NoAction);
			});
			modelBuilder.Entity<SchedulePerson>(entity =>
			{
				entity.ToTable("SchedulePerson", _schema);
				entity.HasKey(e => e.ShedulePersonId);

				entity.Property(e => e.TenantId)
					  .IsRequired();

				entity.Property(e => e.PersonId)
					  .IsRequired();

				entity.Property(e => e.SheduleId)
					  .IsRequired();

				// 🔹 Relationships
				entity.HasOne<TenantInfo>()
					  .WithMany()
					  .HasForeignKey(e => e.TenantId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<Schedule>()
					  .WithMany()
					  .HasForeignKey(e => e.SheduleId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<Person>()
					.WithMany()
					.HasForeignKey(e => e.PersonId)
					.OnDelete(DeleteBehavior.NoAction);
			});
            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Request", _schema);
                entity.HasKey(e => e.MealRequestId);
                entity.Property(e => e.TenantId)
                  .IsRequired();

                entity.Property(e => e.EventDate)
                      .IsRequired();

                entity.Property(e => e.EventType)
                      .HasMaxLength(100) // optional constraint
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.Property(e => e.NoofAttendees)
                      .IsRequired();

                entity.Property(e => e.Status)
                      .IsRequired();

                entity.Property(e => e.RequesterId)
                      .IsRequired();

                entity.HasOne<TenantInfo>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<User>() 
                      .WithMany()
                      .HasForeignKey(e => e.RequesterId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.ApproverOrRejectedId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // 🔹 RequestMeal entity
            modelBuilder.Entity<RequestMeal>(entity =>
            {
                entity.ToTable("RequestMeal", _schema);
                entity.HasKey(e => e.RequestMealId);

                entity.Property(e => e.TenantId)
                  .IsRequired();
				entity.Property(e => e.RequestId)
				.IsRequired();

                entity.Property(e => e.MealTypeId)
                      .IsRequired();

                entity.Property(e => e.SubTypeId)
                      .IsRequired(false);

                entity.Property(e => e.MealCostId)
                      .IsRequired();

                entity.Property(e => e.Quantity)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.HasOne<TenantInfo>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<MealType>()  // assuming you have a MealType entity
                      .WithMany()
                      .HasForeignKey(e => e.MealTypeId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<MealSubType>() // assuming you have a MealSubType entity
                      .WithMany()
                      .HasForeignKey(e => e.SubTypeId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<MealCost>() // assuming you have a MealCost entity
                      .WithMany()
                      .HasForeignKey(e => e.MealCostId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Request>() // assuming you have a MealCost entity
                      .WithMany()
                      .HasForeignKey(e => e.RequestId)
                      .OnDelete(DeleteBehavior.NoAction);

            });
            modelBuilder.Entity<MealConsumption>(entity =>
            {
                entity.ToTable("MealConsumption", _schema);
                entity.HasKey(e => e.MealConsumptionId);

                // Basic required properties
                entity.Property(e => e.TenantId)
                      .IsRequired();

                entity.Property(e => e.PersonId)
                      .IsRequired();

                entity.Property(e => e.Gender)
                      .IsRequired(false);

                entity.Property(e => e.PersonName)
                      .HasMaxLength(200)
                      .IsRequired(false);

                entity.Property(e => e.Date)
                      .IsRequired();

                entity.Property(e => e.Time)
                      .IsRequired();

                entity.Property(e => e.ScheduleId)
                      .IsRequired();

                entity.Property(e => e.ScheduleName)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.AddOnMeal)
                      .IsRequired();

                entity.Property(e => e.MealTypeId)
                      .IsRequired();
                entity.Property(e => e.MealTypeName)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.SubTypeId)
                      .IsRequired(false);

                entity.Property(e => e.SubTypeName)
                      .HasMaxLength(100)
                      .IsRequired(false);

                entity.Property(e => e.MealCostId)
                      .IsRequired();

                // Cost properties
                entity.Property(e => e.SupplierCost)
                      .HasPrecision(10, 2)
                      .IsRequired();

                entity.Property(e => e.SellingPrice)
                      .HasPrecision(10, 2)
                      .IsRequired();

                entity.Property(e => e.CompanyCost)
                      .HasPrecision(10, 2)
                      .IsRequired();

                entity.Property(e => e.EmployeeCost)
                      .HasPrecision(10, 2)
                      .IsRequired();

                // Device info
                entity.Property(e => e.DeviceId)
                      .IsRequired();

                entity.Property(e => e.DeviceSerialNo)
                      .HasMaxLength(100)
                      .IsRequired();

                // Enums
                entity.Property(e => e.ShiftName)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.PayStatus)
                      .HasConversion<string>()
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(e => e.TockenIssued)
                      .IsRequired();

				entity.Property(e => e.JobStatus);

                // Relationships (optional — if navigations exist)
                entity.HasOne<TenantInfo>()
                      .WithMany()
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Person>()
                      .WithMany()
                      .HasForeignKey(e => e.PersonId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<MealType>()
                      .WithMany()
                      .HasForeignKey(e => e.MealTypeId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<MealSubType>()
                      .WithMany()
                      .HasForeignKey(e => e.SubTypeId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<MealCost>()
                      .WithMany()
                      .HasForeignKey(e => e.MealCostId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<ClientDevice>()
                      .WithMany()
                      .HasForeignKey(e => e.DeviceId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<PayStatusByShift>(entity =>
            {
                entity.ToTable("PayStatusByShiftPolicy", _schema); // Renaming the table for clarity
                entity.HasKey(e => e.PolicyId);

                entity.Property(e => e.ShiftType)
                    .HasConversion<string>()
                    .HasMaxLength(100) 
                    .IsRequired();

                entity.Property(e => e.MealTypeId)
                    .IsRequired();

                entity.Property(e => e.IsMalePaid)
                    .IsRequired();

                entity.Property(e => e.IsFemalePaid)
                    .IsRequired();

                entity.HasOne<MealType>()
                    .WithMany()
                    .HasForeignKey(e => e.MealTypeId)
                    .OnDelete(DeleteBehavior.NoAction);

            });
            modelBuilder.Entity<UserDepartment>(entity =>
            {
                entity.ToTable("UserDepartment", _schema); 

                entity.HasKey(e => e.UserDepartmentId);

                entity.Property(e => e.TenantId)
                     .IsRequired();
                entity.Property(e => e.UserRequestId)
                      .IsRequired();

                entity.Property(e => e.UserId)
                      .IsRequired(false);

                entity.Property(e => e.DepartmentId)
                      .IsRequired();

                entity.Property(e => e.RequestStatus)
                      .HasConversion<int>() // store enum as int
                      .IsRequired();

                entity.HasOne<TenantInfo>()
                       .WithMany()
                       .HasForeignKey(e => e.TenantId)
                       .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Department>()
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
			modelBuilder.Entity<UserHistory>(entity =>
			{
				entity.ToTable("UserHistory", _schema);
				entity.HasKey(e => e.UserHistoryId);
				entity.Property(e => e.TenantId).IsRequired();
				entity.Property(e => e.UserId).IsRequired();
				entity.Property(e => e.ActionType).IsRequired();
				entity.Property(e => e.EntityType);
				entity.Property(e => e.Endpoint);
				entity.Property(e => e.Timestamp).IsRequired();
				entity.Property(e => e.IPAddress);

				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<TenantInfo>()
					   .WithMany()
					   .HasForeignKey(e => e.TenantId)
					   .OnDelete(DeleteBehavior.NoAction);


			});
			
				/*modelBuilder.Entity<RequestMealConsumption>(entity =>
			{
				entity.ToTable("RequestMealConsumption", _schema);
				entity.HasKey(e => e.UserHistoryId);
				entity.Property(e => e.TenantId).IsRequired();
				entity.Property(e => e.UserId).IsRequired();
				entity.Property(e => e.ActionType).IsRequired();
				entity.Property(e => e.EntityType);
				entity.Property(e => e.Endpoint);
				entity.Property(e => e.Timestamp).IsRequired();
				entity.Property(e => e.IPAddress);

				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserId)
					  .OnDelete(DeleteBehavior.NoAction);

				entity.HasOne<TenantInfo>()
					   .WithMany()
					   .HasForeignKey(e => e.TenantId)
					   .OnDelete(DeleteBehavior.NoAction);


			});*/

		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// Connection string is already configured in Program.cs
			// Only override if tenant has a specific connection string
			if (_tenantContext.CurrentTenant != null &&
				!string.IsNullOrEmpty(_tenantContext.CurrentTenant.ConnectionString))
			{
				optionsBuilder.UseSqlServer(_tenantContext.CurrentTenant.ConnectionString);
			}
		}
	}
}
