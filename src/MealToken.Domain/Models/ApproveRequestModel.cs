using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class ApproveRequestModel
	{
		[Required]
		public int RequestId { get; set; }

		public string? Comments { get; set; }
	}
}
