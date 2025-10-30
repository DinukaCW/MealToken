using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class DashBoardMealType
	{
		public int TotalMeals { get; set; }
		public List<MealTypeWiseMeal> MealTypesWiseMeals { get; set; }
	}
	public class MealTypeWiseMeal
	{
		public int MealTypeId { get; set; }
		public string MealType { get; set; }
		public int MealsCount { get; set; }
		public List<SubTypeWiseMeal> subTypeWiseMeals { get; set; }
	}
	public class SubTypeWiseMeal
	{
		public int SubTypeId { get; set; }
		public string SubType { get; set; }
		public int MealsCount { get; set; }
	}
	public class MealTypeRawData
	{
		public int MealTypeId { get; set; }
		public string MealTypeName { get; set; }
		public int? SubTypeId { get; set; }
		public string SubTypeName { get; set; }
	}
}
