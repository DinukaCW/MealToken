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
    public class MealConsumption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealConsumptionId { get; set; }
        public int TenantId { get; set; }
        public int PersonId { get; set; }
        public string? PersonName { get; set; }
        public string? Gender { get; set; }
        public DateOnly Date {  get; set; }
        public TimeOnly Time { get; set; }
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; }
        public bool AddOnMeal {  get; set; } = false;
        public int MealTypeId { get; set; }
        public string MealTypeName { get; set; }
        public int? SubTypeId { get; set; }
        public string? SubTypeName { get; set; }
        public int MealCostId { get; set; }
        public decimal SupplierCost { get; set; } 
        public decimal SellingPrice { get; set; } 
        public decimal CompanyCost { get; set; } 
        public decimal EmployeeCost { get; set; } 
        public int DeviceId { get; set; }
        public string DeviceSerialNo { get; set; }
        public Shift ShiftName { get; set; }
        public PayStatus PayStatus { get; set; } // free or paid
        public bool TockenIssued { get; set; }
        public string? JobStatus { get; set; }

    }
   
}
