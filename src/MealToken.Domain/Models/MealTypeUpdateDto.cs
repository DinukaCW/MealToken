using MealToken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class MealTypeUpdateDto
	{
        // The service layer handles name conflict/change. Remove [Required] for partial updates.
        [StringLength(100, ErrorMessage = "Type name cannot exceed 100 characters.")]
        public string? TypeName { get; set; } // CHANGED to nullable string

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; } // CHANGED to nullable string

        // Time/Date fields remain nullable for optional updates
        public string? TokenIssueStartDate { get; set; }
        public string? TokenIssueEndDate { get; set; }
        public TimeOnly? TokenIssueStartTime { get; set; }
        public TimeOnly? TokenIssueEndTime { get; set; }
        public string? MealTimeStartDate { get; set; }
        public string? MealTimeEndDate { get; set; }
        public TimeOnly? MealTimeStartTime { get; set; }
        public TimeOnly? MealTimeEndTime { get; set; }

        public bool IsFunctionKeysEnable { get; set; } = false;
        public bool IsAddOnsEnable { get; set; }

        // CRITICAL CHANGE: Must be nullable to support partial updates (send null to skip list change)
        public List<MealSubTypeDto>? SubTypes { get; set; } 

        public List<MealAddOns>? AddOns { get; set; } 

        // Custom validation for "esterday", "today", "tomorrow"
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			// Time validation
			if (TokenIssueStartTime.HasValue && TokenIssueEndTime.HasValue &&
				TokenIssueStartTime.Value >= TokenIssueEndTime.Value)
			{
				yield return new ValidationResult(
					"Token issue start time must be earlier than end time.",
					new[] { nameof(TokenIssueStartTime), nameof(TokenIssueEndTime) });
			}
			if (MealTimeStartTime.HasValue && MealTimeEndTime.HasValue &&
				MealTimeStartTime.Value >= MealTimeEndTime.Value)
			{
				yield return new ValidationResult(
					"Meal time start must be earlier than end time.",
					new[] { nameof(MealTimeStartTime), nameof(MealTimeEndTime) });
			}

			// SubTypes duplicate validation
			if (IsFunctionKeysEnable && SubTypes != null && SubTypes.Any())
			{
				var duplicateKeys = SubTypes
					.Where(st => !string.IsNullOrWhiteSpace(st.Functionkey))
					.GroupBy(st => st.Functionkey.ToUpper())
					.Where(g => g.Count() > 1)
					.Select(g => g.Key);
				if (duplicateKeys.Any())
				{
					yield return new ValidationResult(
						$"Duplicate SubType function keys found: {string.Join(", ", duplicateKeys)}",
						new[] { nameof(SubTypes) });
				}

				var duplicateNames = SubTypes
					.Where(st => !string.IsNullOrWhiteSpace(st.SubTypeName))
					.GroupBy(st => st.SubTypeName.ToUpper())
					.Where(g => g.Count() > 1)
					.Select(g => g.Key);
				if (duplicateNames.Any())
				{
					yield return new ValidationResult(
						$"Duplicate SubType names found: {string.Join(", ", duplicateNames)}",
						new[] { nameof(SubTypes) });
				}
			}

			// AddOns duplicate validation
			if (IsAddOnsEnable && AddOns != null && AddOns.Any())
			{ 

				var duplicateNames = AddOns
					.Where(a => !string.IsNullOrWhiteSpace(a.AddOnName))
					.GroupBy(a => a.AddOnName.ToUpper())
					.Where(g => g.Count() > 1)
					.Select(g => g.Key);
				if (duplicateNames.Any())
				{
					yield return new ValidationResult(
						$"Duplicate AddOn names found: {string.Join(", ", duplicateNames)}",
						new[] { nameof(AddOns) });
				}
			}
		}
		public class MealSubTypeDto
		{
			public string SubTypeName { get; set; }
			public string? Description { get; set; }
			public string Functionkey { get; set; }
		}
		public class MealAddOns
		{
			public int AddOnId { get; set; }
			public string AddOnName { get; set; }
			public AddOnType AddonType { get; set; }
		}
	}
}
