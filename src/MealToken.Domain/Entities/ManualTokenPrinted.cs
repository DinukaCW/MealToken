using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
	public class ManualTokenPrinted
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ManualTokenPrintedId { get; set; }
		public int TenantId { get; set; }
		public int PersonId { get; set; }
		public DateTime PrintedDate { get; set; }
		public int MealConsumptionId { get; set; }
		public string Reason { get; set; }
		public bool TokenIssued { get; set; }

	}

}
