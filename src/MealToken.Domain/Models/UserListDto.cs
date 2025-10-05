using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class UserListDto
	{
		public int UserID { get; set; }
		public string Username { get; set; }
		public string FullName { get; set; }
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
		public bool IsActive { get; set; }
		public int UserRoleId { get; set; }
	}
}
