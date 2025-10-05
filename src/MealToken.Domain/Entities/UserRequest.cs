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
	public class UserRequest
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserRequestId { get; set; }

		public int TenantId { get; set; }

		[Required]
		public string Username { get; set; }

		[Required]
		[StringLength(255)]
		public string FullName { get; set; }

		public string PasswordHash { get; set; }

		public int UserRoleId { get; set; }

		[Required]
		[StringLength(255)]
		public string Email { get; set; }

		[StringLength(50)]
		public string? PhoneNumber { get; set; }

		public UserRequestStatus Status { get; set; } = UserRequestStatus.Pending;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? ReviewedAt { get; set; }

		public int? ReviewedBy { get; set; } // Admin/Authorizer UserID

		public string? ReviewComments { get; set; }

		public string? RejectionReason { get; set; }
	}

}
