using Microsoft.AspNetCore.Http;
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
		public IFormFile? Image { get; set; }
		public string? CompanyEmail { get; set; }
	}
	public class ApplicationSettingsReturn
	{
		public string? Currency { get; set; }
		public string? Image { get; set; }
		public string? CompanyEmail { get; set; }
	}
}
