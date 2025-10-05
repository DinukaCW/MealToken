using Authentication.Models.DTOs;
using MealToken.API.Helpers;
using MealToken.Application.Interfaces;
using MealToken.Application.Services;
using MealToken.Domain.Enums;
using MealToken.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MealToken.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BusinessController : ControllerBase
	{
		private readonly ILogger<BusinessController> _logger;
		private readonly IConfiguration _configuration;
		private readonly IBusinessService _businessService;
		private readonly ScheduleDateGeneratorService _dateGenerator;

		public BusinessController(ILogger<BusinessController> logger, IConfiguration configuration, IBusinessService businessService,ScheduleDateGeneratorService dateGenerator)
		{
			_logger = logger;
			_configuration = configuration;
			_businessService = businessService;
			_dateGenerator = dateGenerator;
		}
		[HttpGet("GetDevicesList")]
		public async Task<IActionResult> GetClientDevice()
		{
			try
			{
				// Get ClientID from header
				if (!Request.Headers.TryGetValue("ClientID", out var clientIdHeader) ||
					!int.TryParse(clientIdHeader.FirstOrDefault(), out int clientId))
				{
					return BadRequest(new { message = "ClientID header is required and must be a valid integer." });
				}

				var clientDevice = await _businessService.GetClientDeviceDetails(clientId);

				if (clientDevice == null)
				{
					return NotFound(new { message = "Client device not found." });
				}

				return Ok(clientDevice);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving client device for ClientID: {ClientId}",
					Request.Headers["ClientID"].FirstOrDefault());
				return StatusCode(500, new { message = "An error occurred while retrieving client device." });
			}
		}
		/*
		[HttpGet("active")]
		public async Task<IActionResult> GetActiveClientDevices()
		{
			try
			{
				// Get TenantID from header for multi-tenant scenarios
				int? tenantId = null;
				if (Request.Headers.TryGetValue("TenantID", out var tenantIdHeader) &&
					int.TryParse(tenantIdHeader.FirstOrDefault(), out int parsedTenantId))
				{
					tenantId = parsedTenantId;
				}

				var activeDevices = await _clientDeviceService.GetActiveClientDevicesAsync(tenantId);
				return Ok(activeDevices);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving active client devices");
				return StatusCode(500, new { message = "An error occurred while retrieving active client devices." });
			}
		}*/
		[HttpPost("CreateSchedule")]
		[Authorize]
		public async Task<IActionResult> CreateSchedule([FromBody] SheduleCreateDto request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new ServiceResult
				{
					Success = false,
					Message = "Invalid request data.",
					Data = ModelState.Values.SelectMany(v => v.Errors)
											.Select(e => e.ErrorMessage)
				});
			}

			try
			{
				// Generate dates based on schedule period
				var dates = _dateGenerator.GenerateDatesForPeriod(request.SchedulePeriod, request.DateParameters);

				// Create the schedule DTO
				var scheduleDto = new SheduleDTO
				{
					ScheduleName = request.ScheduleName,
					SchedulePeriod = request.SchedulePeriod,
					ScheduleDates = dates,
					Note = request.Note,
					MealTypes = request.MealTypes,
					AssignedPersonIds = request.AssignedPersonIds
				};

				var result = await _businessService.CreateScheduleAsync(scheduleDto);

				if (result.Success)
					return Ok(result);
				else
					return BadRequest(result);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new ServiceResult
				{
					Success = false,
					Message = "Invalid input provided.",
					Data = ex.Message
				});
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new ServiceResult
				{
					Success = false,
					Message = "Operation could not be completed.",
					Data = ex.Message
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ServiceResult
				{
					Success = false,
					Message = "An unexpected error occurred while creating the schedule.",
					Data = ex.Message
				});
			}
		}

		[HttpPut("UpdateSchedule")]
		[Authorize]
		public async Task<IActionResult> UpdateSchedule(int scheduleId, [FromBody] SheduleCreateDto request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new ServiceResult
				{
					Success = false,
					Message = "Invalid request data.",
					Data = ModelState.Values.SelectMany(v => v.Errors)
											.Select(e => e.ErrorMessage)
				});
			}

			try
			{
				// Generate dates based on schedule period
				var dates = _dateGenerator.GenerateDatesForPeriod(request.SchedulePeriod, request.DateParameters);

				var updateDto = new SheduleDTO
				{
					ScheduleName = request.ScheduleName,
					SchedulePeriod = request.SchedulePeriod,
					ScheduleDates = dates,
					Note = request.Note,
					MealTypes = request.MealTypes,
					AssignedPersonIds = request.AssignedPersonIds
				};

				var existingSchedule = await _businessService.UpdateScheduleAsync(scheduleId, updateDto);

				return Ok(new ServiceResult
				{
					Success = true,
					Message = "Schedule updated successfully",
					ObjectId = existingSchedule.ObjectId,
					Data = existingSchedule.Data

				});
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new ServiceResult
				{
					Success = false,
					Message = "Invalid input provided.",
					Data = ex.Message
				});
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new ServiceResult
				{
					Success = false,
					Message = "Operation could not be completed.",
					Data = ex.Message
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ServiceResult
				{
					Success = false,
					Message = "An unexpected error occurred while updating the schedule.",
					Data = ex.Message
				});
			}
		}
		[HttpDelete("DeleteSchedule")]
		[Authorize]
		public async Task<IActionResult> DeleteSchedule(int scheduleId)
		{
			try
			{
				var result = await _businessService.DeleteScheduleAsync(scheduleId);

				if (!result.Success)
				{
					return NotFound(new ServiceResult
					{
						Success = false,
						Message = "Schedule not found.",
						ObjectId = scheduleId
					});
				}

				return Ok(new ServiceResult
				{
					Success = true,
					Message = "Schedule deleted successfully",
					ObjectId = scheduleId
				});
			}
			catch (Exception ex)
			{
				// _logger.LogError(ex, "Error occurred while deleting schedule");

				return StatusCode(500, new ServiceResult
				{
					Success = false,
					Message = "An unexpected error occurred while deleting the schedule.",
					ObjectId = scheduleId
				});
			}
		}

		[HttpGet("GetListOfShedules")]
		[Authorize]
		public async Task<IActionResult> GetschedulesList()
		{
			try
			{
				var schedules = await _businessService.GetScheduleListAsync();

				if (schedules== null )
					return NotFound(new { Success = false, Message = "No schedules found" });

				return Ok(schedules);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving schedules list");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}
		[HttpGet("GeSheduleById")]
		[Authorize]
		public async Task<IActionResult> GetSchedulesById(int scheduleId)
		{
			try
			{
				var schedules = await _businessService.GetScheduleByIdAsync(scheduleId);

				if (schedules == null)
					return NotFound(new { Success = false, Message = "No schedule found" });

				return Ok(schedules);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving schedule details");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}
		[HttpGet("GetScheduleCreationDetails")]
		[Authorize]
		public async Task<IActionResult> GetScheduleCreationDetails()
		{
			try
			{
				var result = await _businessService.GetScheduleCreationDetailsAsync();

				if (result.Data == null)
					return NotFound(new { Success = false, Message = "No data found" });

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving schedule creation details");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

        [HttpPost("CreateMealRequest")]
        [Authorize] 
        public async Task<IActionResult> CreateMealRequest([FromBody] RequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for meal request creation");
                    return BadRequest(ModelState);
                }

                var result = await _businessService.CreateMealRequestAsync(requestDto);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    requestId = result.ObjectId,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateMealRequest endpoint");
                return StatusCode(500, new { success = false, message = "Internal server error occurred while creating meal request." });
            }
        }

        // Update existing meal request
        [HttpPut("UpdateMealRequest")]
        [Authorize] // Only request owner can update
        public async Task<IActionResult> UpdateMealRequest(int requestId, [FromBody] RequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for meal request update. RequestId: {RequestId}", requestId);
                    return BadRequest(ModelState);
                }

                if (requestId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid request ID" });
                }

                var result = await _businessService.UpdateMealRequestAsync(requestId, requestDto);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    requestId = result.ObjectId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateMealRequest endpoint. RequestId: {RequestId}", requestId);
                return StatusCode(500, new { success = false, message = "Internal server error occurred while updating meal request." });
            }
        }

        // Get specific request details
        [HttpGet("GetRequestDetails")]
        [Authorize]
        public async Task<IActionResult> GetRequestDetails(int requestId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid request ID" });
                }

                var result = await _businessService.GetRequestDetailsAsync(requestId);

                if (!result.Success)
                {
                    return NotFound(new { success = false, message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRequestDetails endpoint. RequestId: {RequestId}", requestId);
                return StatusCode(500, new { success = false, message = "Internal server error occurred while retrieving request details." });
            }
        }

        // Get pending requests (for approvers)
        [HttpGet("GetPendingRequests")]
        [Authorize]
        public async Task<IActionResult> GetPendingRequests()
        {
            try
            {
                var result = await _businessService.GetPendingRequestListAsync();

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                var requests = result.Data as List<RequestReturn> ?? new List<RequestReturn>();

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    count = requests.Count,
                    data = requests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPendingRequests endpoint");
                return StatusCode(500, new { success = false, message = "Internal server error occurred while retrieving pending requests." });
            }
        }

        // Get approved requests
        [HttpGet("GetApprovedRequests")]
        [Authorize]
        public async Task<IActionResult> GetApprovedRequests()
        {
            try
            {
                var result = await _businessService.GetApprovedRequestListAsync();

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                var requests = result.Data as List<RequestReturn> ?? new List<RequestReturn>();

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    count = requests.Count,
                    data = requests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetApprovedRequests endpoint");
                return StatusCode(500, new { success = false, message = "Internal server error occurred while retrieving approved requests." });
            }
        }

        // Get current user's requests
        [HttpGet("GetMyRequests")]
        [Authorize]
        public async Task<IActionResult> GetMyRequests()
        {
            try
            {
                var result = await _businessService.GetRequestListByIdAsync();

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                var requests = result.Data as List<RequestReturn> ?? new List<RequestReturn>();

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    count = requests.Count,
                    data = requests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMyRequests endpoint");
                return StatusCode(500, new { success = false, message = "Internal server error occurred while retrieving user requests." });
            }
        }

        // Approve or reject request
        [HttpPut("UpdateRequestStatus")]
        [Authorize]
        public async Task<IActionResult> UpdateRequestStatus(int requestId, UserRequestStatus status)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for status update. RequestId: {RequestId}", requestId);
                    return BadRequest(ModelState);
                }

                if (requestId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid request ID" });
                }
				int userId = GetCurrentUserId();
                var result = await _businessService.UpdateRequestStatusAsync(requestId,status,userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    requestId = result.ObjectId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateRequestStatus endpoint. RequestId: {RequestId}", requestId);
                return StatusCode(500, new { success = false, message = "Internal server error occurred while updating request status." });
            }
        }

        [HttpPost("GetTokenDetails")]
        public async Task<IActionResult> GetMealTokenDetails([FromBody] MealDeviceRequest mealDeviceRequest)
        {

            try
            {
                if (!Request.Headers.TryGetValue("ClientID", out var clientIdHeader) ||
                    !int.TryParse(clientIdHeader.FirstOrDefault(), out int clientId))
                {
                    return BadRequest(new { message = "ClientID header is required and must be a valid integer." });
                }

                var serviceResult = await _businessService.ProcessLogicAsync(clientId, mealDeviceRequest);

                if (serviceResult.Success)
                {
                    return Ok(serviceResult.Data);
                }
                else
                {
                    return BadRequest(new { message = serviceResult.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing token request. ");
                return StatusCode(500, new { message = "An unexpected server error occurred." });
            }
        }
        [HttpPatch("TokenPrinted")]
        public async Task<IActionResult> UpdateMealConsumptionStatus(int mealConsumptionId, bool status)
        {
            // Note: Using [HttpPatch] is slightly more accurate than [HttpPut] for updating a single field (status).

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Call the service layer method to update the record
                var serviceResult = await _businessService.UpdateMealConsumption(
                    mealConsumptionId,status);

                if (serviceResult.Success)
                {
                    // 200 OK is standard for successful updates
                    return Ok(new { message = serviceResult.Message });
                }
                else
                {
                    // Use the status code from the ServiceResult (e.g., 404 Not Found, 409 Conflict)
                    return BadRequest( new { message = serviceResult.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating meal consumption ID: {Id}", mealConsumptionId);
                return StatusCode(500, new { message = "An unexpected error occurred during consumption update." });
            }
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return int.Parse(userIdClaim);
        }
    }
}
