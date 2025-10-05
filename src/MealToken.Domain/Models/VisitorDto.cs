using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class VisitorDto
	{
		public string CardNumber { get; set; }
		public string VisitorType { get; set; }
		public string CardName { get; set; }
		public int DepartmentId { get; set; }
		public bool MealEligibility { get; set; }
		public bool IsActive { get; set; }
		public string SpecialNote { get; set; }
	}
}
