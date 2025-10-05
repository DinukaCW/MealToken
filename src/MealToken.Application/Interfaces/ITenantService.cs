using MealToken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Interfaces
{
	public interface ITenantService
	{
		Task<TenantInfo?> GetTenantBySubdomainAsync(string subdomain);
		Task<TenantInfo?> GetTenantByIdAsync(string tenantId);
		Task<bool> TenantExistsAsync(string subdomain);
	}
}
