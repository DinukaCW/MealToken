using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class MealDashboardDto
	{
		public MealMetricsDto Metrics { get; set; }
		public MealCostDistribution MealCosts{ get; set; }
		public MealTypeDistribution MealTypes { get; set; }
	}
}
