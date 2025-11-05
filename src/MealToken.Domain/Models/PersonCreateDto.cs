using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
    public class PersonCreateDto : IValidatableObject
	{
        public PersonType PersonType { get; set; }

        [Required(ErrorMessage = "Person number/Card number is required.")]
        [StringLength(50, ErrorMessage = "Person number/Card number cannot exceed 50 characters.")]
        public string PersonNumber { get; set; } // Used for Employee Number or Visitor Card Number

        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string? Name { get; set; }

        [StringLength(20, ErrorMessage = "NIC number cannot exceed 20 characters.")]
        public string? NICNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? JoinedDate { get; set; }

		[Required(ErrorMessage = "Department ID is required for employees.")]
		public int DepartmentId { get; set; }

		public int? DesignationId { get; set; }

        [StringLength(50, ErrorMessage = "Employee grade cannot exceed 50 characters.")]
        public string? EmployeeGrade { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Person sub-type cannot exceed 50 characters.")]
        public string PersonSubType { get; set; } // EmployeeType or VisitorType]

       // [Required(ErrorMessage = "Gender Should Provide.")]
        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters.")]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other.")]

        public string? Gender { get; set; }

        [StringLength(20, ErrorMessage = "WhatsApp number cannot exceed 20 characters.")]
        [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Invalid WhatsApp number format.")]
        public string? WhatsappNumber { get; set; }

        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? EMail { get; set; }

        [StringLength(50, ErrorMessage = "Meal group cannot exceed 50 characters.")]
        public string? MealGroup { get; set; }

        public bool MealEligibility { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "Special note cannot exceed 500 characters.")]
        public string? SpecialNote { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (PersonType == PersonType.Employer && string.IsNullOrWhiteSpace(Gender))
			{
				yield return new ValidationResult("Gender is required for employees.", new[] { nameof(Gender) });
			}

			// Example: you can also add other conditional checks here
		}
	}
}
