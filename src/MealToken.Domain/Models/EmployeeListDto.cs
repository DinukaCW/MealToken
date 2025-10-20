using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class EmployeeListDto
	{
		public int EmployeeId { get; set; }
		public string EmployeeNumber { get; set; }
		public string FullName { get; set; }
		public string NICNumber { get; set; }
		public DateTime JoinedDate { get; set; }
		public int DepartmentId { get; set; }
		public string DepartmentName { get; set; }
		public int DesignationId { get; set; }
		public string DesignationName { get; set; }
		public string? EmployeeGrade { get; set; }
		public string? EmployeeType { get; set; }
		public string Gender { get; set; }
		public string? WhatsappNumber { get; set; }
		public string? EMail { get; set; }
        public string? MealGroup { get; set; }
		public bool MealEligibility { get; set; }
		public bool ActiveEmployee { get; set; }
	}

	public class VisitorListDto
	{
		public int VisitorId { get; set; }
		public string CardNumber { get; set; }
		public string CardName { get; set; }
		public string VisitorType { get; set; }
		public int DepartmentId { get; set; }
		public string DepartmentName { get; set; }
		public string? SpecialNote { get; set; }
		public bool MealEligibility { get; set; }
		public bool ActiveVisitor { get; set; }
	}
}
