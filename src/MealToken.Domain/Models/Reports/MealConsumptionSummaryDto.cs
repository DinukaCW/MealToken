using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class MealConsumptionSummaryDto
	{
		public DateOnly Date { get; set; }
		public string MealType { get; set; }
		public string SubType { get; set; }
		public int EmployeeCount { get; set; }
		public int MaleCount { get; set; }
		public int FemaleCount { get; set; }
		public decimal TotalEmployeeContribution { get; set; }
		public decimal TotalEmployerContribution { get; set; }
		public decimal TotalSupplierCost { get; set; }
		public int TotalMealCount { get; set; }
		public string RowType { get; set; }
	}
}
