using MealToken.Application.Services;
using MealToken.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Infrastructure.Persistence
{
	public class MealTokenDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MealTokenDbContext>
	{
        public MealTokenDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found. " +
                    "Ensure appsettings.json exists in the current directory with a DefaultConnection string.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<MealTokenDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Create a dummy tenant context for design-time migrations
            var tenantContext = new TenantContext();

            // Get schema name from configuration or use default
            var schemaName = configuration["MigrationSettings:DefaultSchema"] ?? "dbo";
            var tenantName = configuration["MigrationSettings:TenantName"] ?? "DesignTimeTenant";

            tenantContext.SetTenant(new TenantInfo
            {
                Id = 1,
                SchemaName = schemaName,
                ConnectionString = connectionString,
                Name = tenantName,
                Subdomain = "design-time",
                IsActive = true
            });

            return new MealTokenDbContext(optionsBuilder.Options, tenantContext);
        }
    }
}
