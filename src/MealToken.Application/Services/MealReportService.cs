using Authentication.Interfaces;
using Authentication.Models.DTOs;
using MealToken.Application.Interfaces;
using MealToken.Domain.Interfaces;
using MealToken.Domain.Models;
using MealToken.Domain.Models.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealToken.Application.Services
{
    public class MealReportService : IMealReportService
    {
        private readonly IEncryptionService _encryption;
        private readonly IBusinessRepository _businessData;
        private readonly ILogger<TokenProcessService> _logger;
        private readonly ITenantContext _tenantContext;
        private readonly IAdminRepository _adminData;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MealReportService(
            IEncryptionService encryptionService,
            IBusinessRepository businessRepository,
            ILogger<TokenProcessService> logger,
            ITenantContext tenantContext,
            IAdminRepository adminData,
            IHttpContextAccessor httpContextAccessor)
        {
            _encryption = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _businessData = businessRepository ?? throw new ArgumentNullException(nameof(businessRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _adminData = adminData ?? throw new ArgumentNullException(nameof(adminData));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<ServiceResult<ReportDashBoard>> GetDashboardSummaryAsync()
        {
            try
            {
                // FIXED: Run operations sequentially to avoid DbContext threading issues
                var mealsThisMonth = await _businessData.GetMealsServedThisMonthAsync();
                var mealsLastMonth = await _businessData.GetMealsServedLastMonthAsync();
                var activeEmployees = await _businessData.GetActiveEmployeesCountAsync();
                var activeVisitors = await _businessData.GetActiveVisitorsCountAsync();
                var pendingRequests = await _businessData.GetPendingRequestsCountAsync();

                // Calculate percentage change
                var (percentageChange, isIncrease) = CalculatePercentageChange(mealsThisMonth, mealsLastMonth);

                var dashboard = new ReportDashBoard
                {
                    MealsServedThisMonth = new MealsServedCard
                    {
                        TotalMeals = mealsThisMonth,
                        PercentageChange = Math.Round(percentageChange, 1),
                        IsIncrease = isIncrease
                    },
                    ActiveEmployees = new ActiveEmployeesCard
                    {
                        TotalActiveEmployees = activeEmployees,
                        Status = "Registered in system"
                    },
                    ActiveVisitors = new ActiveVisitorsCard
                    {
                        TotalActiveVisitors = activeVisitors,
                        Status = "Registered in system"
                    },
                    PendingRequests = new PendingRequestsCard
                    {
                        TotalPendingRequests = pendingRequests,
                        Status = "Awaiting approval"
                    }
                };

                return ServiceResult<ReportDashBoard>.SuccessResult(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard summary");
                return ServiceResult<ReportDashBoard>.FailureResult(
                    $"Error generating dashboard summary: {ex.Message}");
            }
        }

        public async Task<ServiceResult<MealConsumptionReportDTO>> GenerateWeeklyReportAsync(
            DateOnly startDate, DateOnly endDate)
        {
            try
            {
                // Fetch data for the date range with all related data in ONE query
                var mealConsumptions = await _businessData.GetMealConsumptioninWeekAsync(startDate, endDate);

                if (!mealConsumptions.Any())
                {
                    return ServiceResult<MealConsumptionReportDTO>.SuccessResult(
                        new MealConsumptionReportDTO
                        {
                            StartDate = startDate,
                            EndDate = endDate,
                            DailyReports = new List<DailyMealReport>()
                        });
                }

                var report = BuildWeeklyReport(mealConsumptions, startDate, endDate);

                return ServiceResult<MealConsumptionReportDTO>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weekly report for date range {StartDate} to {EndDate}",
                    startDate, endDate);
                return ServiceResult<MealConsumptionReportDTO>.FailureResult(
                    $"Error generating weekly report: {ex.Message}");
            }
        }

        public async Task<ServiceResult<MealConsumptionReportDTO>> GenerateCurrentWeekReportAsync()
        {
            var (startOfWeek, endOfWeek) = GetCurrentWeekRange();
            return await GenerateWeeklyReportAsync(startOfWeek, endOfWeek);
        }

		public async Task<ServiceResult<List<MealConsumptionSummaryDto>>> GenerateMealConsumptionSummaryReportAsync(
			DateOnly startDate, DateOnly endDate)
		{
			try
			{
				// Use existing method to get weekly report data
				var reportResult = await GenerateWeeklyReportAsync(startDate, endDate);

				if (!reportResult.Success)
				{
					return ServiceResult<List<MealConsumptionSummaryDto>>.FailureResult(
						reportResult.Message, reportResult.Errors);
				}

				var mealConsumptionReport = reportResult.Data;
				var summaryData = BuildSummaryReportData(mealConsumptionReport);

				return ServiceResult<List<MealConsumptionSummaryDto>>.SuccessResult(summaryData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Error generating meal consumption summary report for date range {StartDate} to {EndDate}",
					startDate, endDate);
				return ServiceResult<List<MealConsumptionSummaryDto>>.FailureResult(
					$"Error generating meal consumption summary report: {ex.Message}");
			}
		}

		/// <summary>
		/// Builds summary report data from meal consumption report
		/// </summary>
		private List<MealConsumptionSummaryDto> BuildSummaryReportData(MealConsumptionReportDTO report)
		{
			var summaryList = new List<MealConsumptionSummaryDto>();

			foreach (var dailyReport in report.DailyReports)
			{
				foreach (var mealTypeGroup in dailyReport.MealTypeGroups)
				{
					// Add subtotal rows for each subtype
					foreach (var subTypeTotal in mealTypeGroup.SubTypeSubTotals)
					{
						var summary = new MealConsumptionSummaryDto
						{
							Date = dailyReport.Date,
							MealType = mealTypeGroup.MealTypeName,
							SubType = subTypeTotal.SubTypeName,
							EmployeeCount = subTypeTotal.EmployeeCount,
							MaleCount = subTypeTotal.MaleCount,
							FemaleCount = subTypeTotal.FemaleCount,
							TotalEmployeeContribution = subTypeTotal.TotalEmployeeContribution,
							TotalEmployerContribution = subTypeTotal.TotalEmployerCost,
							TotalSupplierCost = subTypeTotal.TotalSupplierCost,
							TotalMealCount = subTypeTotal.TotalMealCount,
							RowType = "SubtypeTotal"
						};
						summaryList.Add(summary);
					}

					// Add meal type total row
					var mealTypeTotal = new MealConsumptionSummaryDto
					{
						Date = dailyReport.Date,
						MealType = mealTypeGroup.MealTypeName,
						SubType = "Total",
						EmployeeCount = mealTypeGroup.MealTypeTotal.EmployeeCount,
						MaleCount = mealTypeGroup.MealTypeTotal.MaleCount,
						FemaleCount = mealTypeGroup.MealTypeTotal.FemaleCount,
						TotalEmployeeContribution = mealTypeGroup.MealTypeTotal.TotalEmployeeContribution,
						TotalEmployerContribution = mealTypeGroup.MealTypeTotal.TotalEmployerCost,
						TotalSupplierCost = mealTypeGroup.MealTypeTotal.TotalSupplierCost,
						TotalMealCount = mealTypeGroup.MealTypeTotal.TotalMealCount,
						RowType = "MealTypeTotal"
					};
					summaryList.Add(mealTypeTotal);
				}

				// Add daily total row
				var dailyTotal = new MealConsumptionSummaryDto
				{
					Date = dailyReport.Date,
					MealType = "Daily Total",
					SubType = "",
					EmployeeCount = 0,
					MaleCount = 0,
					FemaleCount = 0,
					TotalEmployeeContribution = dailyReport.DailyTotal.GrandTotalEmployeeContribution,
					TotalEmployerContribution = dailyReport.DailyTotal.GrandTotalEmployerCost,
					TotalSupplierCost = dailyReport.DailyTotal.GrandTotalSupplierCost,
					TotalMealCount = dailyReport.DailyTotal.GrandTotalMealCount,
					RowType = "DailyTotal"
				};
				summaryList.Add(dailyTotal);
			}

			return summaryList;
		}

		#region Private Helper Methods

		private static (decimal percentageChange, bool isIncrease) CalculatePercentageChange(
            int currentValue, int previousValue)
        {
            if (previousValue > 0)
            {
                var change = ((decimal)(currentValue - previousValue) / previousValue) * 100;
                return (Math.Abs(change), change > 0);
            }

            if (currentValue > 0)
            {
                return (100m, true);
            }

            return (0m, false);
        }

        private static (DateOnly startOfWeek, DateOnly endOfWeek) GetCurrentWeekRange()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);
            return (startOfWeek, endOfWeek);
        }

        private MealConsumptionReportDTO BuildWeeklyReport(
            IEnumerable<MealConsumptionWithDetails> mealConsumptions,
            DateOnly startDate,
            DateOnly endDate)
        {
            var report = new MealConsumptionReportDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                DailyReports = new List<DailyMealReport>()
            };

            var groupedByDate = mealConsumptions.GroupBy(m => m.Date);

            foreach (var dateGroup in groupedByDate)
            {
                var dailyReport = BuildDailyReport(dateGroup);
                report.DailyReports.Add(dailyReport);
            }

            return report;
        }

        private DailyMealReport BuildDailyReport(IGrouping<DateOnly, MealConsumptionWithDetails> dateGroup)
        {
            var dailyReport = new DailyMealReport
            {
                Date = dateGroup.Key,
                MealTypeGroups = new List<MealTypeGroup>()
            };

            var groupedByMealType = dateGroup.GroupBy(m => m.MealTypeName);

            foreach (var mealTypeGroup in groupedByMealType)
            {
                var mealTypeData = BuildMealTypeGroup(mealTypeGroup);
                dailyReport.MealTypeGroups.Add(mealTypeData);
            }

            dailyReport.DailyTotal = CalculateDailyTotal(dailyReport.MealTypeGroups);

            return dailyReport;
        }

        private MealTypeGroup BuildMealTypeGroup(IGrouping<string, MealConsumptionWithDetails> mealTypeGroup)
        {
            var mealTypeData = new MealTypeGroup
            {
                MealTypeName = mealTypeGroup.Key,
                Details = new List<MealConsumptionDetail>(),
                SubTypeSubTotals = new List<SubTypeSubTotal>()
            };

            // Add individual consumption details - NO DATABASE CALLS
            foreach (var consumption in mealTypeGroup)
            {
                mealTypeData.Details.Add(new MealConsumptionDetail
                {
                    EmployeeNumber = consumption.PersonId,
                    Name = consumption.PersonName,
                    Department = consumption.DepartmentName ?? "N/A",
                    Designation = consumption.DesignationName ?? "N/A",
                    Gender = consumption.Gender ?? "N/A",
                    Subtype = consumption.SubTypeName ?? "",
                    EmployeeContribution = consumption.EmployeeCost,
                    EmployerContribution = consumption.CompanyCost,
                    TotalSupplierCost = consumption.SupplierCost
                });
            }

            // Calculate subtotals by subtype
            var groupedBySubType = mealTypeGroup
                .Where(m => !string.IsNullOrEmpty(m.SubTypeName))
                .GroupBy(m => m.SubTypeName);

            foreach (var subTypeGroup in groupedBySubType)
            {
                var subTotal = CalculateSubTypeTotal(subTypeGroup);
                mealTypeData.SubTypeSubTotals.Add(subTotal);
            }

            // Calculate meal type total
            mealTypeData.MealTypeTotal = CalculateMealTypeTotal(mealTypeGroup);

            return mealTypeData;
        }

        private SubTypeSubTotal CalculateSubTypeTotal(IGrouping<string, MealConsumptionWithDetails> subTypeGroup)
        {
            var uniquePeople = subTypeGroup
              .GroupBy(m => m.PersonId)
              .Select(g => g.First())
              .ToList();

            // 2. Now perform all counts on this smaller, unique list.
            int maleCount = uniquePeople
                .Count(p => string.Equals(p.Gender, "Male", StringComparison.OrdinalIgnoreCase));

            int femaleCount = uniquePeople
                .Count(p => string.Equals(p.Gender, "Female", StringComparison.OrdinalIgnoreCase));
            return new SubTypeSubTotal
            {
                SubTypeName = subTypeGroup.Key,
                EmployeeCount = uniquePeople.Count,
                MaleCount = maleCount,
                FemaleCount =femaleCount,
                TotalEmployeeContribution = subTypeGroup.Sum(m => m.EmployeeCost),
                TotalEmployerCost = subTypeGroup.Sum(m => m.CompanyCost),
                TotalSupplierCost = subTypeGroup.Sum(m => m.SupplierCost),
                TotalMealCount = subTypeGroup.Count()
            };
        }

        private MealTypeSummary CalculateMealTypeTotal(IGrouping<string, MealConsumptionWithDetails> mealTypeGroup)
        {
            // 1. Get a list of unique people first.
            // We group by PersonId and take the first record for each person.
            var uniquePeople = mealTypeGroup
                .GroupBy(m => m.PersonId)
                .Select(g => g.First())
                .ToList();

            // 2. Now perform all counts on this smaller, unique list.
            int maleCount = uniquePeople
                .Count(p => string.Equals(p.Gender, "Male", StringComparison.OrdinalIgnoreCase));

            int femaleCount = uniquePeople
                .Count(p => string.Equals(p.Gender, "Female", StringComparison.OrdinalIgnoreCase));

            return new MealTypeSummary
            {
                // The total employee count is simply the count of our unique list.
                EmployeeCount = uniquePeople.Count,
                MaleCount = maleCount,
                FemaleCount = femaleCount,

                // Sums are still calculated from the original group, which contains all meal records.
                TotalEmployeeContribution = mealTypeGroup.Sum(m => m.EmployeeCost),
                TotalEmployerCost = mealTypeGroup.Sum(m => m.CompanyCost),
                TotalSupplierCost = mealTypeGroup.Sum(m => m.SupplierCost),
                TotalMealCount = mealTypeGroup.Count()
            };
        }

        private DailyTotalSummary CalculateDailyTotal(List<MealTypeGroup> mealTypeGroups)
        {
            return new DailyTotalSummary
            {
                TotalMealTypeCount = mealTypeGroups.Count,
                GrandTotalEmployeeContribution = mealTypeGroups.Sum(m => m.MealTypeTotal.TotalEmployeeContribution),
                GrandTotalEmployerCost = mealTypeGroups.Sum(m => m.MealTypeTotal.TotalEmployerCost),
                GrandTotalSupplierCost = mealTypeGroups.Sum(m => m.MealTypeTotal.TotalSupplierCost),
                GrandTotalMealCount = mealTypeGroups.Sum(m => m.MealTypeTotal.TotalMealCount)
            };
        }

        #endregion
    }

    #region Helper Interfaces

    public interface IGenderIdentifiable
    {
        int PersonId { get; }
        string Gender { get; }
    }

    #endregion

    #region Service Result

    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }

        public static ServiceResult<T> SuccessResult(T data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Errors = new List<string>()
            };
        }

        public static ServiceResult<T> FailureResult(string message, List<string> errors = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    #endregion
}
