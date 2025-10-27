using MealToken.Domain.Entities;
using MealToken.Domain.Models;
using MealToken.Domain.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Interfaces
{
	public interface IReportRepository
	{
		Task<List<MealConsumptionWithDetails>> GetMealConsumptioninWeekAsync(DateOnly startDate, DateOnly endDate);
		Task<int> GetMealsServedThisMonthAsync();
		Task<int> GetMealsServedLastMonthAsync();
		Task<int> GetActiveEmployeesCountAsync();
		Task<int> GetActiveVisitorsCountAsync();
		Task<int> GetPendingRequestsCountAsync();
		Task<List<MealConsumptionData>> GetMealConsumptionSummaryByDateRangeAsync(DateOnly startDate, DateOnly endDate);
		Task<int> GetTotalMealsServedByDateRangeAsync(DateOnly startDate, DateOnly endDate);
		Task<int> GetUniqueMealTypesCountByDateRangeAsync(DateOnly startDate, DateOnly endDate);
		Task<decimal> GetTotalRevenueByDateRangeAsync(DateOnly startDate, DateOnly endDate);
		Task<List<SupplierMealDetailDto>> GetSupplierMealDetailsByDateRangeAsync(int supplierId, DateOnly startDate, DateOnly endDate);
		Task<SupplierSummaryDto> GetSupplierSummaryByDateRangeAsync(int supplierId, DateOnly startDate, DateOnly endDate);
		Task<Supplier> GetSupplierInfoAsync(int supplierId);
		Task<List<int>> GetActiveSupplierIdsByDateRangeAsync(DateOnly startDate, DateOnly endDate);
	}
}
