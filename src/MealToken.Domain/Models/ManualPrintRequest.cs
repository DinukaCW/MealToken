using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class ManualPrintRequest
	{
		public int PersonId { get; set; }
		public DateTime RequestDate { get; set; }
		public int MealTypeId { get; set; }
		public int? MealSubTypeId { get; set; }
		public int SupplierId { get; set; }
		public int DeviceId { get; set; }
		public string? Reason { get; set; }
		public Shift Shift { get; set; }
	}
}
