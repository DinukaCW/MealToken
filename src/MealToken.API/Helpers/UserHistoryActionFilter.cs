using MealToken.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace MealToken.API.Helpers
{
	public class UserHistoryActionFilter : IAsyncActionFilter
	{
		private readonly ILogger<UserHistoryActionFilter> _logger;
		private readonly IUserHistoryService _userHistoryService;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IUserContext _userContext;

		public UserHistoryActionFilter(
			ILogger<UserHistoryActionFilter> logger,
			IUserHistoryService userHistoryService,
			IHttpContextAccessor httpContextAccessor,
			IUserContext userContext)
		{
			_logger = logger;
			_userHistoryService = userHistoryService;
			_httpContextAccessor = httpContextAccessor;
			_userContext = userContext;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var resultContext = await next(); // Execute the action first

			// Only log if the main action succeeded and user is authenticated
			if (resultContext.Exception == null && context.HttpContext.User.Identity?.IsAuthenticated == true)
			{
				// Fire and forget - don't block the main response
				_ = Task.Run(async () =>
				{
					try
					{
						if (!_userContext.UserId.HasValue || _userContext.UserId.Value <= 0)
						{
							_logger.LogWarning("Skipping user history log - UserId is null or invalid");
							return;
						}

						var userId = _userContext.UserId.Value;
						var ipAddress = GetClientIpAddress();
						var endpoint = context.HttpContext.Request.Path.ToString();
						var actionType = GetActionTypeFromMethod(context.HttpContext.Request.Method);
						var entityType = GetEntityTypeFromController(context.Controller);

						_logger.LogDebug("User history logging - UserId: {UserId}, ActionType: {ActionType}, " +
							"EntityType: {EntityType}, Endpoint: {Endpoint}",
							userId, actionType, entityType, endpoint);

						// Only log if we have valid data
						if (userId > 0 && !string.IsNullOrEmpty(actionType) && !string.IsNullOrEmpty(entityType))
						{
							await _userHistoryService.LogUserActionAsync(
								userId,
								actionType,
								entityType,
								endpoint,
								ipAddress);
						}
						else
						{
							_logger.LogWarning("Skipping user history log due to invalid data - " +
								"UserId: {UserId}, ActionType: {ActionType}, EntityType: {EntityType}",
								userId, actionType, entityType);
						}
					}
					catch (Exception ex)
					{
						// Log but don't throw - we don't want to affect the main response
						_logger.LogError(ex, "Error occurred while logging user history in background task");
					}
				});
			}
			else if (resultContext.Exception != null)
			{
				_logger.LogWarning("Skipping user history log - Action failed with exception: {Exception}",
					resultContext.Exception.GetType().Name);
			}
		}

		private string GetActionTypeFromMethod(string method)
		{
			return method?.ToUpper() switch
			{
				"POST" => "Add",
				"PUT" => "Update",
				"PATCH" => "Update",
				"GET" => "View",
				"DELETE" => "Delete",
				_ => "Unknown"
			};
		}

		private string GetEntityTypeFromController(object controller)
		{
			try
			{
				var controllerName = controller?.GetType().Name.Replace("Controller", "").Trim();
				return controllerName switch
				{
					"User" => "User",
					"Business" => "Business",
					"Report" => "Report",
					"Admin" => "Admin",
					_ => "General"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error extracting entity type from controller");
				return "General";
			}
		}

		private string GetClientIpAddress()
		{
			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext == null)
				return "Unknown";

			// Check for X-Forwarded-For header (for proxy/load balancer scenarios)
			if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
			{
				var ipAddress = forwardedFor.ToString().Split(',')[0].Trim();
				if (!string.IsNullOrEmpty(ipAddress))
					return ipAddress;
			}

			// Fallback to direct connection
			return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
		}
	}
}
