using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
    public class RequestMeal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestMealId { get; set; }
        public int TenantId { get; set; }
        public int RequestId { get; set; }
        public int MealTypeId { get; set; }
        public int? SubTypeId { get; set; }
        public int MealCostId { get; set; }
        public decimal Quantity { get; set; }
    }
}
