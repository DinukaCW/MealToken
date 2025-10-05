using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class PersonCreateDto
	{
		public PersonType PersonType { get; set; }

		[Required(ErrorMessage = "Person number is required.")]
		[StringLength(50, ErrorMessage = "Person number cannot exceed 50 characters.")]
		public string PersonNumber { get; set; }

		[StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
		public string Name { get; set; }

		[StringLength(20, ErrorMessage = "NIC number cannot exceed 20 characters.")]
		public string? NICNumber { get; set; }

		[DataType(DataType.Date)]
		public DateTime? JoinedDate { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Please select a valid Department.")]
		public int DepartmentId { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Please select a valid Designation.")]
		public int? DesignationId { get; set; }

		[StringLength(50, ErrorMessage = "Employee grade cannot exceed 50 characters.")]
		public string? EmployeeGrade { get; set; }

		[Required(ErrorMessage = "Person sub-type is required.")]
		[StringLength(50, ErrorMessage = "Person sub-type cannot exceed 50 characters.")]
		public string PersonSubType { get; set; } // EmployeeType or VisitorType

		[StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters.")]
		[RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other.")]
		public string? Gender { get; set; }

		[StringLength(50, ErrorMessage = "Meal group cannot exceed 50 characters.")]
		public string? MealGroup { get; set; }

		public bool MealEligibility { get; set; }

		public bool IsActive { get; set; } = true;

		[StringLength(500, ErrorMessage = "Special note cannot exceed 500 characters.")]
		public string? SpecialNote { get; set; }

		//Custom conditional validations
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (PersonType == PersonType.Employer)
			{
				if (DepartmentId <= 0)
				{
					yield return new ValidationResult("Department is required for employees.", new[] { nameof(DepartmentId) });
				}

				if (!DesignationId.HasValue || DesignationId <= 0)
				{
					yield return new ValidationResult("Designation is required for employees.", new[] { nameof(DesignationId) });
				}

				if (!JoinedDate.HasValue)
				{
					yield return new ValidationResult("Joined date is required for employees.", new[] { nameof(JoinedDate) });
				}
			}
			else if (PersonType == PersonType.Visitor)
			{
				// Visitors should not be forced to have designation, grade, etc.
				if (DesignationId.HasValue)
				{
					yield return new ValidationResult("Designation should not be set for visitors.", new[] { nameof(DesignationId) });
				}
			}
		}
	}
}
