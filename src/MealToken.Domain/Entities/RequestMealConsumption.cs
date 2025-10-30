using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class RequestMealConsumption
	{
		public int RequestId { get; set; }
		public int TenantId { get; set; }
		public int MealTypeId { get; set; }
		public int? SubTypeId { get; set; }
		public int MealCostId { get; set; }
		public decimal Quantity { get; set; }
		public int SupplierId { get; set; }
		public decimal TotalEmployeeContribution { get; set; }
		public decimal TotalCompanyContribution { get; set; }
		public decimal TotalSupplierContribution { get; set; }
	}
}
