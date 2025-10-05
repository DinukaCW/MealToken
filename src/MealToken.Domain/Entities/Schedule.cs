using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class Schedule
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int SheduleId { get; set; }
		public int TenantId { get; set; }
		public string SheduleName { get; set; }
		public string ShedulePeriod { get; set; }
		public bool IsActive { get; set; }
		public string? Note { get; set; }

		public ICollection<ScheduleMeal> SheduleMeals { get; set; }
		public ICollection<SchedulePerson> MealShedulePeople { get; set; }
		public ICollection<ScheduleDate> SheduleDates { get; set; }
	}
}
