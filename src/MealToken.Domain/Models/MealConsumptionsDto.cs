using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
   public class MealConsumptionsDto
    {
        public DateOnly Date { get; set; }
        public string MealTypeName { get; set; } = string.Empty;
        public string SubTypeName { get; set; } = string.Empty;
        public int PersonId { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public decimal EmployeeCost { get; set; }
        public decimal CompanyCost { get; set; }
        public decimal SupplierCost { get; set; }

        // Joined data from the Employee table
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
    }
}
