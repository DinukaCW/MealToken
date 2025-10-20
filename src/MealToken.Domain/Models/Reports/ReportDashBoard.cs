using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
    public class ReportDashBoard
    {
        public MealsServedCard MealsServedThisMonth { get; set; }
        public ActiveEmployeesCard ActiveEmployees { get; set; }
        public ActiveVisitorsCard ActiveVisitors { get; set; }
        public PendingRequestsCard PendingRequests { get; set; }
    }

    public class MealsServedCard
    {
        public int TotalMeals { get; set; }
        public decimal PercentageChange { get; set; }
        public bool IsIncrease { get; set; }
    }

    public class ActiveEmployeesCard
    {
        public int TotalActiveEmployees { get; set; }
        public string Status { get; set; } // "Registered in system"
    }

    public class ActiveVisitorsCard
    {
        public int TotalActiveVisitors { get; set; }
        public string Status { get; set; } // "Registered in system"
    }

    public class PendingRequestsCard
    {
        public int TotalPendingRequests { get; set; }
        public string Status { get; set; } // "Awaiting approval"

    }
}
