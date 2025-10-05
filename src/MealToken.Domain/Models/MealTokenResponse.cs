using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
    public class MealTokenResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public MealDistributionInfo? MealInfo { get; set; }
    }

    public class MealDistributionInfo
    {
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; }
        public int MealTypeId { get; set; }
        public int? MealSubTypeId { get; set; }
        public string? MealTypeName { get; set; }
        public string? MealSubTypeName { get; set; }
        public int SupplierId { get; set; }
        public TimeOnly TokenIssueStartTime { get; set; }
        public TimeOnly TokenIssueEndTime { get; set; }
    }
}
