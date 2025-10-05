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
        public DateOnly Date {  get; set; }
        public TimeOnly Time { get; set; }
        public int MealTypeId { get; set; }
        public int? SubTypeId { get; set; }
        public int MealCostId { get; set; }
        public int DeviceId { get; set; }
        public Shift ShiftName { get; set; }
        public PayStatus PayStatus { get; set; } // free or paid
        public bool TockenIssued { get; set; }
    }
   
}
