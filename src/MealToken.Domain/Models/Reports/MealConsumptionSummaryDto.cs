using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class MealConsumptionSummaryDto
	{
		public List<MealConsumptionData> MealConsumptionDetails { get; set; }
		public int TotalMealServed { get; set; }
		public decimal TotalEmployeesContribution { get; set; }
		public decimal TotalSupplierContribution { get; set; }
		public decimal TotalCompanyContribution { get; set; }
		
	}
	public class MealConsumptionData
	{
		public DateOnly Date { get; set; }
		public string MealType { get; set; }
		public string SubType { get; set; }
		public decimal EmployeeContribution { get; set; }
		public decimal CompanyContribution { get; set; }
		public decimal SupplierContribution { get; set; }
		public int PersonCount { get; set; }
		public int MaleCount { get; set; }
		public int FemaleCount { get; set; }
		public decimal TotalEmployeeContribution { get; set; }
		public decimal TotalCompanyContribution { get; set; }
		public decimal TotalSupplierCost { get; set; }
		public int TotalMealCount { get; set; }
	}
}
