using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Services
{
	public class TenantContext : ITenantContext
	{
		public int? TenantId { get; private set; }
		public string? SchemaName { get; private set; }
		public TenantInfo? CurrentTenant { get; private set; }

		public void SetTenant(TenantInfo tenant)
		{
			CurrentTenant = tenant;
			TenantId = tenant.Id;
			SchemaName = tenant.SchemaName;
		}
	}
}
