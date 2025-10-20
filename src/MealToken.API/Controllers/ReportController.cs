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
            var result = await _reportService.GetDashboardSummaryAsync();

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result.Data);
        }

        [HttpGet("weekly")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetWeeklyReport( [FromQuery] string startDate, [FromQuery] string endDate)
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

        [HttpGet("current-week")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetCurrentWeekReport()
        {
            var result = await _reportService.GenerateCurrentWeekReportAsync();

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result.Data);
        }
		[HttpGet("weeklyMealTotalReport")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> WeeklyMealTotalReport([FromQuery] string startDate, [FromQuery] string endDate)
		{
			if (!DateOnly.TryParse(startDate, out var start) ||
				!DateOnly.TryParse(endDate, out var end))
			{
				return BadRequest("Invalid date format");
			}

			var result = await _reportService.GenerateMealConsumptionSummaryReportAsync(start, end);

			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result.Data);
		}
	}
}
