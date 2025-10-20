using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
    public class MealConsumptionWithDetails
    {
        public int MealConsumptionId { get; set; }
        public DateOnly Date { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string Gender { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public string MealTypeName { get; set; }
        public string SubTypeName { get; set; }
        public decimal EmployeeCost { get; set; }
        public decimal CompanyCost { get; set; }
        public decimal SupplierCost { get; set; }
    }
}
