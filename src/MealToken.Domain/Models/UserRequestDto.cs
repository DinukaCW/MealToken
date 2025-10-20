using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class UserRequestDto
	{
		public int RequestId { get; set; }
		public string FullName { get; set; }
		public int UserRoleId { get; set; }
		public string UserRole { get; set; }
		public List<DepartmentD> Departments { get; set; }

	}
}
