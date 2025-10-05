using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class ScheduleDetails
	{
		public int ScheduleId { get; set; }
		public string ScheduleName { get; set; }
		public string SchedulePeriod { get; set; } // "Single Date", "Date Range", "Year", "Month", "Custom Days", etc.
		public List<DateOnly> ScheduleDates { get; set; } // All dates for this schedule
		public List<MealTypeD> MealTypes { get; set; }
		public List<SubMealTypeD> SubTypes { get; set; }
		public List<PeopleD> AssignedPersons { get; set; }
	}
	public class MealTypeD
	{
		public int MealTypeId { get; set; }
		public string MealTypeName { get; set; }
		public SupplierD? Supplier { get; set; }

	}
	public class SubMealTypeD
	{
		public int MealSubTypeId { get; set; }
		public string MealSubTypeName { get; set; }
		public SupplierD Supplier { get; set; }
	}
	public class  PeopleD
	{
		public int PersonId { get; set; }
		public string FullName { get; set; }

	}
	public class SupplierD
	{
		public int SupplierId { get; set; }
		public string SupplierName { get; set; }
	}

}
