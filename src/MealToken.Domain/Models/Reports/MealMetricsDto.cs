using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class MealMetricsDto
	{
		public int TotalMealsServed { get; set; }
		public decimal PercentageChange { get; set; }
		public bool IsIncrease { get; set; }

		public decimal TotalCost { get; set; }
		public decimal CostChange { get; set; }
		public bool IsCostIncrease { get; set; }

		public int TotalSpecialRequests { get; set; }
		public int ApprovedRequests { get; set; }
		public int PendingRequests { get; set; }
		public decimal RequestsMealsCount	{ get; set; }
		public decimal RequestsCost { get; set; }
	}
}
