using MealToken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class EmployeeCreationDetails
	{
		public List<DepartmentDto> Departments { get; set; } = new();
		public List<DesignationDto> Designations { get; set; } = new();
	}
	public class DepartmentDto
	{
		public int DepartmentId { get; set; }
		public string DepartmentName { get; set; }
	}
	public class DesignationDto
	{
		public int DesignationId { get; set; }
		public string DesignationName { get; set; }
	}

}
