using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class SchedulePerson
	{
		public int ShedulePersonId { get; set; }
		public int TenantId { get; set; }
		public int PersonId { get; set; }
		public int SheduleId { get; set; }

	}
}
