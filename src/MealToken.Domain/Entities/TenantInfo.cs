using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class TenantInfo
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Subdomain { get; set; } = string.Empty;
		public string SchemaName { get; set; } = string.Empty;
		public string ConnectionString { get; set; } = string.Empty;
		public bool IsActive { get; set; } = true;
		public string? Currency { get; set; } = "LKR";
		public bool? EnableNotifications { get; set; }= true;
		public bool? EnableFunctionKeys { get; set; }= true;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
