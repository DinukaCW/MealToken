using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class ScheduleDate
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int SheduleDateId { get; set; }
		public int TenantId { get; set; }

		public int SheduleId { get; set; }

		public DateOnly Date { get; set; }
		

	}
}
