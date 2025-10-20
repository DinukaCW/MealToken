using Authentication.Interfaces;
using Authentication.Models.DTOs;
using Google.Apis.Auth;
using MealToken.API.Helpers;
using MealToken.Application.Interfaces;
using MealToken.Application.Services;
using MealToken.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MealToken.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService; // User service for handling user-related operations
		private readonly IMfaService _mfaService; // Service for handling Multi-Factor Authentication
		private readonly IConfiguration _configuration; // Configuration for application settings
		private readonly IJwtTokenService _jwtTokenService; // Service for handling JWT tokens
		private readonly IMultifacAuthenticationService _authenticationService; // Service for authentication
		private readonly IEntityCreationService _entityCreationService; // Service for entity creation

		/// <summary>
		/// Initializes a new instance of the <see cref="UserController"/> class.
		/// </summary>
		/// <param name="userService">The user service.</param>
		/// <param name="configuration">The configuration.</param>
		/// <param name="mfaService">The MFA service.</param>
		/// <param name="authenticationService">The authentication service.</param>
		public UserController(IUserService userService, IConfiguration configuration, IMfaService mfaService, IMultifacAuthenticationService authenticationService, IEntityCreationService entityCreationService, IJwtTokenService jwtTokenService)
		{
			_userService = userService;
			_configuration = configuration;
			_mfaService = mfaService;
			_authenticationService = authenticationService;
			_entityCreationService = entityCreationService;
			_jwtTokenService = jwtTokenService;
		}

		// Endpoint for user login
		/// <summary>
		/// Endpoint for user login.
		/// </summary>
		/// <param name="loginRequest">The login request data.</param>
		/// <returns>Returns an action result indicating the login outcome.</returns>
		[HttpPost("login")]
		[EnableRateLimiting("LoginLimit")]
		public async Task<IActionResult> Login([FromBody] RequestLogin loginRequest)
		{
			try
			{   // Check if the model state is valid
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				// Attempt to log the user in
				var result = await _userService.LoginAsync(loginRequest);
				// Check if login was unsuccessful
				if (!result.Success)
				{
					return Unauthorized(new { userlocked = result.UserLocked, message = result.Message });
				}

				// If MFA is required
				if (!result.RequiresMfa)
				{
					return Ok(new { Acctoken = result.AccessToken, RefToken = result.RefreshToken });
				}
				return Ok(new { Success = result.Success, Message = result.Message, UserId = result.UserId, });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error:{ex.Message}");
			}
		}

		[HttpPost("SendUserRequest")]
		public async Task<IActionResult> AddNewUserRequest([FromBody] UserDetails userDetails)
		{
			try
			{
				// Validate the request model
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Create new user
				var result = await _entityCreationService.SubmitUserRequestAsync(userDetails);

				// Handle failed user creation
				if (!result.Success)
				{
					return BadRequest(new { message = result.Message });
				}
				return Ok(new { message = result.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error:{ex.InnerException}");
			}
		}

		[HttpGet("GetPendingRequests")]
		[Authorize(Roles = "Admin,DepartmentHead")] // Require admin or reviewer role
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetPendingUserRequests()
		{
			try
			{
				var requests = await _entityCreationService.GetPendingUserRequestsAsync();

				return Ok(new
				{
					success = true,
					count = requests.Count,
					data = requests
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error occurred while retrieving requests." });
			}
		}

		[HttpPost("ApproveRequest")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> ApproveUserRequest([FromBody] ApproveRequestModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Get reviewer ID from JWT token
				var reviewerId = GetCurrentUserId();
				//var reviewerId = 3;
				if (reviewerId == null)
				{
					return Unauthorized(new { message = "Unable to identify reviewer." });
				}

				var result = await _entityCreationService.ApproveUserRequestAsync(reviewerId.Value, model);

				if (!result.Success)
				{
					return BadRequest(new { message = result.Message });
				}

				return Ok(new
				{
					message = result.Message,
					userId = result.ObjectId
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error occurred while approving request." });
			}
		}

		[HttpPost("RejectRequest")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> RejectUserRequest([FromBody] RejectRequestModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Get reviewer ID from JWT token
				var reviewerId = GetCurrentUserId();
				if (reviewerId == null)
				{
					return Unauthorized(new { message = "Unable to identify reviewer." });
				}

				var result = await _entityCreationService.RejectUserRequestAsync(
					model.RequestId,
					reviewerId.Value,
					model.RejectionReason);

				if (!result.Success)
				{
					return BadRequest(new { message = result.Message });
				}

				return Ok(new { message = result.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error occurred while rejecting request." });
			}
		}

		[HttpGet("GetRequestById")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetUserRequestById(int requestId)
		{
			try
			{
				var request = await _entityCreationService.GetUserRequestByIdAsync(requestId);

				if (request == null)
				{
					return NotFound(new { message = "User request not found." });
				}

				return Ok(new
				{
					success = true,
					data = request
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error occurred while retrieving request." });
			}
		}
		[HttpGet("GetAllUsers")]
		[Authorize(Roles = "Admin,DepartmentHead")]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetAllUsers()
		{
			try
			{
				var users = await _entityCreationService.GetUsersListAsync();

				if (users == null)
				{
					return NotFound(new { message = "Users not found." });
				}

				return Ok(new
				{
					success = true,
					data = users
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error occurred while retrieving request." });
			}
		}
		[HttpGet("GetUserById")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> GetUserById(int userId)
		{
			try
			{
				var users = await _entityCreationService.GetUserByIdAsync(userId);

				if (users == null)
				{
					return NotFound(new { message = "User request not found." });
				}

				return Ok(new
				{
					success = true,
					data = users
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error occurred while retrieving request." });
			}
		}

		[HttpPut("UpdateUser")]
		[Authorize]
		[ServiceFilter(typeof(UserHistoryActionFilter))]
		public async Task<IActionResult> UpdateUserInfo(int userId, [FromBody] UserDetails userDetails)
		{
			try
			{
				// Validate the request model
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Update user information
				var result = await _entityCreationService.UpdateUserAsync(userId, userDetails);

				// Handle failed update
				if (!result.Success)
				{
					return BadRequest(new { message = result.Message });
				}
				return Ok(new { message = result.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error:{ex.InnerException}");
			}
		}
		/// <summary>
		/// Endpoint for refreshing tokens.
		/// </summary>
		/// <param name="refreshTokenRequest">The refresh token request data.</param>
		/// <returns>Returns an action result with the new access and refresh tokens.</returns>
		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken([FromBody] RefTokenRequest refreshTokenRequest)
		{
			// Validate the refresh token request
			if (refreshTokenRequest == null || string.IsNullOrEmpty(refreshTokenRequest.RefreshToken))
			{
				return BadRequest(new { message = "Refresh token is required." });
			}

			try
			{
				// Use the JwtTokenService to refresh the token
				var response = await _jwtTokenService.RefreshToken(refreshTokenRequest.RefreshToken);

				// Return the new access token and refresh token
				return Ok(new
				{
					AccessToken = response.AccessToken,
					RefreshToken = response.RefreshToken
				});
			}
			catch (SecurityTokenException)
			{
				return Unauthorized(new { message = "Invalid or expired refresh token." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while refreshing the token.", error = ex.Message });
			}
		}

		// Endpoint for MFA code validation
		/// <summary>
		/// Endpoint for MFA code validation.
		/// </summary>
		/// <param name="mfaRequest">The MFA validation request data.</param>
		/// <returns>Returns an action result indicating the validation outcome.</returns>
		[HttpPost("validate-mfa")]
		//[EnableRateLimiting("LoginLimit")]
		public async Task<IActionResult> ValidateMfa([FromBody] MfaValidationRequest mfaRequest)
		{
			// Validate the request model state
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			// Attempt to validate the MFA code
			var result = await _mfaService.ValidateMfaAsync(mfaRequest.UserId, mfaRequest.MfaCode);
			// Check if validation was unsuccessful
			if (!result.Success)
			{
				return Unauthorized(new { message = result.Message });
			}

			// If MFA is successfully validated, return JWT token
			return Ok(new { Success = true, Acctoken = result.AccessToken, RefToken = result.RefreshToken });
		}

		/// <summary>
		/// Endpoint for password reset.
		/// </summary>
		/// <param name="loginreq">The login request data for password reset.</param>
		/// <returns>Returns an action result indicating the reset outcome.</returns>
		[HttpPost("reset-password")]
		[EnableRateLimiting("LoginLimit")]
		public async Task<IActionResult> PasswordReset(string userNameOrEmail)
		{
			// Validate the request model state
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			// Attempt to reset the password
			var result = await _userService.PasswordResetAsync(userNameOrEmail);

			if (!result.Success)
			{
				return Unauthorized(new { message = result.Message });
			}

			return Ok(new { UserId = result.UserId, message = result.Message });
		}


		/// <summary>
		/// Endpoint for setting a new password.
		/// </summary>
		/// <param name="passwordreset">The new password request data.</param>
		/// <returns>Returns an action result indicating the outcome.</returns>
		[HttpPost("new-password")]
		public async Task<IActionResult> SetnewPassword([FromBody] PasswordReset passwordreset)
		{
			// Validate the request model state
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			// Attempt to store the new password
			var result = await _userService.StoreNewpasswordAsync(passwordreset);

			if (!result.Success)
			{
				return Unauthorized(new { message = result.Message });
			}

			return Ok(new { message = result.Message });
		}

		/// <summary>
		/// Endpoint for locking a user account.
		/// </summary>
		/// <param name="loginRequest">The login request data for locking the account.</param>
		/// <returns>Returns an action result indicating the locking outcome.</returns>
		[HttpPost("lock-account")]
		public async Task<IActionResult> LockAccount([FromBody] RequestLogin loginRequest)
		{
			// Attempt to lock the account
			var result = await _userService.LockAccountAsync(loginRequest);
			// Check if the account was successfully locked
			if (result)
			{
				return Ok(new { Success = true, Message = "Account has been locked successfully." });
			}
			return BadRequest(new { Success = false, Message = "Unable to lock account. User may not exist." });
		}

		/// <summary>
		/// Endpoint for unlocking a user account.
		/// </summary>
		/// <param name="loginRequest">The login request data for unlocking the account.</param>
		/// <returns>Returns an action result indicating the unlocking outcome.</returns>
		[HttpPost("unlock-account")]
		public async Task<IActionResult> UnlockAccount([FromBody] RequestLogin loginRequest)
		{
			// Attempt to unlock the account
			var result = await _userService.UnlockAccountAsync(loginRequest);
			// Check if the account was successfully unlocked
			if (result)
			{
				return Ok(new { Success = true, Message = "Account has been locked successfully." });
			}
			return BadRequest(new { Success = false, Message = "Unable to lock account. User may not exist." });
		}

		/// <summary>
		/// Verifies the CAPTCHA response from the client.
		/// </summary>
		/// <param name="request">The request containing the CAPTCHA response.</param>
		/// <returns>An IActionResult indicating the result of the CAPTCHA verification.</returns>
		[HttpPost("verify-captcha")]
		public async Task<IActionResult> VerifyCaptcha([FromBody] CaptchaRequest request)
		{
			// Verify the CAPTCHA response using the user service
			CaptchaResponse captchaResponse = await _userService.VerifyCaptcha(request);

			// Check if the CAPTCHA verification was successful
			if (captchaResponse.Success)
			{
				return Ok(new { message = "Captcha verified successfully" });
			}

			return BadRequest(new
			{
				message = "Captcha verification failed",
				errors = captchaResponse.ErrorCodes
			});
		}


		[HttpGet("GetUserRoles")]
		public async Task<IActionResult> GetUserRolesDetails()
		{
			try
			{
				var roles = await _entityCreationService.GetUserRolesAsync();

				if (roles == null)
				{
					return NotFound(new { message = "User request not found." });
				}

				return Ok(roles);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error occurred while retrieving request." });
			}
		}

        [HttpGet("GetDepartments")]
		public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _entityCreationService.GetListofDepartmentsAsync();

                if (departments == null)
                {
                    return NotFound(new { message = "No any department found." });
                }

                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error occurred while retrieving request." });
            }
        }
        private int? GetCurrentUserId()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (int.TryParse(userIdClaim, out int userId))
			{
				return userId;
			}
			return null;
		}

	}

	public class GMLoginRequest
	{
		public string IdToken { get; set; }
	}
	public class RefTokenRequest
	{
		public string RefreshToken { get; set; }
	}

}
