using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class Person
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int PersonId { get; set; }

		[Required]
		public int TenantId { get; set; }

		[Required]
		public PersonType PersonType { get; set; }

		[Required]
		[MaxLength(50)]
		public string PersonNumber { get; set; } // EmployeeNumber or CardNumber

		[MaxLength(200)]
		public string? Name { get; set; } // FullName or CardName

		[MaxLength(50)] // Make this nullable for visitors who might not have NIC
		public string? NICNumber { get; set; }

		// Employee-specific fields (nullable for visitors)
		public DateTime? JoinedDate { get; set; }
		public int? DesignationId { get; set; } // Nullable for visitors
		[Required]
		public int DepartmentId { get; set; }
		public string? EmployeeGrade { get; set; }

		[MaxLength(50)]
		public string PersonSubType { get; set; } // EmployeeType or VisitorType

		[MaxLength(10)]
		public string Gender { get; set; }
        [MaxLength(50)]
        public string? WhatsappNumber { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        [MaxLength(50)]
		public string? MealGroup { get; set; }

		public bool MealEligibility { get; set; } = false;

		public bool IsActive { get; set; } = true;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[MaxLength(500)]
		public string? SpecialNote { get; set; }

	}
}
