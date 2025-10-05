using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class MealCostDto
	{
		[Required(ErrorMessage = "Supplier is required.")]
		[Range(1, int.MaxValue, ErrorMessage = "Please select a valid supplier.")]
		public int SupplierId { get; set; }

		[Required(ErrorMessage = "Meal type is required.")]
		[Range(1, int.MaxValue, ErrorMessage = "Please select a valid meal type.")]
		public int MealTypeId { get; set; }

		// Optional - can be null when meal type has no sub types
		public int? MealSubTypeId { get; set; }

		[Required(ErrorMessage = "Supplier cost is required.")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Supplier cost must be greater than 0.")]
		public decimal SupplierCost { get; set; }

		[Required(ErrorMessage = "Selling price is required.")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Selling price must be greater than 0.")]
		public decimal SellingPrice { get; set; }

		[Required(ErrorMessage = "Company cost is required.")]
		[Range(0, double.MaxValue, ErrorMessage = "Company cost cannot be negative.")]
		public decimal CompanyCost { get; set; }

		[Required(ErrorMessage = "Employee cost is required.")]
		[Range(0, double.MaxValue, ErrorMessage = "Employee cost cannot be negative.")]
		public decimal EmployeeCost { get; set; }

		[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
		public string? Description { get; set; }

		// Custom validation to ensure cost breakdown adds up to selling price
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var totalCost = CompanyCost + EmployeeCost;
			var expectedTotal = SellingPrice;

			// Allow small rounding differences (within 0.01)
			if (Math.Abs(totalCost - expectedTotal) > 0.01m)
			{
				yield return new ValidationResult(
					$"Company cost ({CompanyCost:F2}) + Employee cost ({EmployeeCost:F2}) = {totalCost:F2} must equal Selling price ({SellingPrice:F2})",
					new[] { nameof(CompanyCost), nameof(EmployeeCost), nameof(SellingPrice) });
			}

			// Ensure selling price covers supplier cost with reasonable margin
			if (SellingPrice < SupplierCost)
			{
				yield return new ValidationResult(
					"Selling price cannot be less than supplier cost.",
					new[] { nameof(SellingPrice), nameof(SupplierCost) });
			}
		}
	}
}
