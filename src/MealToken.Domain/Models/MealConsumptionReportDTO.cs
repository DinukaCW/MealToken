using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models
{
    public class MealConsumptionReportDTO
    {
        public List<DailyMealReport> DailyReports { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }

    public class DailyMealReport
    {
        public DateOnly Date { get; set; }
        public List<MealTypeGroup> MealTypeGroups { get; set; }
        public DailyTotalSummary DailyTotal { get; set; }
    }

    public class MealTypeGroup
    {
        public string MealTypeName { get; set; }
        public List<MealConsumptionDetail> Details { get; set; }
        public List<SubTypeSubTotal> SubTypeSubTotals { get; set; }
        public MealTypeSummary MealTypeTotal { get; set; }
    }

    public class MealConsumptionDetail
    {
        public int EmployeeNumber { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Gender { get; set; }
        public string Subtype { get; set; }
        public decimal EmployeeContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal TotalSupplierCost { get; set; }
    }

    public class SubTypeSubTotal
    {
        public string SubTypeName { get; set; }
        public int EmployeeCount { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public decimal TotalEmployeeContribution { get; set; }
        public decimal TotalEmployerCost { get; set; }
        public decimal TotalSupplierCost { get; set; }
        public int TotalMealCount { get; set; }
    }

    public class MealTypeSummary
    {
        public int EmployeeCount { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public decimal TotalEmployeeContribution { get; set; }
        public decimal TotalEmployerCost { get; set; }
        public decimal TotalSupplierCost { get; set; }
        public int TotalMealCount { get; set; }
    }

    public class DailyTotalSummary
    {
        public int TotalMealTypeCount { get; set; }
        public decimal GrandTotalEmployeeContribution { get; set; }
        public decimal GrandTotalEmployerCost { get; set; }
        public decimal GrandTotalSupplierCost { get; set; }
        public int GrandTotalMealCount { get; set; }
    }
}
