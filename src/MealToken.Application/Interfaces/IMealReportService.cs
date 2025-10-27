using Authentication.Models.DTOs;
using MealToken.Application.Services;
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
	}
}
