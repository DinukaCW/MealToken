using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class RequestMealConsumption
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int RequestMealConsumptionId { get; set; }
		public int TenantId { get; set; }
		public int RequestId { get; set; }
		public string EventType { get; set; }
		public string EventDescription { get; set; }
		public int MealTypeId { get; set; }
		public int? SubTypeId { get; set; }
		public int MealCostId { get; set; }
		public DateOnly EventDate { get; set; }
		public int Quantity { get; set; }
		public int SupplierId { get; set; }
		public decimal TotalEmployeeContribution { get; set; }
		public decimal TotalCompanyContribution { get; set; }
		public decimal TotalSupplierCost { get; set; }
		public decimal TotalSellingPrice { get; set; }
	}
}
