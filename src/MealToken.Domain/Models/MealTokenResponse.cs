using MealToken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MealToken.Domain.Models.MealTypeUpdateDto;

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
        public List<MealAddOnDto> MealAddOns { get; set; }
    }
    public class MealAddOnDto
    {
        public int AddOnMealTypeId { get; set; }
        public int AddOnSubTypeId { get; set; }
        public string AddOnName { get; set; }
        public AddOnType AddonType { get; set; }
        public int SupplierId { get; set; }
    }
}
