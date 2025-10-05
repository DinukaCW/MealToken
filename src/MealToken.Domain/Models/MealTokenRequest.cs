using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
    public class MealTokenRequest
    {
        public int PersonId { get; set; }
        public DateOnly RequestDate { get; set; }
        public TimeOnly RequestTime { get; set; }
        public string? FunctionKey { get; set; }
    }
}
