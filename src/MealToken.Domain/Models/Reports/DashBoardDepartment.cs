using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class DashBoardDepartment
	{
		public decimal TotalMealCosts { get; set; }
		public int TotalMealCount { get; set; }
		public List<DepartmentWiseMeal> DepartmentWiseMeals { get; set; }
	}
	public class DepartmentWiseMeal
	{
		public int DepartmentID { get; set; }
		public string DepartmentName { get; set; }
		public int MealCount { get; set; }
		public decimal Precentage { get; set; }
		public decimal MealCosts { get; set; }
		public int EmployeeMealCount { get; set; }
		public int VisitorMealCount { get; set; }
		public decimal EmployeeMealCosts { get; set; }
		public decimal VisitorMealCosts { get; set; }
	}
	public class DepartmentPersonGroupDto
	{
		public int DepartmentId { get; set; }
		public string DepartmentName { get; set; } = string.Empty;
		public List<int> Persons { get; set; } = new();
		public List<int> Employees { get; set; } = new();
		public List<int> Visitors { get; set; } = new();
	}
}
