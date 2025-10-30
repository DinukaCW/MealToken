using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Models;
using MealToken.Domain.Models.Reports;
using MealToken.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Infrastructure.Repositories
{
	public class ReportRepository : IReportRepository
	{
		private readonly MealTokenDbContext _tenantContext;
		private readonly PlatformDbContext _platformContext;
		private readonly ITenantContext _currentTenant;
		private readonly ILogger<AdminRepository> _logger;

		public ReportRepository(
			MealTokenDbContext tenantContext,
			PlatformDbContext platformContext,
			ITenantContext currentTenant,
			ILogger<AdminRepository> logger)
		{
			_tenantContext = tenantContext;
			_platformContext = platformContext;
			_currentTenant = currentTenant;
			_logger = logger;
		}
		public async Task<List<MealConsumptionWithDetails>> GetMealConsumptioninWeekAsync(DateOnly startDate, DateOnly endDate)
		{
			// Step 1: Query the first context (_tenantContext) and bring the initial data into memory.
			// We select into an anonymous type to get just what we need.
			var consumptionsFromTenantDb = await (
				from mc in _tenantContext.MealConsumption
				join p in _tenantContext.Person on mc.PersonId equals p.PersonId
				where mc.Date >= startDate
					  && mc.Date <= endDate
					  && mc.TockenIssued
				select new
				{
					Consumption = mc, // Keep the original consumption object
					p.DepartmentId,
					p.DesignationId
				})
				.ToListAsync();

			// If there's no data, return an empty list early.
			if (!consumptionsFromTenantDb.Any())
			{
				return new List<MealConsumptionWithDetails>();
			}

			// Step 2: Get the unique IDs needed for the second context.
			var departmentIds = consumptionsFromTenantDb.Select(c => c.DepartmentId).Distinct().ToList();
			var designationIds = consumptionsFromTenantDb.Select(c => c.DesignationId).Distinct().ToList();

			// Step 3: Query the second context (_platformContext) for departments and designations.
			// Fetching into dictionaries makes the lookup in the next step very fast.
			var departments = await _platformContext.Department
				.Where(d => departmentIds.Contains(d.DepartmnetId))
				.ToDictionaryAsync(d => d.DepartmnetId, d => d.Name);

			var designations = await _platformContext.Designation
				.Where(des => designationIds.Contains(des.DesignationId))
				.ToDictionaryAsync(des => des.DesignationId, des => des.Title);

			// Step 4: Join the data in memory to build the final list.
			var result = consumptionsFromTenantDb.Select(c => new MealConsumptionWithDetails
			{
				MealConsumptionId = c.Consumption.MealConsumptionId,
				Date = c.Consumption.Date,
				PersonId = c.Consumption.PersonId,
				PersonName = c.Consumption.PersonName,
				Gender = c.Consumption.Gender,
				// Look up the name from the dictionaries. Provide a default value if not found.
				DepartmentName = departments.TryGetValue(c.DepartmentId, out var deptName) ? deptName : "N/A",
				DesignationName = designations.TryGetValue(c.DesignationId ?? 11, out var desigTitle) ? desigTitle : "N/A",
				MealTypeName = c.Consumption.MealTypeName,
				SubTypeName = c.Consumption.SubTypeName,
				EmployeeCost = c.Consumption.EmployeeCost,
				CompanyCost = c.Consumption.CompanyCost,
				SupplierCost = c.Consumption.SupplierCost
			}).ToList();

			// Apply sorting to the final in-memory list
			return result
				.OrderBy(m => m.Date)
				.ThenBy(m => m.MealTypeName)
				.ThenBy(m => m.SubTypeName)
				.ThenBy(m => m.PersonId)
				.ToList();
		}

		public async Task<int> GetMealsServedThisMonthAsync()
		{
			var today = DateTime.Now;
			var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);
			var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

			return await _tenantContext.MealConsumption
				.Where(m => m.Date >= firstDayOfMonth
							 && m.Date <= lastDayOfMonth
							 && m.TockenIssued)
				.CountAsync();
		}

		public async Task<int> GetMealsServedLastMonthAsync()
		{
			var today = DateTime.Now;
			var firstDayOfLastMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
			var lastDayOfLastMonth = new DateOnly(today.Year, today.Month, 1).AddDays(-1);

			return await _tenantContext.MealConsumption
				.Where(m => m.Date >= firstDayOfLastMonth
						 && m.Date <= lastDayOfLastMonth
						 && m.TockenIssued)
				.CountAsync();
		}

		public async Task<int> GetActiveEmployeesCountAsync()
		{
			return await _tenantContext.Person
				.Where(e => e.PersonType == PersonType.Employer && e.IsActive == true)
				.CountAsync();
		}

		public async Task<int> GetActiveVisitorsCountAsync()
		{

			return await _tenantContext.Person
				.Where(v => v.PersonType == PersonType.Visitor && v.IsActive == true)
				.CountAsync();
		}

		public async Task<int> GetPendingRequestsCountAsync()
		{
			return await _tenantContext.Request
				.Where(r => r.Status == UserRequestStatus.Pending)
				.CountAsync();
		}
		public async Task<List<MealConsumptionData>> GetMealConsumptionSummaryByDateRangeAsync(DateOnly startDate,DateOnly endDate)
		{
			return await _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued)
				.GroupBy(m => new
				{
					m.Date,
					m.MealTypeName,
					m.SubTypeName
				})
				.Select(g => new MealConsumptionData
				{
					Date = g.Key.Date,
					MealType = g.Key.MealTypeName,
					SubType = g.Key.SubTypeName,
					EmployeeContribution = g.Average(x => x.EmployeeCost),
					CompanyContribution = g.Average(x => x.CompanyCost),
					SupplierContribution = g.Average(x => x.SupplierCost),
					TotalMealCount = g.Count(),
					PersonCount = g.Select(x => x.PersonId).Distinct().Count(),
					MaleCount = g.Count(x => x.Gender == "Male"),
					FemaleCount = g.Count(x => x.Gender == "Female"),
					TotalEmployeeContribution = g.Sum(x => x.EmployeeCost),
					TotalEmployerContribution = g.Sum(x => x.CompanyCost),
					TotalSupplierCost = g.Sum(x => x.SupplierCost)
				})
				.OrderBy(x => x.Date)
				.ThenBy(x => x.MealType)
				.ThenBy(x => x.SubType)
				.ToListAsync();
		}
		public async Task<int> GetTotalMealsServedByDateRangeAsync(DateOnly startDate, DateOnly endDate)
		{
			return await _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued)
				.CountAsync();
		}

		public async Task<int> GetUniqueMealTypesCountByDateRangeAsync(DateOnly startDate, DateOnly endDate)
		{
			return await _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued)
				.Select(m => m.MealTypeName)
				.Distinct()
				.CountAsync();
		}

		public async Task<decimal> GetTotalRevenueByDateRangeAsync(DateOnly startDate, DateOnly endDate)
		{
			return await _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued)
				.SumAsync(m => m.EmployeeCost + m.CompanyCost);
		}
		public async Task<List<SupplierMealDetailDto>> GetSupplierMealDetailsByDateRangeAsync(int supplierId,DateOnly startDate,DateOnly endDate)
		{
			return await _tenantContext.MealConsumption
				.Join(_tenantContext.MealCost,
					mc => mc.MealCostId,
					cost => cost.MealCostId,
					(mc, cost) => new { MealConsumption = mc, MealCost = cost })
				.Where(x => x.MealCost.SupplierId == supplierId &&
							x.MealConsumption.Date >= startDate &&
							x.MealConsumption.Date <= endDate &&
							x.MealConsumption.TockenIssued)
				.GroupBy(x => new
				{
					x.MealConsumption.Date,
					x.MealConsumption.MealTypeName,
					x.MealConsumption.SubTypeName,
					x.MealConsumption.SupplierCost
				})
				.Select(g => new SupplierMealDetailDto
				{
					Date = g.Key.Date,
					MealType = g.Key.MealTypeName,
					SubMealType = g.Key.SubTypeName,
					UnitPrice = g.Key.SupplierCost,
					QuantityMale = g.Count(x => x.MealConsumption.Gender == "Male"),
					QuantityFemale = g.Count(x => x.MealConsumption.Gender == "Female"),
					TotalQuantity = g.Count(),
					Amount = g.Sum(x => x.MealConsumption.SupplierCost)
				})
				.OrderBy(x => x.Date)
				.ThenBy(x => x.MealType)
				.ThenBy(x => x.SubMealType)
				.ToListAsync();
		}

		public async Task<SupplierSummaryDto> GetSupplierSummaryByDateRangeAsync(int supplierId,DateOnly startDate,DateOnly endDate)
		{
			var data = await _tenantContext.MealConsumption
				.Join(_tenantContext.MealCost,
					mc => mc.MealCostId,
					cost => cost.MealCostId,
					(mc, cost) => new { MealConsumption = mc, MealCost = cost })
				.Where(x => x.MealCost.SupplierId == supplierId &&
							x.MealConsumption.Date >= startDate &&
							x.MealConsumption.Date <= endDate &&
							x.MealConsumption.TockenIssued)
				.ToListAsync();

			if (!data.Any())
			{
				return new SupplierSummaryDto();
			}

			var summary = new SupplierSummaryDto
			{
				TotalPersonCount = data.Select(x => x.MealConsumption.PersonId).Distinct().Count(),
				MaleCount = data.Count(x => x.MealConsumption.Gender == "Male"),
				FemaleCount = data.Count(x => x.MealConsumption.Gender == "Female"),
				TotalEmployeeContribution = data.Sum(x => x.MealConsumption.EmployeeCost),
				TotalEmployerCost = data.Sum(x => x.MealConsumption.CompanyCost),
				TotalSupplierCost = data.Sum(x => x.MealConsumption.SupplierCost),
				TotalMealCount = data.Count()
			};

			return summary;
		}

		public async Task<Supplier> GetSupplierInfoAsync(int supplierId)
		{
			return await _tenantContext.Supplier
				.Where(s => s.SupplierId == supplierId && s.IsActive)
				.FirstOrDefaultAsync();
		}

		public async Task<List<int>> GetActiveSupplierIdsByDateRangeAsync(DateOnly startDate,DateOnly endDate)
		{
			return await _tenantContext.MealConsumption
				.Join(_tenantContext.MealCost,
					mc => mc.MealCostId,
					cost => cost.MealCostId,
					(mc, cost) => new { MealConsumption = mc, MealCost = cost })
				.Where(x => x.MealConsumption.Date >= startDate &&
							x.MealConsumption.Date <= endDate &&
							x.MealConsumption.TockenIssued)
				.Select(x => x.MealCost.SupplierId)
				.Distinct()
				.ToListAsync();
		}

		public async Task<List<int>> GetPersonIdsByDepartmentsAsync(List<int> departmentIds)
		{
			if (departmentIds == null || !departmentIds.Any())
				return null;

			return await _tenantContext.Person
				.Where(p => departmentIds.Contains(p.DepartmentId))
				.Select(p => p.PersonId)
				.ToListAsync();
		}

		public async Task<int> GetTotalMealsServedAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
		{
			var query = _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued);

			if (personIds != null && personIds.Any())
			{
				query = query.Where(m => personIds.Contains(m.PersonId));
			}

			return await query.CountAsync();
		}
		public async Task<decimal> GetTotalCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
		{
			var query = _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued);

			if (personIds != null && personIds.Any())
			{
				query = query.Where(m => personIds.Contains(m.PersonId));
			}

			return await query.SumAsync(m => m.CompanyCost);
		}
		public async Task<List<MealTypeDistributionDto>> GetMealDistributionByTypeAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
		{
			var query = _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued);

			if (personIds != null && personIds.Any())
			{
				query = query.Where(m => personIds.Contains(m.PersonId));
			}

			var distribution = await query
				.GroupBy(m => m.MealTypeName)
				.Select(g => new MealTypeDistributionDto
				{
					MealType = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			var totalCount = distribution.Sum(x => x.Count);

			foreach (var item in distribution)
			{
				item.Percentage = totalCount > 0 ? (decimal)item.Count / totalCount * 100 : 0;
			}

			return distribution.OrderByDescending(x => x.Count).ToList();
		}

		public async Task<MealRequestsDto> GetSpecialRequestsAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
		{
			var query = _tenantContext.Request
			.Where(r => r.EventDate >= startDate && r.EventDate <= endDate);

			// Apply person ID filter only if provided
			if (personIds != null && personIds.Any())
			{
				query = query.Where(r => personIds.Contains(r.RequesterId));
			}

			// Execute query asynchronously
			var requests = await query.ToListAsync();

			// Prepare the result DTO
			return new MealRequestsDto
			{
				TotalRequests = requests.Count,
				ApprovedRequests = requests.Count(r => r.Status == UserRequestStatus.Approved),
				PendingRequests = requests.Count(r => r.Status == UserRequestStatus.Pending)
			};
		}

		public async Task<List<GraphDataPoint>> GetAggregatedConsumptionDataAsync(
	DateOnly startDate,
	DateOnly endDate,
	GroupingLevel grouping,
	TimePeriod timePeriod,
	List<int> personIds = null)
		{
			// Base query with filters
			var query = _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued);

			// Apply optional person filter
			if (personIds != null && personIds.Any())
			{
				query = query.Where(m => personIds.Contains(m.PersonId));
			}

			var results = new List<GraphDataPoint>();

			switch (grouping)
			{
				case GroupingLevel.ByDay:
					var dailyData = await query
						.GroupBy(mc => mc.Date)
						.Select(g => new {
							Date = g.Key,
							MealCount = g.Count(),
							TotalCost = g.Sum(mc => mc.CompanyCost)
						})
						.OrderBy(x => x.Date)
						.ToListAsync();

					results = dailyData.Select(d => new GraphDataPoint
					{
						Label = (timePeriod == TimePeriod.ThisWeek || timePeriod == TimePeriod.LastWeek)
								? d.Date.ToString("ddd") // "Mon", "Tue"
								: d.Date.ToString("MMM d"), // "Oct 29"
						MealCount = d.MealCount,
						TotalCost = d.TotalCost
					}).ToList();
					break;

				case GroupingLevel.ByWeek:
					// ✅ FIXED: Use week of MONTH, not week of YEAR
					var rawDataForWeek = await query
						.Select(mc => new { mc.Date, mc.CompanyCost })
						.ToListAsync();

					var weeklyData = rawDataForWeek
						.GroupBy(mc => new {
							Year = mc.Date.Year,
							Month = mc.Date.Month,
							Week = GetWeekOfMonth(mc.Date) // ✅ Week within the month
						})
						.Select(g => new {
							Year = g.Key.Year,
							Month = g.Key.Month,
							WeekNumber = g.Key.Week,
							StartDate = g.Min(x => x.Date),
							MealCount = g.Count(),
							TotalCost = g.Sum(mc => mc.CompanyCost)
						})
						.OrderBy(x => x.StartDate);

					results = weeklyData.Select(w => new GraphDataPoint
					{
						Label = $"Week {w.WeekNumber}",
						MealCount = w.MealCount,
						TotalCost = w.TotalCost
					}).ToList();
					break;

				case GroupingLevel.ByMonth:
					// ✅ FIXED: Group by Year AND Month, not just Month
					var rawDataForMonth = await query
						.Select(mc => new { mc.Date, mc.CompanyCost })
						.ToListAsync();

					var monthlyData = rawDataForMonth
						.GroupBy(mc => new { mc.Date.Year, mc.Date.Month }) // ✅ Include Year
						.Select(g => new {
							Year = g.Key.Year,
							Month = g.Key.Month,
							Date = new DateOnly(g.Key.Year, g.Key.Month, 1),
							MealCount = g.Count(),
							TotalCost = g.Sum(mc => mc.CompanyCost)
						})
						.OrderBy(x => x.Date);

					results = monthlyData.Select(m => new GraphDataPoint
					{
						// Show year only for multi-year periods
						Label = (timePeriod == TimePeriod.ThisYear || timePeriod == TimePeriod.LastYear)
								? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Month) // "January"
								: $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m.Month)} {m.Year}", // "Jan 2025"
						MealCount = m.MealCount,
						TotalCost = m.TotalCost
					}).ToList();
					break;
			}

			return results;
		}

		public async Task<List<DepartmentPersonGroupDto>> GetPersonsGroupedByDepartmentAsync(IEnumerable<int>? departmentIds)
		{
			// Step 1: Get departments from platform context
			var departmentsQuery = _platformContext.Department.AsQueryable();

			if (departmentIds != null && departmentIds.Any())
			{
				departmentsQuery = departmentsQuery.Where(d => departmentIds.Contains(d.DepartmnetId));
			}

			var departments = await departmentsQuery
				.Select(d => new { d.DepartmnetId, d.Name })
				.ToListAsync();

			// Step 2: Get persons from tenant context (filter by those department IDs)
			var departmentIdsList = departments.Select(d => d.DepartmnetId).ToList();

			var persons = await _tenantContext.Person
				.Where(p => departmentIdsList.Contains(p.DepartmentId))
				.Select(p => new { p.PersonId, p.DepartmentId })
				.ToListAsync();

			// Step 3: Combine results in memory
			var result = departments
				.Select(d => new DepartmentPersonGroupDto
				{
					DepartmentId = d.DepartmnetId,
					DepartmentName = d.Name,
					Persons = persons
						.Where(p => p.DepartmentId == d.DepartmnetId)
						.Select(p => p.PersonId)
						.ToList()
				})
				.ToList();

			return result;
		}


		public async Task<decimal> GetTotalSupplierCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
		{
			var query = _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued);

			if (personIds != null && personIds.Any())
			{
				query = query.Where(m => personIds.Contains(m.PersonId));
			}
			return await query.SumAsync(m => m.SupplierCost);
		}

		public async Task<List<SupplierWiseMeals>> GetSupplierBreakdownAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
		{
			var query = _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued);

			if (personIds != null && personIds.Any())
			{
				query = query.Where(m => personIds.Contains(m.PersonId));
			}

			var groupedData = await query
				.Join(_tenantContext.MealCost, // Join to get SupplierId
					mc => mc.MealCostId,
					cost => cost.MealCostId,
					(mc, cost) => new { MealConsumption = mc, cost.SupplierId })
				.Join(_tenantContext.Supplier, // Join to get SupplierName
					mc_cost => mc_cost.SupplierId,
					supplier => supplier.SupplierId,
					(mc_cost, supplier) => new { mc_cost.MealConsumption, Supplier = supplier })
				.GroupBy(x => new { x.Supplier.SupplierId, x.Supplier.SupplierName })
				.Select(g => new SupplierWiseMeals
				{
					SupplierId = g.Key.SupplierId,
					SupplierName = g.Key.SupplierName,
					MealCount = g.Count(),
					SupplierCost = g.Sum(x => x.MealConsumption.SupplierCost),
					Precentage = 0 
				})
				.ToListAsync();

			return groupedData;
		}
		
		public async Task<decimal> GetTotalEmployeeCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
		{
			var query = _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued);

			if (personIds != null && personIds.Any())
			{
				query = query.Where(m => personIds.Contains(m.PersonId));
			}

			// Sum EmployeeCost field
			return await query.SumAsync(m => m.EmployeeCost);
		}
		public async Task<List<MealTypeRawData>> GetMealTypeRawDataAsync(DateOnly startDate,DateOnly endDate,List<int> personIds)
		{
			var query = _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued);

			if (personIds != null && personIds.Any())
			{
				query = query.Where(m => personIds.Contains(m.PersonId));
			}

			// Select only the columns needed for grouping
			return await query
				.Select(mc => new MealTypeRawData
				{
					MealTypeId = mc.MealTypeId,
					MealTypeName = mc.MealTypeName,
					SubTypeId = mc.SubTypeId,
					SubTypeName = mc.SubTypeName
				})
				.ToListAsync();
		}
		public async Task<List<int>> GetFilteredPersonIdsByTypeAsync(List<int> personIds, PersonType personType)
		{
			if (personIds == null || !personIds.Any())
			{
				return new List<int>();
			}
			return await _tenantContext.Person
				.Where(p => personIds.Contains(p.PersonId) && // Filter from the master list
							p.PersonType == personType)      // Filter by the desired type
				.Select(p => p.PersonId)
				.ToListAsync();
		}

		private int GetWeekOfMonth(DateOnly date)
		{
			var firstDayOfMonth = new DateOnly(date.Year, date.Month, 1);
			int daysOffset = (int)firstDayOfMonth.DayOfWeek;
			int weekNumber = (date.Day + daysOffset - 1) / 7 + 1;
			return weekNumber;
		}

	}
	
}
