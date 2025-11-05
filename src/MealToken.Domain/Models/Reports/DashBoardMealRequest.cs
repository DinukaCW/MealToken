using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class DashBoardMealRequest
	{
		public List<GraphDataPoint> MealRequestCostDetails { get; set; }
		public List<MealTypeDistributionDto> RequestMealTypesDetails { get; set; }
	}
}
