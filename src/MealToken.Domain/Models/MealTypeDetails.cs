using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MealToken.Domain.Models.MealTypeUpdateDto;

namespace MealToken.Domain.Models
{
	public class MealTypeDetails
	{
		public int MealTypeId { get; set; }
		public string TypeName { get; set; } // e.g., Breakfast, Lunch, Dinner

		public string Description { get; set; }

		// Accept only "yesterday", "today", or "tomorrow"
		public string? TokenIssueStartDate { get; set; }
		public string? TokenIssueEndDate { get; set; }

		public TimeOnly? TokenIssueStartTime { get; set; }
		public TimeOnly? TokenIssueEndTime { get; set; }

		public string? MealTimeStartDate { get; set; } // yesterday, today, tomorrow
		public string? MealTimeEndDate { get; set; }

		public TimeOnly? MealTimeStartTime { get; set; }
		public TimeOnly? MealTimeEndTime { get; set; }

		public bool IsFunctionKeysEnable { get; set; } = false;

		public bool IsAddOnsEnable { get; set; }
		public List<MealSubTypeDto> SubTypes { get; set; }
		public List<MealAddOns> AddOns { get; set; }
	}
}
