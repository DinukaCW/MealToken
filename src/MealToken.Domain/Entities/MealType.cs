using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class MealType
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int MealTypeId { get; set; }
		public int TenantId { get; set; }
		public string TypeName { get; set; } // e.g., Breakfast, Lunch, Dinner
		public string? Description { get; set; }
		public string? TokenIssueStartDate { get; set; } // e.g., "08:00"
		public string? TokenIssueEndDate { get; set; } // e.g., "10:00"
		public TimeOnly? TokenIssueStartTime { get; set; } 
		public TimeOnly? TokenIssueEndTime { get; set; } 
		public string? MealTimeStartDate { get; set; } // e.g., "12:00"
		public string? MealTimeEndDate { get; set; } // e.g., "14:00"
		public TimeOnly? MealTimeStartTime { get; set; }
		public TimeOnly? MealTimeEndTime { get; set; }
		public bool IsFunctionKeysEnable { get; set; }
		public bool IsAddOnsEnable { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

}
