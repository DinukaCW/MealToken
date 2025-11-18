using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class ManualTokenDto
	{
		public int PersonId { get; set; }
		public string PersonName { get; set; }
		public DateOnly PrintedDate { get; set; }
		public TimeOnly PrintedTime { get; set; }
		public string MealType { get; set; }
		public string? SubMealType { get; set; }
		public string Reason { get; set; }
	}
}
