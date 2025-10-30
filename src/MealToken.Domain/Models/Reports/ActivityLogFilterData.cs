using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class ActivityLogFilterData
	{
		public List<UserDto> Users { get; set; } = new List<UserDto>();
		public List<string> EntityTypes { get; set; } = new List<string>();
		public List<string> ActionTypes { get; set; } = new List<string>();
	}

	public class UserDto
	{
		public int UserID { get; set; }
		public string FullName { get; set; }
	}
}
