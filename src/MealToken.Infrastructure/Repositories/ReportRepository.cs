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
		
	}
}
