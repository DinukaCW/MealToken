using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class DashboardSupplier
	{
		public int TotalMeals { get; set; }
		public decimal TotalSupplierSellingPrice { get; set; }
		public List<SupplierWiseMeals> SupplierWiseMeals { get; set; }
	}
	public class SupplierWiseMeals
	{
		public int SupplierId { get; set; }
		public string SupplierName { get; set; }
		public int MealCount { get; set; }
		public decimal Precentage { get; set; }
		public decimal SupplierSellingPrice { get; set; }
	}
}
