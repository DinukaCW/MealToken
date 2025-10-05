using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
   public class RequestReturn
    {
        public int RequestId { get; set; }
        public DateOnly EventDate { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public int NoOfAttendess { get; set; }
        public List<RequestMealDto>? RequestMeals { get; set; }
    }
}
