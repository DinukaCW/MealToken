using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
    public class MealDeviceRequest
    {
        public string PersonNumber { get; set; }
        public DateTime DateTime { get; set; }
        public string DeviceSerialNo { get; set; }
        public string? FunctionKey { get; set; }
     
    }
}
