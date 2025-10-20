using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class PersonListDto
	{
		public int PersonId { get; set; }
		public PersonType PersonType { get; set; }
		public string PersonNumber { get; set; }
		public string Name { get; set; }
		public string? NICNumber { get; set; }
		public DateTime? JoinedDate { get; set; }
		public int DepartmentId { get; set; }
		public string DepartmentName { get; set; }
		public int? DesignationId { get; set; }
		public string? DesignationName { get; set; }
		public string? EmployeeGrade { get; set; }
		public string PersonSubType { get; set; } // EmployeeType or VisitorType
		public string? Gender { get; set; }
		public string? WhatsappNumber { get; set; }
		public string? EMail { get; set; }
        public string? MealGroup { get; set; }
		public bool MealEligibility { get; set; }
		public bool IsActive { get; set; }
		public string? SpecialNote { get; set; }
	}
}
