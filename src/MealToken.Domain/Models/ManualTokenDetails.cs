using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class ManualTokenDetails
	{
		public List<PeopleReturn> Persons { get; set; }
		public List<MealTypeReturn> MealTypes { get; set; }
		public List<SupplierD> Suppliers { get; set; }
		public List<ShiftReturn> Shifts { get; set; }
	}

	public class ShiftReturn
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
