using MealToken.API.Helpers;
using MealToken.Application.Interfaces;
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


	}
}
