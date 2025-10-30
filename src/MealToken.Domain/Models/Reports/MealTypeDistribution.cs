using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	public class MealTypeDistribution
	{
		 public List<MealTypeDistributionDto> Distribution { get; set; }
	}

	public class MealTypeDistributionDto
	{
		public string MealType { get; set; }
		public int Count { get; set; }
		public decimal Percentage { get; set; }
	}
}
