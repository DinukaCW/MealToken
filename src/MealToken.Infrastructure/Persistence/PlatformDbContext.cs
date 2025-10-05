using Authentication.Models.Entities;
using MealToken.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Infrastructure.Persistence
{

	public class PlatformDbContext : DbContext
	{
		private readonly IConfiguration _configuration;

		public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
			: base(options)
		{
		}
		public virtual DbSet<TenantInfo> TenantInfo { get; set; }
		public virtual DbSet<UserRole> UserRole { get; set; }
		public virtual DbSet<Message> Message { get; set; }
		public DbSet<Department> Department { get; set; }
		public DbSet<Designation> Designation { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//modelBuilder.Entity<TenantInfo>().ToTable("TenantInfo");

			modelBuilder.Entity<TenantInfo>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Name)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.Subdomain)
					  .HasMaxLength(100)
					  .IsRequired();

				entity.Property(e => e.SchemaName)
					  .HasMaxLength(100)
					  .IsRequired();

				entity.Property(e => e.ConnectionString)
					  .HasMaxLength(500)
					  .IsRequired();

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				entity.Property(e => e.CreatedAt)
					  .IsRequired()
					  .HasDefaultValueSql("GETUTCDATE()");

				// Indexes
				entity.HasIndex(e => e.Subdomain).IsUnique();
				entity.HasIndex(e => e.SchemaName).IsUnique();
			});
			modelBuilder.Entity<UserRole>(entity =>
			{
				// Primary Key
				entity.HasKey(e => e.UserRoleID);

				// Properties
				entity.Property(e => e.UserRoleName)
					  .HasMaxLength(100)
					  .IsRequired();

				entity.Property(e => e.Description)
					  .HasMaxLength(500);

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);

				// Index
				entity.HasIndex(e => e.UserRoleName).IsUnique();
			});
			modelBuilder.Entity<Message>(entity =>
			{
				// Primary Key
				entity.HasKey(e => e.MessageId);

				// Properties
				entity.Property(e => e.MessageName)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.MessageBody)
					  .HasMaxLength(2000)
					  .IsRequired();
			});

			modelBuilder.Entity<Department>(entity =>
			{
				// Primary Key
				entity.HasKey(e => e.DepartmnetId);

				// Properties
				entity.Property(e => e.Name)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.Description)
					  .HasMaxLength(2000)
					  .IsRequired();

				entity.Property(e => e.IsActive)
					  .IsRequired()
					  .HasDefaultValue(true);
			});
			modelBuilder.Entity<Designation>(entity =>
			{
				// Primary Key
				entity.HasKey(e => e.DesignationId);

				// Properties
				entity.Property(e => e.Title)
					  .HasMaxLength(255)
					  .IsRequired();

				entity.Property(e => e.Description)
					  .HasMaxLength(2000)
					  .IsRequired();

				entity.Property(e => e.IsActive)
				  .IsRequired()
				  .HasDefaultValue(true);
			});

			base.OnModelCreating(modelBuilder);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// use hard-coded connection for migrations
			//var localConnection = "Data Source=207.180.217.101\\SQLEXPRESS;Initial Catalog=TestDBH;User ID=cln_db_app;Password=bDHTqcNHJAMHIPjZRuSj;TrustServerCertificate=True";// _configuration.Value.PlatformConnectionString;
			if (!optionsBuilder.IsConfigured)
			{
				var localConnection = _configuration.GetConnectionString("DefaultConnection");
				optionsBuilder.UseSqlServer(localConnection);
			}
			//optionsBuilder.UseSqlServer(localConnection);
			//if (!optionsBuilder.IsConfigured)
			//{
			//    var localConnection = _configuration.Value.PlatformConnectionString;

			//    optionsBuilder.UseSqlServer(localConnection); 
			//}
		}
	}
}
