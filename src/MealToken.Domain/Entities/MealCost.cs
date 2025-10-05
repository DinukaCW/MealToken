using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class MealCost
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int MealCostId { get; set; }
		public int TenantId { get; set; }
		public int SupplierId { get; set; }
		public int MealTypeId { get; set; }
		public int? MealSubTypeId { get; set; } // Nullable - can be null when meal type has no sub types

		// Cost breakdown
		public decimal SupplierCost { get; set; } // Cost in LKR
		public decimal SellingPrice { get; set; } // Total selling price in LKR
		public decimal CompanyCost { get; set; } // Company contribution in LKR
		public decimal EmployeeCost { get; set; } // Employee contribution in LKR

		public string? Description { get; set; }
	}
}
