using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class SupplierPaymentReportDto
	{
		public string SupplierName { get; set; }
		public string ContactNumber { get; set; }
		public string Address { get; set; }
		public List<SupplierMealDetailDto> MealDetails { get; set; }
		public SupplierSummaryDto Summary { get; set; }
	}

	public class SupplierMealDetailDto
	{
		public DateOnly Date { get; set; }
		public string MealType { get; set; }
		public string SubMealType { get; set; }
		public decimal UnitPrice { get; set; }
		public int QuantityMale { get; set; }
		public int QuantityFemale { get; set; }
		public int TotalQuantity { get; set; }
		public decimal Amount { get; set; }
	}

	public class SupplierSummaryDto
	{
		public int TotalPersonCount { get; set; }
		public int MaleCount { get; set; }
		public int FemaleCount { get; set; }
		public decimal TotalEmployeeContribution { get; set; }
		public decimal TotalEmployerCost { get; set; }
		public decimal TotalSupplierCost { get; set; }
		public int TotalMealCount { get; set; }
	}
}
