using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
    public class ScheduleMealDetails
    {
        public int MealTypeId { get; set; }
        public int? MealSubTypeId { get; set; }
        public TimeOnly? StartTime {  get; set; }
        public TimeOnly? EndTime { get; set; }
    }
}
