using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class MealCostDistribution
	{
		public List<GraphDataPoint> DailyData { get; set; }
	}

	public class GraphDataPoint
	{
		public string Label { get; set; }
		public int MealCount { get; set; }
		public decimal TotalCost { get; set; }
	}
}
