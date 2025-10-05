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
	public class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
	{
		public PlatformDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<PlatformDbContext>();

			// Hardcode or load connection string here for design-time
			var connectionString = "Server=207.180.217.101,1433;Initial Catalog=MealTokenDB;Persist Security Info=False;User ID=peoplehubadmin;Password=u1mXITgbcGH94v7bzLgA;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";

			optionsBuilder.UseSqlServer(connectionString);

			return new PlatformDbContext(optionsBuilder.Options);
		}
	}
}
