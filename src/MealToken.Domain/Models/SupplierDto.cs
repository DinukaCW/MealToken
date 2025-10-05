using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class SupplierDto
	{
		public int SupplierId { get; set; }
		public string SupplierName { get; set; }
		public string ContactNumber { get; set; }
		public string Email { get; set; }
		public string Address { get; set; }
		public int SupplierRating { get; set; }
		public bool IsActive { get; set; }
	}
}
