using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class SheduleCreateDto
	{
		[Required(ErrorMessage = "Schedule name is required.")]
		[StringLength(200, ErrorMessage = "Schedule name cannot exceed 200 characters.")]
		public string ScheduleName { get; set; }

		[Required(ErrorMessage = "Schedule period is required.")]
		[StringLength(50, ErrorMessage = "Schedule period cannot exceed 50 characters.")]
		public string SchedulePeriod { get; set; }

		[Required(ErrorMessage = "Date parameters are required.")]
		public Dictionary<string, object> DateParameters { get; set; }

		[StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
		public string? Note { get; set; }

		[Required(ErrorMessage = "At least one meal type is required.")]
		[MinLength(1, ErrorMessage = "At least one meal type must be provided.")]
		public List<SheduleMealDto> MealTypes { get; set; }

		[Required(ErrorMessage = "At least one person must be assigned.")]
		[MinLength(1, ErrorMessage = "At least one assigned person must be provided.")]
		public List<int> AssignedPersonIds { get; set; }

	}
}
