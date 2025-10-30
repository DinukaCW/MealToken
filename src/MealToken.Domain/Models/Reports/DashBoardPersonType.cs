using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class DashBoardPersonType
	{
		public int EmployeeCount { get; set; }
		public int EmployeeMeals { get; set; }
		public decimal EmployeeMealCost { get; set; }
		public int VisitorCount { get; set; }
		public int VisitorMeals {  get; set; }
		public decimal VisitorMealCost { get; set; }
	}
}
