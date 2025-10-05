using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class SupplierCreateRequestDto
	{
		[Required(ErrorMessage = "Supplier name is required.")]
		[StringLength(200, ErrorMessage = "Supplier name cannot exceed 200 characters.")]
		public string SupplierName { get; set; }

		[Required(ErrorMessage = "Contact number is required.")]
		[StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters.")]
		[Phone(ErrorMessage = "Invalid contact number format.")]
		public string ContactNumber { get; set; }

		[Required(ErrorMessage = "Email is required.")]
		[StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Address is required.")]
		[StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
		public string Address { get; set; }

		[Range(0, 5, ErrorMessage = "Supplier rating must be between 0 and 5.")]
		public int SupplierRating { get; set; } = 0; // Default to 0 (no rating)

		public bool IsActive { get; set; } = true;
	}
}
