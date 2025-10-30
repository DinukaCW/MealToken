using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class MealRequestsDto
	{
	
		public int TotalRequests { get; set; }
		public int ApprovedRequests { get; set; }
		public int PendingRequests { get; set; }
		
	}
}
