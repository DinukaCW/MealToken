using Authentication.Interfaces;
using Authentication.Models.DTOs;
using MealToken.Application.Interfaces;
using MealToken.Domain.Enums;
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
        private readonly ILogger<TokenProcessService> _logger;
        private readonly ITenantContext _tenantContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReportRepository _reportRepository;
        private readonly IBusinessRepository _businessData;

        public MealReportService(
            IEncryptionService encryptionService,
            ILogger<TokenProcessService> logger,
            ITenantContext tenantContext,
            IAdminRepository adminData,
            IHttpContextAccessor httpContextAccessor,
            IReportRepository reportRepository,
            IBusinessRepository businessData)
        {
            _encryption = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
            _businessData = businessData ?? throw new ArgumentNullException(nameof(businessData));
        }

        public async Task<ServiceResult<ReportDashBoard>> GetDashboardSummaryAsync()
        {
            try
            {
                // FIXED: Run operations sequentially to avoid DbContext threading issues
                var mealsThisMonth = await _reportRepository.GetMealsServedThisMonthAsync();
                var mealsLastMonth = await _reportRepository.GetMealsServedLastMonthAsync();
                var activeEmployees = await _reportRepository.GetActiveEmployeesCountAsync();
                var activeVisitors = await _reportRepository.GetActiveVisitorsCountAsync();
                var pendingRequests = await _reportRepository.GetPendingRequestsCountAsync();

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
                var mealConsumptions = await _reportRepository.GetMealConsumptioninWeekAsync(startDate, endDate);

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

		public async Task<ServiceResult> GetMealConsumptionSummaryAsync(DateOnly startDate, DateOnly? endDate = null)
		{
			_logger.LogInformation("Meal consumption summary request started. StartDate: {StartDate}, EndDate: {EndDate}", startDate, endDate);

			try
			{
				// If endDate is not provided, use startDate (single day report)
				var reportEndDate = endDate ?? startDate;

				// Validate date range
				if (reportEndDate < startDate)
				{
					_logger.LogWarning("Invalid date range detected. StartDate: {StartDate}, EndDate: {EndDate}", startDate, reportEndDate);
					return new ServiceResult
					{
						Success = false,
						Message = "End date cannot be before start date."
					};
				}

				_logger.LogInformation("Fetching meal consumption data between {StartDate} and {EndDate}", startDate, reportEndDate);

				// Get all data for the requested range
				var allData = await _reportRepository.GetMealConsumptionSummaryByDateRangeAsync(startDate, reportEndDate);

				if (allData == null || !allData.Any())
				{
					_logger.LogInformation("No meal consumption records found for the given period. StartDate: {StartDate}, EndDate: {EndDate}", startDate, reportEndDate);

					return new ServiceResult
					{
						Success = true,
						Message = "No meal consumption records found for the selected date range.",
						Data = new List<MealConsumptionSummaryDto>()
					};
				}

				// Group by date to prepare summary list
				var summaries = allData
					.GroupBy(x => x.Date)
					.Select(g => new MealConsumptionSummaryDto
					{
						MealConsumptionDetails = g.ToList(),
						TotalMealServed = g.Sum(x => x.TotalMealCount),
						TotalEmployeesContribution = g.Sum(x => x.TotalEmployeeContribution),
						TotalSupplierContribution = g.Sum(x => x.TotalSupplierCost),
						TotalCompanyContribution = g.Sum(x => x.TotalEmployerContribution)
					})
					.OrderBy(x => x.MealConsumptionDetails.First().Date)
					.ToList();

				_logger.LogInformation("Meal consumption summary generated successfully for {DaysCount} day(s).", summaries.Count);

				return new ServiceResult
				{
					Success = true,
					Message = "Meal consumption summary generated successfully.",
					Data = summaries
				};
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning(ex, "Invalid input parameters: {Message}", ex.Message);
				return new ServiceResult
				{
					Success = false,
					Message = $"Invalid input: {ex.Message}"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An unexpected error occurred while generating meal consumption summary.");
				return new ServiceResult
				{
					Success = false,
					Message = "An unexpected error occurred while processing the meal consumption summary. Please try again later."
				};
			}
		}

		public async Task<SupplierPaymentReportDto> GetSupplierPaymentReportAsync(int supplierId,DateOnly startDate,DateOnly? endDate = null)
		{
			var reportEndDate = endDate ?? startDate;

			if (reportEndDate < startDate)
			{
				throw new ArgumentException("End date cannot be before start date.");
			}

			// Get supplier info
			var supplierInfo = await _reportRepository.GetSupplierInfoAsync(supplierId);
			if (supplierInfo == null)
			{
				throw new KeyNotFoundException($"Supplier with ID {supplierId} not found. or supplier is not Active");
			}
     
			// Get meal details
			var mealDetails = await _reportRepository.GetSupplierMealDetailsByDateRangeAsync(
				supplierId,
				startDate,
				reportEndDate
			);

			// Get summary
			var summary = await _reportRepository.GetSupplierSummaryByDateRangeAsync(
				supplierId,
				startDate,
				reportEndDate
			);

			return new SupplierPaymentReportDto
			{
				SupplierName = supplierInfo.SupplierName,
				ContactNumber = _encryption.DecryptData(supplierInfo.ContactNumber),
				Address = _encryption.DecryptData(supplierInfo.Address),
				MealDetails = mealDetails,
				Summary = summary
			};
		}
		public async Task<ServiceResult> GetAllSuppliersPaymentReportAsync(DateOnly startDate, DateOnly? endDate = null)
		{

			try
			{
				// If endDate is not provided, use startDate (single day report)
				var reportEndDate = endDate ?? startDate;

				// Validate date range
				if (reportEndDate < startDate)
				{
					_logger.LogWarning("Invalid date range detected. StartDate: {StartDate}, EndDate: {EndDate}", startDate, reportEndDate);
					return new ServiceResult
					{
						Success = false,
						Message = "End date cannot be before start date."
					};
				}

				_logger.LogInformation("Fetching active suppliers between {StartDate} and {EndDate}", startDate, reportEndDate);

				// Get all active suppliers
				var supplierIds = await _reportRepository.GetActiveSupplierIdsByDateRangeAsync(startDate, reportEndDate);

				if (supplierIds == null || !supplierIds.Any())
				{
					_logger.LogInformation("No active suppliers found in the specified date range. StartDate: {StartDate}, EndDate: {EndDate}", startDate, reportEndDate);
					return new ServiceResult
					{
						Success = true,
						Message = "No active suppliers found for the selected date range.",
						Data = new List<SupplierPaymentReportDto>()
					};
				}

				var reports = new List<SupplierPaymentReportDto>();

				foreach (var supplierId in supplierIds)
				{
					try
					{
						var report = await GetSupplierPaymentReportAsync(supplierId, startDate, reportEndDate);

						if (report != null)
						{
							reports.Add(report);
						}
						else
						{
							_logger.LogWarning("Report generation returned null for SupplierId: {SupplierId}", supplierId);
						}
					}
					catch (Exception innerEx)
					{
						_logger.LogError(innerEx, "Error while generating report for SupplierId: {SupplierId}", supplierId);
					}
				}

				_logger.LogInformation("Supplier payment report generation completed successfully. Total Reports: {Count}", reports.Count);

				return new ServiceResult
				{
					Success = true,
					Message = "Supplier payment report generated successfully.",
					Data = reports
				};
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning(ex, "Invalid input parameters for supplier payment report: {Message}", ex.Message);
				return new ServiceResult
				{
					Success = false,
					Message = $"Invalid input: {ex.Message}"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An unexpected error occurred while generating supplier payment reports.");
				return new ServiceResult
				{
					Success = false,
					Message = "An unexpected error occurred while generating supplier payment reports. Please try again later."
				};
			}
		}

		public async Task<ServiceResult> GetTodayMealSchedulesAsync(DateOnly date, TimeOnly time)
		{

			try
			{
				// Get today's schedule IDs
				var scheduleIds = await _businessData.GetScheduleByDateAsync(date);

				if (scheduleIds == null || !scheduleIds.Any())
				{
					_logger.LogInformation("No schedules found for date {Date}.", date);
					return new ServiceResult
					{
						Success = true,
						Message = "No schedules found for the selected date.",
						Data = new List<TodayScheduleDto>()
					};
				}

				_logger.LogInformation("Retrieved {Count} schedule IDs for date {Date}.", scheduleIds.Count(), date);

				// Get meals by schedule IDs
				var meals = await _businessData.GetMealsByScheduleIdsAsync(scheduleIds);

				if (meals == null || !meals.Any())
				{
					_logger.LogInformation("No meals found for the given schedule IDs on date {Date}.", date);
					return new ServiceResult
					{
						Success = true,
						Message = "No meals available for the selected date.",
						Data = new List<TodayScheduleDto>()
					};
				}

				// Determine meal status based on current time
				foreach (var meal in meals)
				{
					if (time >= meal.StartTime && time <= meal.EndTime)
					{
						meal.Status = MealStatus.Active;
					}
					else if (time < meal.StartTime)
					{
						meal.Status = MealStatus.Upcoming;
					}
					else
					{
						meal.Status = MealStatus.Completed;
					}
				}

				var sortedMeals = meals
					.OrderBy(m => m.StartTime)
					.ThenBy(m => m.ScheduleName)
					.ToList();

				_logger.LogInformation("Today's meal schedules fetched successfully. Total meals: {Count}", sortedMeals.Count);

				return new ServiceResult
				{
					Success = true,
					Message = "Today's meal schedules retrieved successfully.",
					Data = sortedMeals
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while fetching today's meal schedules for {Date}.", date);
				return new ServiceResult
				{
					Success = false,
					Message = "An unexpected error occurred while fetching today's meal schedules. Please try again later."
				};
			}
		}



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
