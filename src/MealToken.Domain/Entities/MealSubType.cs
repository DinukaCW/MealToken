using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class MealSubType
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int MealSubTypeId { get; set; }
		public int TenantId { get; set; }
		public int MealTypeId { get; set; }
		public string SubTypeName { get; set; }
		public string? Description { get; set; }
		public string Functionkey { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
