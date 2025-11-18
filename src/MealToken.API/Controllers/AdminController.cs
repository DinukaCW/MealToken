using Authentication.Models.DTOs;
using Authentication.Models.Entities;
using MealToken.API.Helpers;
using MealToken.Application.Interfaces;
using MealToken.Application.Services;
using MealToken.Domain.Interfaces;
using MealToken.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealToken.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AdminController : ControllerBase
	{
		private readonly ILogger<AdminController> _logger;
		private readonly IConfiguration _configuration;
		private readonly IAdminService _adminService;

		public AdminController(ILogger<AdminController> logger, IConfiguration configuration, IAdminService adminService)
		{
			_logger = logger;
			_configuration = configuration;
			_adminService = adminService;
		}

		[HttpPost("AddPerson")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> AddPerson([FromBody] PersonCreateDto personCreateDto)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);

				var result = await _adminService.AddPersonAsync(personCreateDto);

				if (!result.Success)
					return BadRequest(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				// optional: _logger.LogError(ex, "Error adding employee");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpPut("UpdatePerson")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> UpdatePerson(int personId, [FromBody] PersonCreateDto personCreateDto)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);

				var result = await _adminService.UpdatePersonAsync(personId,personCreateDto);

				if (!result.Success)
					return NotFound(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				// optional: _logger.LogError(ex, "Error updating employee {EmployeeId}", id);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpDelete("DeletePerson")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> DeletePerson(int personId)
		{
			try
			{
				var result = await _adminService.DeletePersonAsync(personId);

				if (!result.Success)
					return NotFound(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				// optional: _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpGet("GetPersonList")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetPersonsList()
		{
			try
			{
				var persons = await _adminService.GetPersonsListAsync();

				if (persons == null || !persons.Any())
					return NotFound(new { Success = false, Message = "No employees found" });

				return Ok(persons);
			}
			catch (Exception ex)
			{
				// optional: _logger.LogError(ex, "Error retrieving employees list");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpGet("GetPersonById")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetPersonById(int personId)
		{
			try
			{
				var person = await _adminService.GetPersonByIdAsync(personId);

				if (person == null)
					return NotFound(new { Success = false, Message = "No person found" });

				return Ok(person);
			}
			catch (Exception ex)
			{
				// optional: _logger.LogError(ex, "Error retrieving employees list");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpGet("GetPersonDetails")]
		public async Task<IActionResult> GetEmployeeCreationDetails()
		{
			try
			{
				var empDetails = await _adminService.GetEmployeeCreationDetailsAsync();

				if (empDetails == null)
				{
					return NotFound(new { message = "User request not found." });
				}

				return Ok(empDetails);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error occurred while retrieving request." });
			}
		}

		[HttpPost("CreateSupplier")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> CreateSupplier([FromBody] SupplierCreateRequestDto supplierDto)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);

				var result = await _adminService.CreateSupplierAsync(supplierDto);

				if (!result.Success)
					return BadRequest(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating supplier");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpPut("UpdateSupplier")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> UpdateSupplier(int supplierId, [FromBody] SupplierCreateRequestDto supplierDto)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);

				var result = await _adminService.UpdateSupplierAsync(supplierId, supplierDto);

				if (!result.Success)
					return NotFound(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating supplier {SupplierId}", supplierId);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpDelete("DeleteSupplier")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> DeleteSupplier(int supplierId)
		{
			try
			{
				var result = await _adminService.DeleteSupplierAsync(supplierId);

				if (!result.Success)
					return NotFound(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting supplier {SupplierId}", supplierId);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}
		[HttpGet("GetSuppliersList")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetSuppliersList()
		{
			try
			{
				var suppliers = await _adminService.GetSuppliersListAsync();

				if (suppliers == null || !suppliers.Any())
					return NotFound(new { Success = false, Message = "No suppliers found" });

				return Ok(suppliers);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving suppliers list");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpGet("GetSupplierById")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetSupplierById(int supplierId)
		{
			try
			{
				var supplier = await _adminService.GetSupplierByIdAsync(supplierId);

				if (supplier == null || (supplier is ServiceResult result && !result.Success))
					return NotFound(new { Success = false, Message = "Supplier not found" });

				return Ok(supplier);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving supplier {SupplierId}", supplierId);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}
		[HttpPost("AddMealType")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> AddMealType(string mealTypeName, string? description)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);

				var result = await _adminService.CreateMealTypeAsync(mealTypeName,description);

				if (!result.Success)
					return BadRequest(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				// optional: _logger.LogError(ex, "Error adding employee");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpPut("UpdateMealType")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> UpdateMealType(int mealTypeId, [FromBody] MealTypeUpdateDto mealTypeUpdateDto)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);

				var result = await _adminService.UpdateMealTypeAsync(mealTypeId,mealTypeUpdateDto);

				if (!result.Success)
					return NotFound(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				// optional: _logger.LogError(ex, "Error updating employee {EmployeeId}", id);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpDelete("DeleteMealType")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> DeleteMealType(int mealTypeId)
		{
			try
			{
				var result = await _adminService.DeleteMealTypeAsync(mealTypeId);

				if (!result.Success)
					return NotFound(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				// optional: _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpGet("GetAddOns")]
		[Authorize]
		public async Task<IActionResult> GetMealAddOns()
		{
			try
			{
				var addOns = await _adminService.GetAddOnsAsync();

				// If the service returned null OR both lists are null/empty → NotFound
				if (addOns == null ||
					((addOns.Snacks == null || !addOns.Snacks.Any()) &&
					 (addOns.Beverages == null || !addOns.Beverages.Any())))
				{
					return NotFound(new { Success = false, Message = "No add-ons found" });
				}

				// Ensure we don’t return null lists → always at least empty collections
				addOns.Snacks ??= new List<Snacks>();
				addOns.Beverages ??= new List<Beverages>();

				return Ok(new { Success = true, Data = addOns });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving add-ons");
				return StatusCode(500, new { Success = false, Message = "Internal server error" });
			}
		}


		[HttpGet("GetMealTypes")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetMealTypeList()
		{
			try
			{
				var mealTypes= await _adminService.GetMealTypeListAsync();

				if (mealTypes == null || !mealTypes.Any())
					return NotFound(new { Success = false, Message = "No meal types found" });

				return Ok(mealTypes);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving meal type list");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpGet("GetMealTypeDetailsById")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetMealTypeDetailsById(int mealTypeId)
		{
			try
			{
				var mealTypes = await _adminService.GetMealTypeByIdAsync(mealTypeId);

				if (mealTypes.Data == null)
					return NotFound(new { Success = false, Message = "No meal types found" });

				return Ok(mealTypes);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving meal type list");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}
		[HttpPost("AddMealCost")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> AddMealCost([FromBody] MealCostDto mealCostDto)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);

				var result = await _adminService.CreateMealCostAsync(mealCostDto);

				if (!result.Success)
					return BadRequest(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding meal cost");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpPut("UpdateMealCost")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> UpdateMealCost(int mealCostId, [FromBody] MealCostDto mealCostDto)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);

				var result = await _adminService.UpdateMealCostAsync(mealCostId,mealCostDto);

				if (!result.Success)
					return NotFound(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating mealCost {mealCostId}", mealCostId);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpDelete("DeleteMealCost")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> DeleteMealCost(int mealCostId)
		{
			try
			{
				var result = await _adminService.DeleteMealCostAsync(mealCostId);

				if (!result.Success)
					return NotFound(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating mealCost {mealCostId}", mealCostId);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpGet("GetMealCostCreationDetails")]
		[Authorize]

		public async Task<IActionResult> GetMealCostCreationDetails()
		{
			try
			{
				var details = await _adminService.GetMealCostCreationDetails();

				// Check if no details exist
				if (details == null ||
					((details.Suppliers == null || !details.Suppliers.Any()) &&
					 (details.MealTypes == null || !details.MealTypes.Any())))
				{
					return NotFound(new { Success = false, Message = "No meal cost creation details found" });
				}

				// Normalize null lists to empty lists
				details.Suppliers ??= new List<SupplierD>();
				details.MealTypes ??= new List<MealTypeReturn>();

				return Ok(new { Success = true, Data = details });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving meal cost creation details");
				return StatusCode(500, new { Success = false, Message = "Internal server error" });
			}
		}



		[HttpGet("GetMealCostList")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetMealCostsList()
		{
			try
			{
				var mealCosts = await _adminService.GetMealCostsListAsync();

				if (mealCosts == null || !mealCosts.Any())
					return NotFound(new { Success = false, Message = "No suppliers found" });

				return Ok(mealCosts);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving suppliers list");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}

		[HttpGet("GetMealCostById")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetMealCostById(int mealCostId)
		{
			try
			{
				var mealCost = await _adminService.GetMealCostByIdAsync(mealCostId);

				if (mealCost == null)
					return NotFound(new { Success = false, Message = "mealCost not found" });

				return Ok(mealCost);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving mealCost {mealCostId}", mealCostId);
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}
		[HttpPut("UpdateSettings")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> UpdateSettings([FromForm] ApplicationSettings settings)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest(ModelState);

				var result = await _adminService.UpdateSettingsAsync(settings);

				if (!result.Success)
					return NotFound(new { result.Success, result.Message });

				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating settings");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}
		[HttpGet("GetApplicationSettings")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetApplicationSettings()
		{
			try
			{
				var tenant = await _adminService.GetApplicationSettingsAsync();

				if (tenant == null)
					return NotFound(new { Success = false, Message = "tenant not found" });

				return Ok(tenant);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving settings");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}
		[HttpGet("GetManualPrintedTokens")]
		[Authorize(Roles = "Admin")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetManualPrintedTokens()
		{
			try
			{
				var tokens = await _adminService.GetManualPrintedTokensAsync();

				if (tokens == null)
					return NotFound(new { Success = false, Message = "Tokens not found" });

				return Ok(tokens);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving manual printed tokens");
				return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
			}
		}
	}
}
