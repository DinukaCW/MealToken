using MealToken.Application.Interfaces;
using MealToken.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Infrastructure.Persistence
{
	public class TenantDbContextFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public TenantDbContextFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public MealTokenDbContext CreateDbContext(string tenantId)
		{
			var tenantService = _serviceProvider.GetRequiredService<ITenantService>();
			var tenant = tenantService.GetTenantByIdAsync(tenantId).Result;

			if (tenant == null)
				throw new InvalidOperationException($"Tenant {tenantId} not found");

			var optionsBuilder = new DbContextOptionsBuilder<MealTokenDbContext>();
			optionsBuilder.UseSqlServer(tenant.ConnectionString);

			var tenantContext = new TenantContext();
			tenantContext.SetTenant(tenant);

			return new MealTokenDbContext(optionsBuilder.Options, tenantContext);
		}
	}
}
