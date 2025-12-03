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
        public string MealType { get; set; }
        public string Shift { get; set; }
        public string EmpNo { get; set; }
        public string EmpName { get; set; }
        public string Gender { get; set; }
        public string Department { get; set; }
        public string TokenType { get; set; }
        public decimal Contribution { get; set; }
        public string? DeviceSerialNo { get; set; } = null;
		public int MealConsumptionId { get; set; }
    }
}
