using MealToken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class AddOnDto
	{
		public List<Snacks> Snacks { get; set; }
		public List<Beverages> Beverages { get; set; }
	}

	public class Snacks
	{
		public int SnackId { get; set; }
		public string SnackName { get; set; }
		public AddOnType AddOnType { get; set; }
	}
	public class Beverages
	{
		public int BeverageId { get; set; }
		public string BeverageName { get; set; }
		public AddOnType AddOnType { get; set; }
	}
}
