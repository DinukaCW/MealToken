using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class TokenTimeConflict
	{
		public int PersonId { get; set; }
		public string PersonName { get; set; }
		public DateOnly ConflictDate { get; set; }
		public string NewMealType { get; set; }
		public string NewTokenTime { get; set; }
		public int ExistingScheduleId { get; set; }
		public string ExistingScheduleName { get; set; }
		public string ExistingMealType { get; set; }
		public string ExistingTokenTime { get; set; }
	}
}
