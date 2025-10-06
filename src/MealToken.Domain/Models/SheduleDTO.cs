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
        public List<DateOnly>? ScheduleDates { get; set; } // Make this nullable
        public string? Note { get; set; } // Make this nullable for consistency if not provided
        public List<SheduleMealDto>? MealTypes { get; set; } // Make this nullable
        public List<int>? AssignedPersonIds { get; set; } // Make this nullable
    }
}
