using MealToken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Interfaces
{
	public interface ITenantContext
	{
		int? TenantId { get; }
		string? SchemaName { get; }
		TenantInfo? CurrentTenant { get; }
	}
}
