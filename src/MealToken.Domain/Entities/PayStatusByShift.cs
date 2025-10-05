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
   public class PayStatusByShift
   {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PolicyId { get; set; }
        public Shift ShiftType { get; set; }
        public int MealTypeId { get; set; }  // Breakfast, Lunch, Dinner, Tea/Milk, Snacks
        public bool IsMalePaid { get; set; }
        public bool IsFemalePaid { get; set; }
    }
}
