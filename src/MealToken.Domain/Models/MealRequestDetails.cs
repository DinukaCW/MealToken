using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
    public class MealRequestDetails
    {
        public int MealTypeId { get; set; }
        public string MealTypeName { get; set; }
        public List<MealCostDetails> MealSubTypeDetails { get; set; }
    }
}
