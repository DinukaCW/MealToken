using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class ScheduleMeal
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int SheduleMealSubTypeId { get; set; }
		public int TenantId { get; set; }
		public int SheduleId { get; set; }
		public int MealTypeId { get; set; }
		public int? MealSubTypeId { get; set; }
		public int SupplierId { get; set; }
		public bool IsFunctionKeysEnable { get; set; }
		public string? FunctionKey { get; set; }
		public TimeOnly? TokenIssueStartTime { get; set; }
		public TimeOnly? TokenIssueEndTime { get; set; }
		public bool IsAvailable { get; set; }
	}
}
