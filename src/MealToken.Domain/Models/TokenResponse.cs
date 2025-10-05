using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
   public class TokenResponse
    {
        public DateOnly Date {  get; set; }
        public TimeOnly Time { get; set; }
        public string Shift { get; set; }
        public string EmpNo { get; set; }
        public string Department { get; set; }
        public string TokenType { get; set; }
        public int MealConsumptionId { get; set; }
    }
}
