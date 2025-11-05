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
		public List<SupplierRequestDetailDto> RequestDetails { get; set; }
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

	public class SupplierRequestDetailDto
	{
		public DateOnly EventDate { get; set; }
		public string EventType { get; set; }
		public string Description { get; set; }
		public string MealType { get; set; }
		public string SubMealType { get; set; }
		public decimal Quantity { get; set; }
		public decimal SellingPrice { get; set; }
	}

	public class SupplierSummaryDto
	{
		public int TotalPersonCount { get; set; }
		public int MaleCount { get; set; }
		public int FemaleCount { get; set; }
		public int RequestsCount { get; set; }
		public int TotalMealCount { get; set; }
		public decimal TotalEmployeeContribution { get; set; }
		public decimal TotalCompanyContribution { get; set; }
		public decimal TotalSupplierCost { get; set; }
		public decimal TotalSellingPrice { get; set; }
		
	}

	public class SupplierRequestCosts
	{
		public decimal TotalEmployeeContribution { get; set; }
		public decimal TotalCompanyContribution { get; set; }
		public decimal TotalSupplierCost { get; set; }
		public decimal TotalSellingPrice { get; set; }
	}
}
