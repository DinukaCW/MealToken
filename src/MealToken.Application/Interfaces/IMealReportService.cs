using Authentication.Models.DTOs;
using MealToken.Application.Services;
using MealToken.Domain.Enums;
using MealToken.Domain.Models;
using MealToken.Domain.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Interfaces
{
    public interface IMealReportService
    {
        Task<ServiceResult<MealConsumptionReportDTO>> GenerateWeeklyReportAsync(DateOnly startDate, DateOnly endDate);
        Task<ServiceResult<MealConsumptionReportDTO>> GenerateCurrentWeekReportAsync();
        Task<ServiceResult<ReportDashBoard>> GetDashboardSummaryAsync();
        Task<ServiceResult> GetMealConsumptionSummaryAsync(DateOnly startDate, DateOnly? endDate = null);
        Task<SupplierPaymentReportDto> GetSupplierPaymentReportAsync(int supplierId, DateOnly startDate, DateOnly? endDate = null);
		Task<ServiceResult> GetAllSuppliersPaymentReportAsync(DateOnly startDate, DateOnly? endDate = null);
        Task<ServiceResult> GetTodayMealSchedulesAsync(DateOnly date, TimeOnly time);
        Task<MealDashboardDto> GetMealDashboardDataAsync(TimePeriod timePeriod, List<int> departmentIds, DateOnly? customStartDate = null, DateOnly? customEndDate = null);
        Task<DashBoardDepartment> GetMealsByDepartmentAsync(TimePeriod timePeriod, List<int> departmentIds, DateOnly? customStartDate = null, DateOnly? customEndDate = null);
        Task<DashboardSupplier> GetMealsBySupplierAsync(TimePeriod timePeriod, List<int> departmentIds, DateOnly? customStartDate = null, DateOnly? customEndDate = null);
        Task<DashBoardMealType> GetMealsByMealTypeAsync(TimePeriod timePeriod, List<int> departmentIds, DateOnly? customStartDate = null, DateOnly? customEndDate = null);
		Task<DashBoardCostAnalysis> GetMealsByCostAsync(TimePeriod timePeriod,List<int> departmentIds,DateOnly? customStartDate = null,DateOnly? customEndDate = null);

	}
}
