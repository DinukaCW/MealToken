using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class TodayScheduleDto
	{
		public string MealType { get; set; }
		public TimeOnly StartTime { get; set; }
		public TimeOnly EndTime { get; set; }
		public MealStatus? Status { get; set; }
		public int ScheduleId { get; set; }
		public string ScheduleName { get; set; }
	}
}
