using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class RejectRequestModel
	{
		[Required]
		public int RequestId { get; set; }

		[StringLength(500)]
		public string? RejectionReason { get; set; }
	}
}
