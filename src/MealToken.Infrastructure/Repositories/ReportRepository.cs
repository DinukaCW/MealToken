using Authentication.Models.Entities;
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
		public async Task<List<MealConsumptionWithDetails>> GetMealConsumptioninWeekAsync(DateOnly startDate, DateOnly endDate, TimeOnly? startTime = null,
	TimeOnly? endTime = null)
		{
			// Step 1: Query the first context (_tenantContext) and bring the initial data into memory.
			// We select into an anonymous type to get just what we need.
			var consumptionsFromTenantDb = await (
				from mc in _tenantContext.MealConsumption
				join p in _tenantContext.Person on mc.PersonId equals p.PersonId
				where mc.Date >= startDate
					  && mc.Date <= endDate
					  && mc.TockenIssued && (
							startTime == null || endTime == null ||
							(
								mc.Time >= startTime.Value &&
								mc.Time <= endTime.Value
							))
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
		public async Task<List<MealConsumptionData>> GetMealConsumptionSummaryByDateRangeAsync(DateOnly startDate,DateOnly endDate, TimeOnly? startTime, TimeOnly? endTime)
		{
			return await _tenantContext.MealConsumption
				.Where(m => m.Date >= startDate &&
							m.Date <= endDate &&
							m.TockenIssued
							&& (
								startTime == null || endTime == null ||
								(
									m.Time >= startTime.Value &&
									m.Time <= endTime.Value
								)
							 ))
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
					TotalCompanyContribution = g.Sum(x => x.CompanyCost),
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
					x.MealConsumption.SellingPrice
				})
				.Select(g => new SupplierMealDetailDto
				{
					Date = g.Key.Date,
					MealType = g.Key.MealTypeName,
					SubMealType = g.Key.SubTypeName,
					UnitPrice = g.Key.SellingPrice,
					QuantityMale = g.Count(x => x.MealConsumption.Gender == "Male"),
					QuantityFemale = g.Count(x => x.MealConsumption.Gender == "Female"),
					TotalQuantity = g.Count(),
					Amount = g.Sum(x => x.MealConsumption.SellingPrice)
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
				TotalCompanyContribution = data.Sum(x => x.MealConsumption.CompanyCost),
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
			
			return await query.SumAsync(m => m.SupplierCost);
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
							TotalCost = g.Sum(mc => mc.SupplierCost)
						})
						.ToListAsync();

					var dailyDataDict = dailyData.ToDictionary(d => d.Date);
					var currentDate = startDate;

					while (currentDate <= endDate)
					{
						var hasData = dailyDataDict.TryGetValue(currentDate, out var data);

						results.Add(new GraphDataPoint
						{
							Label = (timePeriod == TimePeriod.ThisWeek || timePeriod == TimePeriod.LastWeek)
									? currentDate.ToString("ddd") // "Mon", "Tue"
									: currentDate.ToString("MMM d"), // "Oct 29"
							MealCount = hasData ? data.MealCount : 0,
							TotalCost = hasData ? data.TotalCost : 0
						});

						currentDate = currentDate.AddDays(1);
					}
					break;

				case GroupingLevel.ByWeek:
					var rawDataForWeek = await query
						.Select(mc => new { mc.Date, mc.SupplierCost })
						.ToListAsync();

					var weeklyData = rawDataForWeek
						.GroupBy(mc => new {
							Year = mc.Date.Year,
							Month = mc.Date.Month,
							Week = GetWeekOfMonth(mc.Date)
						})
						.Select(g => new {
							Year = g.Key.Year,
							Month = g.Key.Month,
							WeekNumber = g.Key.Week,
							MealCount = g.Count(),
							TotalCost = g.Sum(mc => mc.SupplierCost)
						})
						.ToList();

					var weeklyDataDict = weeklyData.ToDictionary(
						w => (w.Year, w.Month, w.WeekNumber),
						w => w
					);

					var weekPeriods = GetAllWeekPeriodsInRange(startDate, endDate);

					foreach (var period in weekPeriods)
					{
						var hasData = weeklyDataDict.TryGetValue(
							(period.Year, period.Month, period.WeekNumber),
							out var data
						);

						results.Add(new GraphDataPoint
						{
							Label = $"Week {period.WeekNumber}",
							MealCount = hasData ? data.MealCount : 0,
							TotalCost = hasData ? data.TotalCost : 0
						});
					}
					break;

				case GroupingLevel.ByMonth:
					var rawDataForMonth = await query
						.Select(mc => new { mc.Date, mc.SupplierCost })
						.ToListAsync();

					var monthlyData = rawDataForMonth
						.GroupBy(mc => new { mc.Date.Year, mc.Date.Month })
						.Select(g => new {
							Year = g.Key.Year,
							Month = g.Key.Month,
							MealCount = g.Count(),
							TotalCost = g.Sum(mc => mc.SupplierCost)
						})
						.ToList();

					var monthlyDataDict = monthlyData.ToDictionary(
						m => (m.Year, m.Month),
						m => m
					);

					var currentMonth = new DateOnly(startDate.Year, startDate.Month, 1);
					var endMonth = new DateOnly(endDate.Year, endDate.Month, 1);

					while (currentMonth <= endMonth)
					{
						var hasData = monthlyDataDict.TryGetValue(
							(currentMonth.Year, currentMonth.Month),
							out var data
						);

						results.Add(new GraphDataPoint
						{
							Label = (timePeriod == TimePeriod.ThisYear || timePeriod == TimePeriod.LastYear)
									? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth.Month)
									: $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(currentMonth.Month)} {currentMonth.Year}",
							MealCount = hasData ? data.MealCount : 0,
							TotalCost = hasData ? data.TotalCost : 0
						});

						currentMonth = currentMonth.AddMonths(1);
					}
					break;
			}

			return results;
		}

		
		// Add this method to your ReportRepository
		public async Task<List<InternalDepartmentMealStats>> GetAggregatedMealStatsAsync(
			DateOnly startDate, DateOnly endDate, List<int> departmentIds)
		{
			// This query joins MealConsumption and Person (both in _tenantContext)
			var query = from c in _tenantContext.MealConsumption
						join p in _tenantContext.Person on c.PersonId equals p.PersonId
						where c.Date >= startDate
						   && c.Date <= endDate
						   && c.TockenIssued
						   && departmentIds.Contains(p.DepartmentId)
						group new { c, p } by p.DepartmentId into g
						select new InternalDepartmentMealStats
						{
							DepartmentId = g.Key,
							TotalCount = g.Count(),
							TotalCost = g.Sum(x => x.c.SupplierCost),

							EmployeeCount = g.Count(x => x.p.PersonType == PersonType.Employer),
							EmployeeCost = g.Where(x => x.p.PersonType == PersonType.Employer)
											.Sum(x => x.c.SupplierCost),

							VisitorCount = g.Count(x => x.p.PersonType == PersonType.Visitor),
							VisitorCost = g.Where(x => x.p.PersonType == PersonType.Visitor)
										   .Sum(x => x.c.SupplierCost)
						};

			return await query.ToListAsync();
		}
		// Add this method to your ReportRepository
		public async Task<Dictionary<int, string>> GetDepartmentNamesAsync(List<int> departmentIds)
		{
			var query = _platformContext.Department.AsQueryable();

			if (departmentIds?.Any() == true)
			{
				query = query.Where(d => departmentIds.Contains(d.DepartmnetId));
			}

			return await query
				.ToDictionaryAsync(d => d.DepartmnetId, d => d.Name);
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
					SupplierSellingPrice = g.Sum(x => x.MealConsumption.SupplierCost),
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

		public async Task<decimal> GetTotalCompanyCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
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
			return await query.SumAsync(m => m.CompanyCost);
		}
		public async Task<decimal> GetTotalSellingPriceAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
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
			return await query.SumAsync(m => m.SellingPrice);
		}
		public async Task<decimal> GetTotalSupplierMealCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
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
			return await query.SumAsync(m => m.SupplierCost);
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
		private List<(int Year, int Month, int WeekNumber)> GetAllWeekPeriodsInRange(
		DateOnly startDate,
		DateOnly endDate)
		{
			var periods = new HashSet<(int Year, int Month, int WeekNumber)>();
			var currentDate = startDate;

			while (currentDate <= endDate)
			{
				periods.Add((
					currentDate.Year,
					currentDate.Month,
					GetWeekOfMonth(currentDate)
				));
				currentDate = currentDate.AddDays(1);
			}

			return periods.OrderBy(p => p.Year)
						  .ThenBy(p => p.Month)
						  .ThenBy(p => p.WeekNumber)
						  .ToList();
		}

		public async Task<List<UserHistory>> GetActivityLogsAsync(
	DateTime? startDateTime,
	DateTime? endDateTime,
	List<string>? entityType,
	List<string>? actionType,
	List<int>? userId)
		{
			var query = _tenantContext.UserHistory.AsQueryable();

			// Filter by date range
			if (startDateTime.HasValue)
				query = query.Where(u => u.Timestamp >= startDateTime.Value);

			if (endDateTime.HasValue)
				query = query.Where(u => u.Timestamp <= endDateTime.Value);

			// Filter by entity types (if provided)
			if (entityType != null && entityType.Any())
				query = query.Where(u => entityType.Contains(u.EntityType));

			// Filter by action types (if provided)
			if (actionType != null && actionType.Any())
				query = query.Where(u => actionType.Contains(u.ActionType));

			// Filter by user IDs (if provided)
			if (userId != null && userId.Any())
				query = query.Where(u => userId.Contains(u.UserId));

			// Get results ordered by latest first
			var result = await query
				.OrderByDescending(u => u.Timestamp)
				.ToListAsync();

			return result;
		}
		public async Task<User> GetUserByIdAsync(int userId)
		{
			return await _tenantContext.Users.FindAsync(userId);
		}
		public async Task<string> GetUserRoleNameAsync(int userRoleId)
		{
			var userRole = await _platformContext.UserRole.FindAsync(userRoleId);
			return userRole?.UserRoleName ?? "Unknown";
		}
		public async Task<List<UserDto>> GetAllUsersAsync()
		{
			return await _tenantContext.Users
				.Where(u => u.IsActive)
				.OrderBy(u => u.FullName)
				.Select(u => new UserDto
				{
					UserID = u.UserID,
					FullName = u.FullName
				})
				.ToListAsync();
		}
		public async Task<List<SupplierRequestDetailDto>> GetRequestBySupplierAsync(int supplierId, DateOnly startDate, DateOnly endDate)
		{

			var requests = await (
			from request in _tenantContext.RequestMealConsumption
			join mealType in _tenantContext.MealType
				on request.MealTypeId equals mealType.MealTypeId
			join subType in _tenantContext.MealSubType
				on request.SubTypeId equals subType.MealSubTypeId into subTypeJoin
			from subType in subTypeJoin.DefaultIfEmpty() // Left join (subtype optional)
			where request.SupplierId == supplierId
				  && request.EventDate >= startDate
				  && request.EventDate <= endDate
			select new SupplierRequestDetailDto
			{
				EventDate = request.EventDate,
				EventType = request.EventType,
				Description = request.EventDescription,
				MealType = mealType.TypeName,
				SubMealType = subType != null ? subType.SubTypeName : null,
				Quantity = request.Quantity,
				SellingPrice = request.TotalSupplierCost
			}
			).ToListAsync();

			return requests;
		}

		public async Task<SupplierRequestCosts> GetSupplierRequestCostDetailsAsync(int supplierId, DateOnly startDate, DateOnly endDate)
		{
			var data = await _tenantContext.RequestMealConsumption
				.Where(x => x.SupplierId == supplierId &&
							x.EventDate >= startDate &&
							x.EventDate <= endDate)
				.ToListAsync();

			var summary = new SupplierRequestCosts
			{
				TotalEmployeeContribution = data.Sum(x => x.TotalEmployeeContribution),
				TotalCompanyContribution = data.Sum(x => x.TotalCompanyContribution),
				TotalSupplierCost = data.Sum(x => x.TotalSupplierCost),
				TotalSellingPrice = data.Sum(x => x.TotalSellingPrice)
			};

			return summary;
		}
		public async Task<int> GetTotalRequestMealssAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
		{
			
			var requestMealCount = await _tenantContext.RequestMealConsumption
				.Where(rmc => rmc.EventDate >= startDate &&
							  rmc.EventDate <= endDate)
				.SumAsync(rmc => rmc.Quantity);

			var totalMeals = requestMealCount;
			return totalMeals;
		}
		public async Task<decimal> GetTotalRequestsCostAsync(DateOnly startDate, DateOnly endDate, List<int> personIds = null)
		{
			
			var requestCost = await _tenantContext.RequestMealConsumption
				.Where(rmc => rmc.EventDate >= startDate &&
							  rmc.EventDate <= endDate)
				.SumAsync(rmc => rmc.TotalSupplierCost);
			return requestCost;
		}
		public async Task<string> GetPersonNumberByIdsync(int personId)
		{
			var person = await _tenantContext.Person.FindAsync(personId);
			return person?.PersonNumber ?? "Unknown";
		}

		public async Task<List<GraphDataPoint>> GetAggregatedRequestConsumptionDataAsync(DateOnly startDate,DateOnly endDate,GroupingLevel grouping,TimePeriod timePeriod)
		{
			// Base query with filters
			var current = DateOnly.FromDateTime(DateTime.Now);

			var query = _tenantContext.RequestMealConsumption
				.Where(rmc =>
					rmc.EventDate >= startDate &&
					rmc.EventDate <= endDate &&
					rmc.EventDate <= current);

			var results = new List<GraphDataPoint>();

			switch (grouping)
			{
				case GroupingLevel.ByDay:
					var dailyData = await query
						.GroupBy(rmc => rmc.EventDate)
						.Select(g => new {
							Date = g.Key,
							MealCount = g.Sum(x => x.Quantity), // ✅ Sum of quantities
							TotalCost = g.Sum(rmc => rmc.TotalSupplierCost)
						})
						.ToListAsync();

					var dailyDataDict = dailyData.ToDictionary(d => d.Date);
					var currentDate = startDate;

					while (currentDate <= endDate)
					{
						var hasData = dailyDataDict.TryGetValue(currentDate, out var data);

						results.Add(new GraphDataPoint
						{
							Label = (timePeriod == TimePeriod.ThisWeek || timePeriod == TimePeriod.LastWeek)
									? currentDate.ToString("ddd") // "Mon", "Tue"
									: currentDate.ToString("MMM d"), // "Oct 29"
							MealCount = hasData ? data.MealCount : 0,
							TotalCost = hasData ? data.TotalCost : 0
						});

						currentDate = currentDate.AddDays(1);
					}
					break;

				case GroupingLevel.ByWeek:
					// Fetch raw data
					var rawDataForWeek = await query
						.Select(rmc => new { rmc.EventDate, rmc.Quantity, rmc.TotalSupplierCost })
						.ToListAsync();

					var weeklyData = rawDataForWeek
						.GroupBy(rmc => new {
							Year = rmc.EventDate.Year,
							Month = rmc.EventDate.Month,
							Week = GetWeekOfMonth(rmc.EventDate)
						})
						.Select(g => new {
							Year = g.Key.Year,
							Month = g.Key.Month,
							WeekNumber = g.Key.Week,
							MealCount = g.Sum(x => x.Quantity), // ✅ Sum of quantities
							TotalCost = g.Sum(rmc => rmc.TotalSupplierCost)
						})
						.ToList();

					var weeklyDataDict = weeklyData.ToDictionary(
						w => (w.Year, w.Month, w.WeekNumber),
						w => w
					);

					var weekPeriods = GetAllWeekPeriodsInRange(startDate, endDate);

					foreach (var period in weekPeriods)
					{
						var hasData = weeklyDataDict.TryGetValue(
							(period.Year, period.Month, period.WeekNumber),
							out var data
						);

						results.Add(new GraphDataPoint
						{
							Label = $"Week {period.WeekNumber}",
							MealCount = hasData ? data.MealCount : 0,
							TotalCost = hasData ? data.TotalCost : 0
						});
					}
					break;

				case GroupingLevel.ByMonth:
					// Fetch raw data
					var rawDataForMonth = await query
						.Select(rmc => new { rmc.EventDate, rmc.Quantity, rmc.TotalSupplierCost })
						.ToListAsync();

					var monthlyData = rawDataForMonth
						.GroupBy(rmc => new { rmc.EventDate.Year, rmc.EventDate.Month })
						.Select(g => new {
							Year = g.Key.Year,
							Month = g.Key.Month,
							MealCount = g.Sum(x => x.Quantity), // ✅ Sum of quantities
							TotalCost = g.Sum(rmc => rmc.TotalSupplierCost)
						})
						.ToList();

					var monthlyDataDict = monthlyData.ToDictionary(
						m => (m.Year, m.Month),
						m => m
					);

					var currentMonth = new DateOnly(startDate.Year, startDate.Month, 1);
					var endMonth = new DateOnly(endDate.Year, endDate.Month, 1);

					while (currentMonth <= endMonth)
					{
						var hasData = monthlyDataDict.TryGetValue(
							(currentMonth.Year, currentMonth.Month),
							out var data
						);

						results.Add(new GraphDataPoint
						{
							Label = (timePeriod == TimePeriod.ThisYear || timePeriod == TimePeriod.LastYear)
									? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth.Month)
									: $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(currentMonth.Month)} {currentMonth.Year}",
							MealCount = hasData ? data.MealCount : 0,
							TotalCost = hasData ? data.TotalCost : 0
						});

						currentMonth = currentMonth.AddMonths(1);
					}
					break;
			}

			return results;
		}
		public async Task<List<MealTypeDistributionDto>> GetRequestMealDistributionByTypeAsync(DateOnly startDate, DateOnly endDate)
		{
			var currentDate = DateOnly.FromDateTime(DateTime.Now);

			var query = _tenantContext.RequestMealConsumption
				.Where(rmc =>
					rmc.EventDate >= startDate &&
					rmc.EventDate <= endDate &&
					rmc.EventDate <= currentDate);

			var distribution = await query
				.GroupBy(m => m.MealTypeId)
				.Select(g => new
				{
					MealTypeId = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			// Get MealType names
			var mealTypeIds = distribution.Select(d => d.MealTypeId).ToList();
			var mealTypes = await _tenantContext.MealType
				.Where(mt => mealTypeIds.Contains(mt.MealTypeId))
				.ToDictionaryAsync(mt => mt.MealTypeId, mt => mt.TypeName);

			var totalCount = distribution.Sum(x => x.Count);

			var result = distribution
				.Select(d => new MealTypeDistributionDto
				{
					MealType = mealTypes.ContainsKey(d.MealTypeId) ? mealTypes[d.MealTypeId] : "Unknown",
					Count = d.Count,
					Percentage = totalCount > 0 ? (decimal)d.Count / totalCount * 100 : 0
				})
				.OrderByDescending(x => x.Count)
				.ToList();

			return result;
		}


	}

}
