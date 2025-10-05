using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class SchedulecreationDetails
	{
		public List<PeopleReturn> Persons { get; set; }
		public List<MealTypeReturn> MealTypes { get; set; }
		public List<SupplierD> Suppliers { get; set; }
	}
	public class PeopleReturn
	{
		public int PersonId { get; set; }
		public PersonType PersonType { get; set; }
		public string Name { get; set; }
	}
	public class MealTypeReturn
	{
		public int MealTypeId { get; set; }
		public string MealTypeName { get; set; }
		public List<SubMealTypeReturn> SubTypes { get; set; }
	}
	public class SubMealTypeReturn
	{
		public int MealSubTypeId { get; set; }
		public string MealSubTypeName { get; set; }
	}
}
