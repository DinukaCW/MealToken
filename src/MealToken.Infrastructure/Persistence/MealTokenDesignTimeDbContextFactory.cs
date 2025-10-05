using MealToken.Application.Services;
using MealToken.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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
			var optionsBuilder = new DbContextOptionsBuilder<MealTokenDbContext>();

			// Use a default / development connection string
			var defaultConnection = "Server=207.180.217.101,1433;Initial Catalog=MealTokenDB;Persist Security Info=False;User ID=peoplehubadmin;Password=u1mXITgbcGH94v7bzLgA;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
			optionsBuilder.UseSqlServer(defaultConnection);

			// Create a dummy tenant context for design-time
			var tenantContext = new TenantContext();
			tenantContext.SetTenant(new TenantInfo
			{
				Id = 1,
				SchemaName = "dbo",
				ConnectionString = defaultConnection,
				Name = "DesignTimeTenant",
				Subdomain = "design-time",
				IsActive = true
			});

			return new MealTokenDbContext(optionsBuilder.Options, tenantContext);
		}
	}
}
