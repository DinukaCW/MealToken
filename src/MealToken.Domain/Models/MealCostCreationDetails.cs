using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class MealCostCreationDetails
	{
		public List<SupplierD> Suppliers { get; set; }
		public List<MealTypeReturn> MealTypes { get; set; }

	}

	
}
