using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class DashBoardCostAnalysis
	{
		public decimal EmployeesCost { get; set; }
		public decimal CompanyCost { get; set; }
		public decimal SellingPrice { get; set; }
		public decimal SupplierCost { get; set; }
		public decimal ProfitAmount { get; set; }
	}
}
