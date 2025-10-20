using Authentication.Interfaces;
using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Services
{
	public class UserHistoryService : IUserHistoryService
	{
		private readonly IUserRepository _userData;
		private readonly ITenantContext _tenantContext;
		private readonly ILogger<UserHistoryService> _logger;

		public UserHistoryService(
			IUserRepository userData,
			ITenantContext tenantContext,
			ILogger<UserHistoryService> logger)
		{
			_userData = userData;
			_tenantContext = tenantContext;
			_logger = logger;
		}

		public async Task LogUserActionAsync(
			int userId,
			string actionType,
			string entityType,
			string endpoint,
			string ipAddress)
		{
			try
			{
				int tenantId = _tenantContext.TenantId.Value;

				var history = new UserHistory
				{
					UserId = userId,
					TenantId = tenantId,
					ActionType = actionType,
					EntityType = entityType,
					Endpoint = endpoint,
					Timestamp = DateTime.UtcNow,
					IPAddress = ipAddress
				};

				await _userData.AddUserHistorysAync(history);

				_logger.LogDebug("User action logged successfully - UserId: {UserId}, Action: {ActionType}, Entity: {EntityType}",
					userId, actionType, entityType);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error logging user action - UserId: {UserId}, ActionType: {ActionType}",
					userId, actionType);
				// Don't throw - we don't want to affect the main application flow
			}
		}
	}
}
