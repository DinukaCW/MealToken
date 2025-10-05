using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class Supplier
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int SupplierId { get; set; }
		public int TenantId { get; set; }
		public string SupplierName { get; set; }
		public string ContactNumber { get; set; }
		public string Email { get; set; }
		public string Address { get; set; }
		public int SupplierRating { get; set; } // 1-5 rating
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
