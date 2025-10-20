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

        // Removed [Range] attribute to align with the new requirement that it's NOT required for create employee
        public int? DepartmentId { get; set; }

        public int? DesignationId { get; set; }

        [StringLength(50, ErrorMessage = "Employee grade cannot exceed 50 characters.")]
        public string? EmployeeGrade { get; set; }

        [StringLength(50, ErrorMessage = "Person sub-type cannot exceed 50 characters.")]
        public string PersonSubType { get; set; } // EmployeeType or VisitorType]

        [Required(ErrorMessage = "Gender Type Should be Provide")]
        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters.")]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other.")]

        public string Gender { get; set; }

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

        // Using IValidatableObject for custom conditional validations
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // For 'create', the built-in [Required] on PersonNumber is sufficient for both types.
            // We only need to check for invalid/extra data based on the type.

            if (PersonType == PersonType.Employer)
            {
                // NEW REQUIREMENT: Employee Number (PersonNumber) is the only required thing for create employee.
                // This means we remove the checks for DepartmentId, DesignationId, and JoinedDate.

                // Only perform checks for properties that *shouldn't* be set for an employee if it makes sense,
                // but based on your requirement, the goal seems to be minimal validation on create.
            }
            else if (PersonType == PersonType.Visitor)
            {
                // NEW REQUIREMENT: Card Number (PersonNumber) is only required to create visitor.
                // Visitors should not have employee-specific data.

                if (DesignationId.HasValue && DesignationId > 0)
                {
                    yield return new ValidationResult("Designation should not be set for visitors.", new[] { nameof(DesignationId) });
                }

                if (!string.IsNullOrEmpty(EmployeeGrade))
                {
                    yield return new ValidationResult("Employee Grade should not be set for visitors.", new[] { nameof(EmployeeGrade) });
                }

                if (JoinedDate.HasValue)
                {
                    yield return new ValidationResult("Joined Date should not be set for visitors.", new[] { nameof(JoinedDate) });
                }
            }
        }
    }
}
