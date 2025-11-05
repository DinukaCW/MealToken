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
		// This method assumes your DbContext is _context and has DbSet<UserHistory> and DbSet<User>
		public async Task<ServiceResult> GetActivityLogsAsync(ActivityLogFilter logFilter)
		{
			try
			{
				var activityLogs = await _reportRepository.GetActivityLogsAsync(
					logFilter.StartDateTime,
					logFilter.EndDateTime,
					logFilter.EntityTypes,
					logFilter.ActionTypes,
					logFilter.UserIds);

				if (activityLogs == null || !activityLogs.Any())
				{
					return new ServiceResult
					{
						Success = false,
						Message = "No activity logs found for the specified filters."
					};
				}

				var activityLogDtos = new List<ActivityLogDto>();

				foreach (var log in activityLogs)
				{
					// Get user details
					var user = await _reportRepository.GetUserByIdAsync(log.UserId);
					var roleName = user != null
						? await _reportRepository.GetUserRoleNameAsync(user.UserRoleId)
						: "Unknown Role";

					// Extract last part of the endpoint
					string endpointName = string.Empty;
					if (!string.IsNullOrEmpty(log.Endpoint))
					{
						endpointName = log.Endpoint.Trim('/');
						var parts = endpointName.Split('/');
						endpointName = parts.LastOrDefault() ?? string.Empty;
					}

					activityLogDtos.Add(new ActivityLogDto
					{
						Timestamp = log.Timestamp,
						UserName = user?.FullName ?? "Unknown User",
						UserRole = roleName,
						Action = log.ActionType,
						Entity = log.EntityType,
						Details = endpointName,
						IpAddress = log.IPAddress
					});
				}

				return new ServiceResult
				{
					Success = true,
					Message = "Activity logs retrieved successfully.",
					Data = activityLogDtos
				};
			}
			catch (Exception ex)
			{
				return new ServiceResult
				{
					Success = false,
					Message = $"An error occurred while retrieving activity logs: {ex.Message}"
				};
			}
		}

		public async Task<ServiceResult> GetActivityFilterDataAsync()
		{
			try
			{
				// Fetch all users from repository
				var users = await _reportRepository.GetAllUsersAsync();

				// Static entity types and action types for filtering
						var entityTypes = new List<string>
				{
					"User",
					"Schedule & Request",
					"Report",
					"Admin",
					"General"
				};

				var actionTypes = new List<string>
				{
					"Add",
					"Update",
					"View",
					"Delete",
					"Unknown"
				};

				// Prepare filter data object
				var filterData = new ActivityLogFilterData
				{
					Users = users,
					EntityTypes = entityTypes,
					ActionTypes = actionTypes
				};

				// Return successful service result
				return new ServiceResult
				{
					Success = true,
					Message = "Activity filter data retrieved successfully.",
					Data = filterData
				};
			}
			catch (Exception ex)
			{
				// Return error result if something fails
				return new ServiceResult
				{
					Success = false,
					Message = $"An error occurred while retrieving activity filter data: {ex.Message}"
				};
			}
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
			DateOnly startDate, DateOnly endDate,TimeOnly? startTime, TimeOnly? endTime)
		{
			try
			{
				// Fetch data for the date range with all related data in ONE query
				var mealConsumptions = await _reportRepository.GetMealConsumptioninWeekAsync(startDate, endDate,startTime,endTime);

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

				var report = await   BuildWeeklyReport(mealConsumptions, startDate, endDate);

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

		public async Task<ServiceResult<MealConsumptionReportDTO>> GenerateCurrentWeekReportAsync(TimeOnly? startTime, TimeOnly? endTime)
		{
			var (startOfWeek, endOfWeek) = GetCurrentWeekRange();
			return await GenerateWeeklyReportAsync(startOfWeek, endOfWeek,startTime,endTime);
		}

		public async Task<ServiceResult> GetMealConsumptionSummaryAsync(DateOnly startDate, DateOnly? endDate, TimeOnly? startTime, TimeOnly? endTime)
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
				var allData = await _reportRepository.GetMealConsumptionSummaryByDateRangeAsync(startDate, reportEndDate, startTime,endTime);

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
						TotalCompanyContribution = g.Sum(x => x.TotalCompanyContribution)
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

		public async Task<SupplierPaymentReportDto> GetSupplierPaymentReportAsync(int supplierId, DateOnly startDate, DateOnly? endDate = null)
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

			var requestDetails = await _reportRepository.GetRequestBySupplierAsync(
				supplierId,
				startDate,
				reportEndDate
			);
			var requestCosts = await _reportRepository.GetSupplierRequestCostDetailsAsync(
				supplierId,
				startDate,
				reportEndDate
			);
			summary.TotalEmployeeContribution += requestCosts.TotalEmployeeContribution;
			summary.TotalCompanyContribution += requestCosts.TotalCompanyContribution;
			summary.TotalSupplierCost += requestCosts.TotalSupplierCost;
			summary.TotalSellingPrice += requestCosts.TotalSellingPrice;

			return new SupplierPaymentReportDto
			{
				SupplierName = supplierInfo.SupplierName,
				ContactNumber = _encryption.DecryptData(supplierInfo.ContactNumber),
				Address = _encryption.DecryptData(supplierInfo.Address),
				MealDetails = mealDetails,
				RequestDetails = requestDetails,
				Summary = summary,
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

		private async Task<MealConsumptionReportDTO> BuildWeeklyReport(
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
				var dailyReport = await BuildDailyReport(dateGroup);
				report.DailyReports.Add(dailyReport);
			}

			return report;
		}

		private async Task<DailyMealReport> BuildDailyReport(IGrouping<DateOnly, MealConsumptionWithDetails> dateGroup)
		{
			var dailyReport = new DailyMealReport
			{
				Date = dateGroup.Key,
				MealTypeGroups = new List<MealTypeGroup>()
			};

			var groupedByMealType = dateGroup.GroupBy(m => m.MealTypeName);

			foreach (var mealTypeGroup in groupedByMealType)
			{
				var mealTypeData = await BuildMealTypeGroup(mealTypeGroup);
				dailyReport.MealTypeGroups.Add(mealTypeData);
			}

			dailyReport.DailyTotal = CalculateDailyTotal(dailyReport.MealTypeGroups);

			return dailyReport;
		}

		private async Task<MealTypeGroup> BuildMealTypeGroup(IGrouping<string, MealConsumptionWithDetails> mealTypeGroup)
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
					EmployeeNumber =  await _reportRepository.GetPersonNumberByIdsync(consumption.PersonId),
					Name = consumption.PersonName,
					Department = consumption.DepartmentName ?? "N/A",
					Designation = consumption.DesignationName ?? "N/A",
					Gender = consumption.Gender ?? "N/A",
					Subtype = consumption.SubTypeName ?? "",
					EmployeeContribution = consumption.EmployeeCost,
					CompanyContribution = consumption.CompanyCost,
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
				FemaleCount = femaleCount,
				TotalEmployeeContribution = subTypeGroup.Sum(m => m.EmployeeCost),
				TotalCompanyContribution = subTypeGroup.Sum(m => m.CompanyCost),
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
				TotalCompanyContribution = mealTypeGroup.Sum(m => m.CompanyCost),
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
				GrandTotalCompanyContribution = mealTypeGroups.Sum(m => m.MealTypeTotal.TotalCompanyContribution),
				GrandTotalSupplierCost = mealTypeGroups.Sum(m => m.MealTypeTotal.TotalSupplierCost),
				GrandTotalMealCount = mealTypeGroups.Sum(m => m.MealTypeTotal.TotalMealCount)
			};
		}


		public async Task<MealDashboardDto> GetMealDashboardDataAsync(
		 TimePeriod timePeriod,
		 List<int> departmentIds,
		 DateOnly? customStartDate = null,
		 DateOnly? customEndDate = null)
		{
			try
			{
				var (startDate, endDate) = GetDateRange(timePeriod, customStartDate, customEndDate);
				var (previousStartDate, previousEndDate) = GetPreviousDateRange(timePeriod, startDate, endDate);

				List<int> personIds = await _reportRepository.GetPersonIdsByDepartmentsAsync(departmentIds);
				if (personIds == null || !personIds.Any())
				{
					_logger.LogWarning("No personIds found for departments {DepartmentIds}. Returning empty dashboard.",
						string.Join(",", departmentIds));
					return new MealDashboardDto(); 
				}

				var totalMeals= await _reportRepository.GetTotalMealsServedWithRequestsAsync(startDate, endDate, personIds);
				var totalCost = await _reportRepository.GetTotalCostAsync(startDate, endDate, personIds);
				var specialRequests = await _reportRepository.GetSpecialRequestsAsync(startDate, endDate, personIds);
				var mealDistribution = await _reportRepository.GetMealDistributionByTypeAsync(startDate, endDate, personIds);
				var mealCostDistribution = await GetMealConsumptionGraphData(timePeriod, personIds, startDate, endDate);

				var previousMeals = await _reportRepository.GetTotalMealsServedWithRequestsAsync(previousStartDate, previousEndDate, personIds);
				var previousCost= await _reportRepository.GetTotalCostAsync(previousStartDate, previousEndDate, personIds);

				var mealChange = previousMeals > 0 ? ((decimal)(totalMeals - previousMeals) / previousMeals) * 100 : 0;
				var costChange = previousCost > 0 ? totalCost - previousCost : 0;

				return new MealDashboardDto
				{
					Metrics = new MealMetricsDto
					{
						TotalMealsServed = totalMeals,
						PercentageChange = Math.Abs(mealChange),
						IsIncrease = mealChange >= 0,

						TotalCost = totalCost,
						CostChange = Math.Abs(costChange),
						IsCostIncrease = costChange >= 0,

						TotalSpecialRequests = specialRequests.TotalRequests,
						ApprovedRequests = specialRequests.ApprovedRequests,
						PendingRequests = specialRequests.PendingRequests
					},
					MealCosts = new MealCostDistribution
					{
						DailyData = mealCostDistribution
					},
					MealTypes = new MealTypeDistribution
					{
						Distribution = mealDistribution
					}
				};
			}
			catch (Exception ex)
			{
				// --- 4. Logging & Re-throwing ---

				// Log the error with all the context needed for debugging
				_logger.LogError(ex,
					"An error occurred in {MethodName}." +
					"Parameters: TimePeriod={TimePeriod}, DepartmentIds={DepartmentIds}, " +
					"CustomStart={CustomStartDate}, CustomEnd={CustomEndDate}",
					nameof(GetMealDashboardDataAsync),
					timePeriod,
					string.Join(",", departmentIds ?? new List<int>()), // Don't log the list object itself
					customStartDate,
					customEndDate);

				// Re-throw the exception. This allows your global error handling
				// middleware to catch it and return a 500 Internal Server Error.
				// Do not "return null" or "return new()", as that hides the
				// error from the client.
				throw;
			}
		}
		public async Task<DashBoardDepartment> GetMealsByDepartmentAsync(
			TimePeriod timePeriod,
			List<int> departmentIds,
			DateOnly? customStartDate = null,
			DateOnly? customEndDate = null)
		{
			try
			{
				var (startDate, endDate) = GetDateRange(timePeriod, customStartDate, customEndDate);

				List<DepartmentPersonGroupDto> departmentGroups = await _reportRepository.GetPersonsGroupedByDepartmentAsync(departmentIds);

				if (departmentGroups == null || !departmentGroups.Any())
				{
					_logger.LogWarning("No person groups found for departments {DepartmentIds}. Returning empty.",
						string.Join(",", departmentIds));
					return new DashBoardDepartment { DepartmentWiseMeals = new List<DepartmentWiseMeal>() };
				}

				// --- Changed to run sequentially ---
				var departmentMeals = new List<DepartmentWiseMeal>();
				foreach (var dept in departmentGroups)
				{
					// Await each call one by one inside the loop
					var mealCount = await _reportRepository.GetTotalMealsServedAsync(startDate, endDate, dept.Persons);
					var mealCost = await _reportRepository.GetTotalCostAsync(startDate, endDate, dept.Persons);
					var employeeMealCount = await _reportRepository.GetTotalMealsServedAsync(startDate, endDate, dept.Employees);
					var visitorMealCount = await _reportRepository.GetTotalMealsServedAsync(startDate, endDate, dept.Visitors);
					var employeeMealCost = await _reportRepository.GetTotalCostAsync(startDate, endDate, dept.Employees);
					var visitorMealCost = await _reportRepository.GetTotalCostAsync(startDate, endDate, dept.Visitors);
					departmentMeals.Add(new DepartmentWiseMeal
					{
						DepartmentID = dept.DepartmentId,
						DepartmentName = dept.DepartmentName,
						MealCount = mealCount,
						MealCosts = mealCost,
						EmployeeMealCount = employeeMealCount,
						VisitorMealCount = visitorMealCount,
						EmployeeMealCosts = employeeMealCost,
						VisitorMealCosts = visitorMealCost
					});
				}
				// --- End of change ---

				// Calculate totals and percentages
				var totalMealCount = departmentMeals.Sum(d => d.MealCount);
				var totalMealCosts = departmentMeals.Sum(d => d.MealCosts);

				foreach (var deptMeal in departmentMeals)
				{
					if (totalMealCount > 0)
					{
						deptMeal.Precentage = (int)Math.Round(((double)deptMeal.MealCount / totalMealCount) * 100);
					}
					else
					{
						deptMeal.Precentage = 0;
					}
				}

				// Build the final DTO
				return new DashBoardDepartment
				{
					TotalMealCosts = totalMealCosts,
					TotalMealCount = totalMealCount,
					DepartmentWiseMeals = departmentMeals
											.OrderByDescending(d => d.MealCount)
											.ToList()
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"An error occurred in {MethodName}." +
					"Parameters: TimePeriod={TimePeriod}, DepartmentIds={DepartmentIds}, " +
					"CustomStart={CustomStartDate}, CustomEnd={CustomEndDate}",
					nameof(GetMealsByDepartmentAsync),
					timePeriod,
					string.Join(",", departmentIds ?? new List<int>()),
					customStartDate,
					customEndDate);

				throw;
			}
		}

		public async Task<DashboardSupplier> GetMealsBySupplierAsync(
			TimePeriod timePeriod,
			List<int> departmentIds,
			DateOnly? customStartDate = null,
			DateOnly? customEndDate = null)
		{
			try
			{
				var (startDate, endDate) = GetDateRange(timePeriod, customStartDate, customEndDate);

				List<int> personIds = await _reportRepository.GetPersonIdsByDepartmentsAsync(departmentIds);
				if (personIds == null || !personIds.Any())
				{
					_logger.LogWarning("No personIds found for departments {DepartmentIds}. Returning empty dashboard.",
						string.Join(",", departmentIds));
					return new DashboardSupplier { SupplierWiseMeals = new List<SupplierWiseMeals>() };
				}

				// Get total meal count
				var totalMeals = await _reportRepository.GetTotalMealsServedAsync(startDate, endDate, personIds);

				// Get total supplier cost
				var totalCost = await _reportRepository.GetTotalSupplierCostAsync(startDate, endDate, personIds);

				// Get the list of meals grouped by supplier
				var breakdown = await _reportRepository.GetSupplierBreakdownAsync(startDate, endDate, personIds);

				// --- FIXED: Assign the 'breakdown' result to 'supplierMeals' ---
				List<SupplierWiseMeals> supplierMeals = breakdown;

				// Calculate percentages
				foreach (var supplier in supplierMeals)
				{
					if (totalMeals > 0)
					{
						supplier.Precentage = Math.Round(((decimal)supplier.MealCount / totalMeals) * 100, 2);
					}
					else
					{
						supplier.Precentage = 0;
					}
				}

				// Return the final DTO
				return new DashboardSupplier
				{
					TotalMeals = totalMeals,
					TotalSupplierSellingPrice = totalCost,
					SupplierWiseMeals = supplierMeals.OrderByDescending(s => s.MealCount).ToList()
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"An error occurred in {MethodName}." +
					"Parameters: TimePeriod={TimePeriod}, DepartmentIds={DepartmentIds}, " +
					"CustomStart={CustomStartDate}, CustomEnd={CustomEndDate}",
					nameof(GetMealsBySupplierAsync),
					timePeriod,
					string.Join(",", departmentIds ?? new List<int>()),
					customStartDate,
					customEndDate);

				throw;
			}
		}

		public async Task<DashBoardMealType> GetMealsByMealTypeAsync(
			TimePeriod timePeriod,
			List<int> departmentIds,
			DateOnly? customStartDate = null,
			DateOnly? customEndDate = null)
		{
			try
			{
				var (startDate, endDate) = GetDateRange(timePeriod, customStartDate, customEndDate);

				List<int> personIds = await _reportRepository.GetPersonIdsByDepartmentsAsync(departmentIds);
				if (personIds == null || !personIds.Any())
				{
					_logger.LogWarning("No personIds found for departments {DepartmentIds}. Returning empty dashboard.",
						string.Join(",", departmentIds));

					// FIXED: Return the correct empty DTO
					return new DashBoardMealType { MealTypesWiseMeals = new List<MealTypeWiseMeal>() };
				}

				// 1. Get the raw data from the new repository method
				var rawData = await _reportRepository.GetMealTypeRawDataAsync(startDate, endDate, personIds);

				if (rawData == null || !rawData.Any())
				{
					return new DashBoardMealType { MealTypesWiseMeals = new List<MealTypeWiseMeal>() };
				}

				// 2. Get total meal count
				int totalMeals = rawData.Count;

				// 3. Process the raw data into the hierarchical DTO
				var mealTypeGroups = rawData
					.GroupBy(mc => new { mc.MealTypeId, mc.MealTypeName }) // Group by main meal type
					.Select(mealGroup => new MealTypeWiseMeal
					{
						MealTypeId = mealGroup.Key.MealTypeId,
						MealType = mealGroup.Key.MealTypeName,
						MealsCount = mealGroup.Count(), // Total meals for this type (incl. all subtypes)

						// Now, group the items *within* this mealGroup by subtype
						subTypeWiseMeals = mealGroup
							.Where(m => m.SubTypeId.HasValue) // Only include items that HAVE a subtype
							.GroupBy(sub => new {
								SubTypeId = sub.SubTypeId.Value,
								SubTypeName = sub.SubTypeName ?? "N/A" // Handle potential null name
							})
							.Select(subGroup => new SubTypeWiseMeal
							{
								SubTypeId = subGroup.Key.SubTypeId,
								SubType = subGroup.Key.SubTypeName,
								MealsCount = subGroup.Count()
							})
							.OrderByDescending(s => s.MealsCount)
							.ToList()
					})
					.OrderByDescending(m => m.MealsCount)
					.ToList();

				// 4. Return the final DTO
				return new DashBoardMealType
				{
					TotalMeals = totalMeals,
					MealTypesWiseMeals = mealTypeGroups
				};
			}
			catch (Exception ex)
			{
				// FIXED: Completed the log message
				_logger.LogError(ex,
					"An error occurred in {MethodName}." +
					"Parameters: TimePeriod={TimePeriod}, DepartmentIds={DepartmentIds}, " +
					"CustomStart={CustomStartDate}, CustomEnd={CustomEndDate}",
					nameof(GetMealsByMealTypeAsync), // Corrected method name
					timePeriod,
					string.Join(",", departmentIds ?? new List<int>()),
					customStartDate,
					customEndDate);

				throw; // Re-throw the exception
			}
		}

		public async Task<DashBoardCostAnalysis> GetMealsByCostAsync(
	TimePeriod timePeriod,
	List<int> departmentIds,
	DateOnly? customStartDate = null,
	DateOnly? customEndDate = null)
		{
			try
			{
				var (startDate, endDate) = GetDateRange(timePeriod, customStartDate, customEndDate);

				List<int> personIds = await _reportRepository.GetPersonIdsByDepartmentsAsync(departmentIds);
				if (personIds == null || !personIds.Any())
				{
					_logger.LogWarning("No personIds found for departments {DepartmentIds}. Returning empty dashboard.",
						string.Join(",", departmentIds));

					// FIXED: Return the correct empty DTO
					return new DashBoardCostAnalysis();
				}

				// Get Employee Cost (New repository method needed)
				var employeeCost = await _reportRepository.GetTotalEmployeeCostAsync(startDate, endDate, personIds);

				// Get Company Cost (Uses existing method)
				var companyCost = await _reportRepository.GetTotalCompanyCostAsync(startDate, endDate, personIds);
				var sellingPrice = await _reportRepository.GetTotalSellingPriceAsync(startDate, endDate, personIds);
				// Get Supplier Cost (Uses existing method)
				var supplierCost = await _reportRepository.GetTotalSupplierMealCostAsync(startDate, endDate, personIds);

				// --- Return the final DTO ---
				return new DashBoardCostAnalysis
				{
					EmployeesCost = employeeCost,
					CompanyCost = companyCost,
					SellingPrice = sellingPrice,
					SupplierCost = supplierCost
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"An error occurred in {MethodName}." +
					"Parameters: TimePeriod={TimePeriod}, DepartmentIds={DepartmentIds}, " +
					"CustomStart={CustomStartDate}, CustomEnd={CustomEndDate}",

					// FIXED: Corrected the method name in the log
					nameof(GetMealsByCostAsync),
					timePeriod,
					string.Join(",", departmentIds ?? new List<int>()),
					customStartDate,
					customEndDate);

				throw;
			}
		}

		public async Task<DashBoardPersonType> GetMealsByPersonTypeAsync(
	TimePeriod timePeriod,
	List<int> departmentIds,
	DateOnly? customStartDate = null,
	DateOnly? customEndDate = null)
		{
			try
			{
				var (startDate, endDate) = GetDateRange(timePeriod, customStartDate, customEndDate);

				List<int> allPersonIds = await _reportRepository.GetPersonIdsByDepartmentsAsync(departmentIds);
				if (allPersonIds == null || !allPersonIds.Any())
				{
					_logger.LogWarning("No personIds found for departments {DepartmentIds}. Returning empty dashboard.",
						string.Join(",", departmentIds));

					return new DashBoardPersonType();
				}

				var employeePersonIds = await _reportRepository.GetFilteredPersonIdsByTypeAsync(allPersonIds, PersonType.Employer);
				var visitorPersonIds = await _reportRepository.GetFilteredPersonIdsByTypeAsync(allPersonIds, PersonType.Visitor);

				int employeeMeals = 0;
				decimal employeeCost = 0;
				if (employeePersonIds.Any())
				{
					employeeMeals = await _reportRepository.GetTotalMealsServedAsync(startDate, endDate, employeePersonIds);
					employeeCost = await _reportRepository.GetTotalCostAsync(startDate, endDate, employeePersonIds);
				}

				int visitorMeals = 0;
				decimal visitorCost = 0;
				if (visitorPersonIds.Any())
				{
					visitorMeals = await _reportRepository.GetTotalMealsServedAsync(startDate, endDate, visitorPersonIds);
					visitorCost = await _reportRepository.GetTotalCostAsync(startDate, endDate, visitorPersonIds);
				}

				return new DashBoardPersonType
				{
					EmployeeCount = employeePersonIds.Count,
					EmployeeMeals = employeeMeals,
					EmployeeMealCost = employeeCost,
					VisitorCount = visitorPersonIds.Count,
					VisitorMeals = visitorMeals,
					VisitorMealCost = visitorCost
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"An error occurred in {MethodName}." +
					"Parameters: TimePeriod={TimePeriod}, DepartmentIds={DepartmentIds}, " +
					"CustomStart={CustomStartDate}, CustomEnd={CustomEndDate}",

					// FIXED: Corrected the method name
					nameof(GetMealsByPersonTypeAsync),
					timePeriod,
					string.Join(",", departmentIds ?? new List<int>()),
					customStartDate,
					customEndDate);

				throw;
			}
		}

		public async Task<DashBoardMealRequest> GetMealsInRequestsAsync(
	TimePeriod timePeriod,
	List<int> departmentIds,
	DateOnly? customStartDate = null,
	DateOnly? customEndDate = null)
		{
			try
			{
				_logger.LogInformation(
					"Starting {MethodName} with parameters: TimePeriod={TimePeriod}, CustomStart={CustomStartDate}, CustomEnd={CustomEndDate}",
					nameof(GetMealConsumptionGraphData),
					timePeriod,
					customStartDate,
					customEndDate);

				var (startDate, endDate) = GetDateRange(timePeriod, customStartDate, customEndDate);
				GroupingLevel grouping = DetermineGroupingLevel(timePeriod, startDate, endDate);

				// Fetch aggregated request consumption data
				var requestsCosts = await _reportRepository.GetAggregatedRequestConsumptionDataAsync(
					startDate, endDate, grouping, timePeriod);

				// Fetch meal request type distribution
				var requestMeals = await _reportRepository.GetRequestMealDistributionByTypeAsync(
					startDate, endDate);

				if (requestsCosts == null && (requestMeals == null || !requestMeals.Any()))
				{
					_logger.LogWarning(
						"No data found in {MethodName} for the given date range ({StartDate} to {EndDate}). Returning empty dashboard.",
						nameof(GetMealConsumptionGraphData),
						startDate,
						endDate);

					return new DashBoardMealRequest();
				}

				var result = new DashBoardMealRequest
				{
					MealRequestCostDetails = requestsCosts ?? new List<GraphDataPoint>(),
					RequestMealTypesDetails = requestMeals ?? new List<MealTypeDistributionDto>()
				};

				_logger.LogInformation(
					"Successfully completed {MethodName}. Retrieved {CostCount} cost records and {TypeCount} meal type records.",
					nameof(GetMealConsumptionGraphData),
					result.MealRequestCostDetails?.Count ?? 0,
					result.RequestMealTypesDetails?.Count ?? 0);

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"An error occurred in {MethodName}. Parameters: TimePeriod={TimePeriod}, CustomStart={CustomStartDate}, CustomEnd={CustomEndDate}",
					nameof(GetMealConsumptionGraphData),
					timePeriod,
					customStartDate,
					customEndDate);

				throw; // Re-throw after logging
			}
		}

		private async Task<List<GraphDataPoint>> GetMealConsumptionGraphData(
			TimePeriod timePeriod,
			List<int> personIds, 
			DateOnly startDate,
			DateOnly endDate)
		{
			
			GroupingLevel grouping = DetermineGroupingLevel(timePeriod, startDate, endDate);

			// 5. Call the repository to get the aggregated graph data
			return await _reportRepository.GetAggregatedConsumptionDataAsync(
				startDate,
				endDate,
				grouping,
				timePeriod, // Pass for correct label formatting
				personIds
			);
		}


		// Helper method to get date ranges based on time period
		/// <summary>
		/// Gets the start and end date range based on a selected TimePeriod.
		/// </summary>
		private (DateOnly startDate, DateOnly endDate) GetDateRange(
			TimePeriod timePeriod,
			DateOnly? customStartDate,
			DateOnly? customEndDate)
		{
			var today = DateOnly.FromDateTime(DateTime.Now);

			return timePeriod switch
			{
				TimePeriod.Today => (today, today),

				TimePeriod.Yesterday => (today.AddDays(-1), today.AddDays(-1)),

				TimePeriod.ThisWeek => (
					today.AddDays(-(int)today.DayOfWeek), // Assumes Sunday as first day
					today.AddDays(6 - (int)today.DayOfWeek)
				),

				TimePeriod.LastWeek => (
					today.AddDays(-(int)today.DayOfWeek - 7),
					today.AddDays(-(int)today.DayOfWeek - 1)
				),

				TimePeriod.ThisMonth => (
					new DateOnly(today.Year, today.Month, 1),
					new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month))
				),

				TimePeriod.LastMonth => (
					new DateOnly(today.Year, today.Month, 1).AddMonths(-1),
					new DateOnly(today.Year, today.Month, 1).AddDays(-1)
				),

				TimePeriod.ThisYear => (
					new DateOnly(today.Year, 1, 1),
					new DateOnly(today.Year, 12, 31)
				),

				TimePeriod.LastYear => (
					new DateOnly(today.Year - 1, 1, 1),
					new DateOnly(today.Year - 1, 12, 31)
				),

				TimePeriod.AllTime => (
					DateOnly.MinValue, // Represents the earliest possible date
					today
				),

				TimePeriod.CustomRange => (
					customStartDate ?? today, // Default to today if null
					customEndDate ?? today   // Default to today if null
				),

				_ => (today, today) // Default case
			};
		}

		// Helper method to get previous period for comparison
		/// <summary>
		/// Gets the previous date range for comparison.
		/// </summary>
		/// <remarks>
		/// Note: The signature is changed to require the timePeriod to handle
		/// variable-length periods (like months) correctly.
		/// </remarks>
		private (DateOnly startDate, DateOnly endDate) GetPreviousDateRange(
			TimePeriod timePeriod,
			DateOnly currentStartDate,
			DateOnly currentEndDate)
		{
			switch (timePeriod)
			{
				case TimePeriod.Today:
				case TimePeriod.Yesterday:
					// Previous period is one day before
					return (currentStartDate.AddDays(-1), currentEndDate.AddDays(-1));

				case TimePeriod.ThisWeek:
				case TimePeriod.LastWeek:
					// Previous period is 7 days before
					return (currentStartDate.AddDays(-7), currentEndDate.AddDays(-7));

				case TimePeriod.ThisMonth:
					// Previous period is last month
					var prevMonthStart = currentStartDate.AddMonths(-1);
					var prevMonthEnd = currentStartDate.AddDays(-1);
					return (prevMonthStart, prevMonthEnd);

				case TimePeriod.LastMonth:
					// Previous period is the month before last
					var monthBeforeLastStart = currentStartDate.AddMonths(-1);
					var monthBeforeLastEnd = currentStartDate.AddDays(-1);
					return (monthBeforeLastStart, monthBeforeLastEnd);

				case TimePeriod.ThisYear:
				case TimePeriod.LastYear:
					// Previous period is one year before
					return (currentStartDate.AddYears(-1), currentEndDate.AddYears(-1));

				case TimePeriod.CustomRange:
					// For custom ranges, the original logic is correct
					var daysDiff = currentEndDate.DayNumber - currentStartDate.DayNumber + 1;
					return (currentStartDate.AddDays(-daysDiff), currentStartDate.AddDays(-1));

				case TimePeriod.AllTime:
					// There is no previous period for "AllTime"
					// Return MinValue as a signal, or handle as needed
					return (DateOnly.MinValue, DateOnly.MinValue);

				default:
					// Default: return the day before
					return (currentStartDate.AddDays(-1), currentEndDate.AddDays(-1));
			}
		}
		private GroupingLevel DetermineGroupingLevel(TimePeriod timePeriod, DateOnly startDate, DateOnly endDate)
		{
			switch (timePeriod)
			{
				case TimePeriod.Today:
				case TimePeriod.Yesterday:
				case TimePeriod.ThisWeek:
				case TimePeriod.LastWeek:
					return GroupingLevel.ByDay;

				case TimePeriod.ThisMonth:
				case TimePeriod.LastMonth:
					return GroupingLevel.ByWeek;

				case TimePeriod.ThisYear:
				case TimePeriod.LastYear:
					return GroupingLevel.ByMonth;

				case TimePeriod.AllTime:
				case TimePeriod.CustomRange:
					// ✅ FIXED: Better thresholds
					int daySpan = endDate.DayNumber - startDate.DayNumber + 1;

					if (daySpan <= 7) // Up to 1 week
						return GroupingLevel.ByDay;
					else if (daySpan <= 93) // Up to ~3 months (✅ changed from 30)
						return GroupingLevel.ByWeek;
					else
						return GroupingLevel.ByMonth;

				default:
					return GroupingLevel.ByDay;
			}
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
