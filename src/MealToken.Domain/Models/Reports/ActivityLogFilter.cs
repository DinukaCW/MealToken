using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class ActivityLogFilter
	{
		public DateTime? StartDateTime { get; set; }
		public DateTime? EndDateTime { get; set; }
		public List<string>? EntityTypes { get; set; }
		public List<string>? ActionTypes { get; set; }
		public List<int>? UserIds{ get; set; }
	}
}
