using MealToken.API.Helpers;
using MealToken.Application.Interfaces;
using MealToken.Domain.Enums;
using MealToken.Domain.Models.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealToken.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMealReportService _reportService;

        public ReportController(ILogger<ReportController> logger, IConfiguration configuration, IMealReportService mealReportService)
        {
            _logger = logger;
            _configuration = configuration;
            _reportService = mealReportService;

        }


		[HttpPost("GetActivityLogs")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetDashboardSummary([FromBody] ActivityLogFilter logFilter)
		{
			try
			{
				var result = await _reportService.GetActivityLogsAsync(logFilter);

				if (!result.Success)
				{
					return BadRequest(result);
				}

				return Ok(result.Data);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error:{ex.InnerException}");
			}
		}

		[HttpGet("GetActivityLogsFilterData")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetActivityLogsFilterData()
		{
			try
			{
				var result = await _reportService.GetActivityFilterDataAsync();

				if (!result.Success)
				{
					return BadRequest(result);
				}

				return Ok(result.Data);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error:{ex.InnerException}");
			}
		}

		[HttpGet("reportDashboard")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetDashboardSummary()
		{
			try
			{
				var result = await _reportService.GetDashboardSummaryAsync();

				if (!result.Success)
				{
					return BadRequest(result);
				}

				return Ok(result.Data);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error:{ex.InnerException}");
			}
        }

        [HttpGet("weekly")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetWeeklyReport( [FromQuery] string startDate, [FromQuery] string endDate)
        {
			try
			{
				if (!DateOnly.TryParse(startDate, out var start) ||
					!DateOnly.TryParse(endDate, out var end))
				{
					return BadRequest("Invalid date format");
				}

				var result = await _reportService.GenerateWeeklyReportAsync(start, end);

				if (!result.Success)
				{
					return BadRequest(result);
				}


				return Ok(result.Data);
			}
			catch (Exception ex) 
			{
				return StatusCode(500, $"Internal server error:{ex.InnerException}");
			}

        }

        [HttpGet("current-week")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetCurrentWeekReport()
		{
			try
			{
				var result = await _reportService.GenerateCurrentWeekReportAsync();

				if (!result.Success)
				{
					return BadRequest(result);
				}

				return Ok(result.Data);
			}
			catch (Exception ex) {
				return StatusCode(500, $"Internal server error:{ex.InnerException}");
			}

        }

		[HttpGet("MealConsumptionSummary")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> MealConsumptionSummaryReport([FromQuery] string startDate, [FromQuery] string endDate)
		{
			try
			{
				if (!DateOnly.TryParse(startDate, out var start) ||
					!DateOnly.TryParse(endDate, out var end))
				{
					return BadRequest("Invalid date format");
				}

				var result = await _reportService.GetMealConsumptionSummaryAsync(start, end);

				if (!result.Success)
				{
					return BadRequest(result);
				}

				return Ok(result.Data);
			}
			catch (Exception ex) 
			{ return StatusCode(500, $"Internal server error:{ex.InnerException}"); }

		}

		[HttpGet("SupplierWiseReport")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> SupplierWiseReport([FromQuery] string startDate, [FromQuery] string endDate)
		{
			try
			{
				if (!DateOnly.TryParse(startDate, out var start) ||
					!DateOnly.TryParse(endDate, out var end))
				{
					return BadRequest("Invalid date format");
				}

				var result = await _reportService.GetAllSuppliersPaymentReportAsync(start, end);

				if (!result.Success)
				{
					return BadRequest(result);
				}

				return Ok(result.Data);
			}
			catch (Exception ex)
			{ return StatusCode(500, $"Internal server error:{ex.InnerException}"); }
		}

		[HttpGet("GetTodayMeals")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetTodayMeals([FromQuery] string date, [FromQuery] string time)
		{
			try
			{
				if (!DateOnly.TryParse(date, out var parsedDate))
				{
					return BadRequest(new
					{
						Success = false,
						Message = "Invalid date format. Please use YYYY-MM-DD."
					});
				}

				if (!TimeOnly.TryParse(time, out var parsedTime))
				{
					return BadRequest(new
					{
						Success = false,
						Message = "Invalid time format. Please use HH:mm (24-hour format)."
					});
				}

				var result = await _reportService.GetTodayMealSchedulesAsync(parsedDate, parsedTime);

				if (!result.Success)
				{
					return BadRequest(result);
				}

				return Ok(result);
			}
			catch (Exception ex)
			{ return StatusCode(500, $"Internal server error:{ex.InnerException}"); }
		}

		[HttpPost("GetDashboardOverview")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetDashBoardOverView([FromBody] DashBoardRequest request)
		{
			try
			{
				DateOnly? parsedStartDate = null;
				DateOnly? parsedEndDate = null;

				// Date parsing logic is now inside the method
				if (request.TimePeriod == TimePeriod.CustomRange)
				{
					if (!DateOnly.TryParse(request.RangeStartDate, out var startDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeStartDate format. Please use YYYY-MM-DD." });
					}
					if (!DateOnly.TryParse(request.RangeEndDate, out var endDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeEndDate format. Please use YYYY-MM-DD." });
					}

					parsedStartDate = startDate;
					parsedEndDate = endDate;
				}

				// Call the correct service method that matches the request
				var result = await _reportService.GetMealDashboardDataAsync(
					request.TimePeriod,
					request.DepartmentIds,
					parsedStartDate,
					parsedEndDate);

				return Ok(result);
			}
			catch (Exception ex)
			{
				// Log the full exception with context
				_logger.LogError(ex, "Error occurred in GetDashBoardOverView for TimePeriod {TimePeriod}", request.TimePeriod);

				// Return a generic, safe error message
				return StatusCode(500, new { Success = false, Message = "An internal server error occurred." });
			}
		}

		[HttpPost("GetDashBoardDepartmentData")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetDashBoardDepartmentOverView([FromBody] DashBoardRequest request)
		{
			try
			{
				DateOnly? parsedStartDate = null;
				DateOnly? parsedEndDate = null;

				// Date parsing logic is now inside the method
				if (request.TimePeriod == TimePeriod.CustomRange)
				{
					if (!DateOnly.TryParse(request.RangeStartDate, out var startDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeStartDate format. Please use YYYY-MM-DD." });
					}
					if (!DateOnly.TryParse(request.RangeEndDate, out var endDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeEndDate format. Please use YYYY-MM-DD." });
					}

					parsedStartDate = startDate;
					parsedEndDate = endDate;
				}

				// Call the correct service method that matches the request
				var result = await _reportService.GetMealsByDepartmentAsync(
					request.TimePeriod,
					request.DepartmentIds,
					parsedStartDate,
					parsedEndDate);

				return Ok(result);
			}
			catch (Exception ex)
			{
				// Log the full exception with context
				_logger.LogError(ex, "Error occurred in GetDashBoardOverView for TimePeriod {TimePeriod}", request.TimePeriod);

				// Return a generic, safe error message
				return StatusCode(500, new { Success = false, Message = "An internal server error occurred." });
			}
		}
		[HttpPost("GetDashboardSupplierData")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetDashBoardSupplierOverView([FromBody] DashBoardRequest request)
		{
			try
			{
				DateOnly? parsedStartDate = null;
				DateOnly? parsedEndDate = null;

				// Date parsing logic is now inside the method
				if (request.TimePeriod == TimePeriod.CustomRange)
				{
					if (!DateOnly.TryParse(request.RangeStartDate, out var startDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeStartDate format. Please use YYYY-MM-DD." });
					}
					if (!DateOnly.TryParse(request.RangeEndDate, out var endDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeEndDate format. Please use YYYY-MM-DD." });
					}

					parsedStartDate = startDate;
					parsedEndDate = endDate;
				}

				// Call the correct service method that matches the request
				var result = await _reportService.GetMealsBySupplierAsync(
					request.TimePeriod,
					request.DepartmentIds,
					parsedStartDate,
					parsedEndDate);

				return Ok(result);
			}
			catch (Exception ex)
			{
				// Log the full exception with context
				_logger.LogError(ex, "Error occurred in GetDashBoardOverView for TimePeriod {TimePeriod}", request.TimePeriod);

				// Return a generic, safe error message
				return StatusCode(500, new { Success = false, Message = "An internal server error occurred." });
			}
		}
		[HttpPost("GetDashboardMealTypeData")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetDashBoardMealTypeOverView([FromBody] DashBoardRequest request)
		{
			try
			{
				DateOnly? parsedStartDate = null;
				DateOnly? parsedEndDate = null;

				// Date parsing logic is now inside the method
				if (request.TimePeriod == TimePeriod.CustomRange)
				{
					if (!DateOnly.TryParse(request.RangeStartDate, out var startDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeStartDate format. Please use YYYY-MM-DD." });
					}
					if (!DateOnly.TryParse(request.RangeEndDate, out var endDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeEndDate format. Please use YYYY-MM-DD." });
					}

					parsedStartDate = startDate;
					parsedEndDate = endDate;
				}

				// Call the correct service method that matches the request
				var result = await _reportService.GetMealsByMealTypeAsync(
					request.TimePeriod,
					request.DepartmentIds,
					parsedStartDate,
					parsedEndDate);

				return Ok(result);
			}
			catch (Exception ex)
			{
				// Log the full exception with context
				_logger.LogError(ex, "Error occurred in GetDashBoardOverView for TimePeriod {TimePeriod}", request.TimePeriod);

				// Return a generic, safe error message
				return StatusCode(500, new { Success = false, Message = "An internal server error occurred." });
			}
		}

		[HttpPost("GetDashboardMealCostData")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetDashBoardMealCostOverView([FromBody] DashBoardRequest request)
		{
			try
			{
				DateOnly? parsedStartDate = null;
				DateOnly? parsedEndDate = null;

				// Date parsing logic is now inside the method
				if (request.TimePeriod == TimePeriod.CustomRange)
				{
					if (!DateOnly.TryParse(request.RangeStartDate, out var startDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeStartDate format. Please use YYYY-MM-DD." });
					}
					if (!DateOnly.TryParse(request.RangeEndDate, out var endDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeEndDate format. Please use YYYY-MM-DD." });
					}

					parsedStartDate = startDate;
					parsedEndDate = endDate;
				}

				// Call the correct service method that matches the request
				var result = await _reportService.GetMealsByCostAsync(
					request.TimePeriod,
					request.DepartmentIds,
					parsedStartDate,
					parsedEndDate);

				return Ok(result);
			}
			catch (Exception ex)
			{
				// Log the full exception with context
				_logger.LogError(ex, "Error occurred in GetDashBoardOverView for TimePeriod {TimePeriod}", request.TimePeriod);

				// Return a generic, safe error message
				return StatusCode(500, new { Success = false, Message = "An internal server error occurred." });
			}
		}

		[HttpPost("GetDashboardPersonTypeData")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetDashBoardPersonTypesOverView([FromBody] DashBoardRequest request)
		{
			try
			{
				DateOnly? parsedStartDate = null;
				DateOnly? parsedEndDate = null;

				// Date parsing logic is now inside the method
				if (request.TimePeriod == TimePeriod.CustomRange)
				{
					if (!DateOnly.TryParse(request.RangeStartDate, out var startDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeStartDate format. Please use YYYY-MM-DD." });
					}
					if (!DateOnly.TryParse(request.RangeEndDate, out var endDate))
					{
						return BadRequest(new { Success = false, Message = "Invalid RangeEndDate format. Please use YYYY-MM-DD." });
					}

					parsedStartDate = startDate;
					parsedEndDate = endDate;
				}

				// Call the correct service method that matches the request
				var result = await _reportService.GetMealsByPersonTypeAsync(
					request.TimePeriod,
					request.DepartmentIds,
					parsedStartDate,
					parsedEndDate);

				return Ok(result);
			}
			catch (Exception ex)
			{
				// Log the full exception with context
				_logger.LogError(ex, "Error occurred in GetDashBoardOverView for TimePeriod {TimePeriod}", request.TimePeriod);

				// Return a generic, safe error message
				return StatusCode(500, new { Success = false, Message = "An internal server error occurred." });
			}
		}
	}

	public class DashBoardRequest
	{
		public TimePeriod TimePeriod { get; set; }
		public List<int> DepartmentIds { get; set; }
		public string? RangeStartDate { get; set; }
		public string? RangeEndDate { get; set; }
	}
}
