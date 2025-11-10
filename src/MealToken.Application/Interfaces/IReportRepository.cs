using Authentication.Models.DTOs;
using Authentication.Models.Entities;
using MealToken.Domain.Entities;
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
	public interface IReportRepository
	{

		Task<List<MealConsumptionWithDetails>> GetMealConsumptioninWeekAsync(DateOnly startDate, DateOnly endDate, TimeOnly? startTime = null,TimeOnly? endTime = null);
		Task<int> GetMealsServedThisMonthAsync();
		Task<int> GetMealsServedLastMonthAsync();
		Task<int> GetActiveEmployeesCountAsync();
		Task<int> GetActiveVisitorsCountAsync();
		Task<int> GetPendingRequestsCountAsync();
		Task<List<MealConsumptionData>> GetMealConsumptionSummaryByDateRangeAsync(DateOnly startDate, DateOnly endDate, TimeOnly? startTime, TimeOnly? endTime);
		Task<int> GetTotalMealsServedByDateRangeAsync(DateOnly startDate, DateOnly endDate);
		Task<int> GetUniqueMealTypesCountByDateRangeAsync(DateOnly startDate, DateOnly endDate);
		Task<decimal> GetTotalRevenueByDateRangeAsync(DateOnly startDate, DateOnly endDate);
		Task<List<SupplierMealDetailDto>> GetSupplierMealDetailsByDateRangeAsync(int supplierId, DateOnly startDate, DateOnly endDate);
		Task<SupplierSummaryDto> GetSupplierSummaryByDateRangeAsync(int supplierId, DateOnly startDate, DateOnly endDate);
		Task<Supplier> GetSupplierInfoAsync(int supplierId);
		Task<List<int>> GetActiveSupplierIdsByDateRangeAsync(DateOnly startDate, DateOnly endDate);
		Task<List<int>> GetPersonIdsByDepartmentsAsync(List<int> departmentIds);
		Task<int> GetTotalMealsServedAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<decimal> GetTotalCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<List<MealTypeDistributionDto>> GetMealDistributionByTypeAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<MealRequestsDto> GetSpecialRequestsAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<List<GraphDataPoint>> GetAggregatedConsumptionDataAsync(DateOnly startDate, DateOnly endDate, GroupingLevel grouping, TimePeriod timePeriod, List<int> personIds = null);
		Task<List<InternalDepartmentMealStats>> GetAggregatedMealStatsAsync(DateOnly startDate, DateOnly endDate, List<int> departmentIds);
		Task<Dictionary<int, string>> GetDepartmentNamesAsync(List<int> departmentIds);
		Task<decimal> GetTotalSupplierCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<List<SupplierWiseMeals>> GetSupplierBreakdownAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<List<MealTypeRawData>> GetMealTypeRawDataAsync(DateOnly startDate, DateOnly endDate, List<int> personIds);
		Task<decimal> GetTotalEmployeeCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<decimal> GetTotalCompanyCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<decimal> GetTotalSellingPriceAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<decimal> GetTotalSupplierMealCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<List<int>> GetFilteredPersonIdsByTypeAsync(List<int> personIds, PersonType personType);
		Task<List<UserHistory>> GetActivityLogsAsync(DateTime? startDateTime, DateTime? endDateTime, List<string>? entityType, List<string>? actionType, List<int>? userId);
		Task<User> GetUserByIdAsync(int userId);
		Task<string> GetUserRoleNameAsync(int userRoleId);
		Task<List<UserDto>> GetAllUsersAsync();
		Task<List<SupplierRequestDetailDto>> GetRequestBySupplierAsync(int supplierId, DateOnly startDate, DateOnly endDate);
		Task<SupplierRequestCosts> GetSupplierRequestCostDetailsAsync(int supplierId, DateOnly startDate, DateOnly endDate);
		Task<int> GetTotalRequestMealssAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<decimal> GetTotalRequestsCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null);
		Task<string> GetPersonNumberByIdsync(int personId);
		Task<List<GraphDataPoint>> GetAggregatedRequestConsumptionDataAsync(DateOnly startDate, DateOnly endDate, GroupingLevel grouping, TimePeriod timePeriod);
		Task<List<MealTypeDistributionDto>> GetRequestMealDistributionByTypeAsync(DateOnly startDate, DateOnly endDate);
	}
}
