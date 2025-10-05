using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class SheduleDTO
	{
		public string ScheduleName { get; set; }
		public string SchedulePeriod { get; set; } // "Single Date", "Date Range", "Year", "Month", "Custom Days", etc.
		public List<DateOnly> ScheduleDates { get; set; } // All dates for this schedule
		public string Note { get; set; }
		public List<SheduleMealDto> MealTypes { get; set; }
		public List<int> AssignedPersonIds { get; set; }
	}
}
