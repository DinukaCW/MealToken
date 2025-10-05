using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class SheduleMealDto
	{
		public int MealTypeId { get; set; }
		public int? SupplierId { get; set; }
		public List<SubMealTypeDto>? SubMealTypes { get; set; }
	}

	public class SubMealTypeDto
	{
		public int MealSubTypeId { get; set; }
		public int SupplierId { get; set; }
	}
}
