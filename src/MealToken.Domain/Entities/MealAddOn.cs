using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class MealAddOn
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int MealTypeAddOnId { get; set; }
		public int TenantId { get; set; }
		public int MealTypeId { get; set; }
		public string AddOnName { get; set; }
		public AddOnType AddOnType { get; set; }
		public string? Description { get; set; }
	}

	public enum AddOnType
	{
		Snacks,
		Beverages
	}
}
