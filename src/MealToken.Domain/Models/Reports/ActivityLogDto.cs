using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class ActivityLogDto
	{
		public DateTime Timestamp { get; set; }
		public string UserName { get; set; }
		public string UserRole { get; set; }
		public string Action { get; set; }
		public string Entity { get; set; }
		public string Details { get; set; }
		public string IpAddress { get; set; }
	}
}
