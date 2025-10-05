using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class MealCostDetails
	{
		public int MealCostId { get; set; }
		public int SupplierId { get; set; }
		public string SupplierName { get; set; }
		public int MealTypeId { get; set; }
		public string MealTypeName { get; set; }
		public int? MealSubTypeId { get; set; }
		public string? MealSubTypeName { get; set; }

		public decimal SupplierCost { get; set; }
		public decimal SellingPrice { get; set; }
		public decimal CompanyCost { get; set; }
		public decimal EmployeeCost { get; set; }

		public string? Description { get; set; }
	}
}
