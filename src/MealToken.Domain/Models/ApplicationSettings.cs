using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
	public class ApplicationSettings
	{
		public string? Currency { get; set; }
		public bool? EnableNotifications { get; set; }
		public bool? EnableFunctionKeys { get; set; }
	}
}
