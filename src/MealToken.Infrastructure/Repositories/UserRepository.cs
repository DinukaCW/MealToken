using Authentication.Interfaces;
using Authentication.Models.Entities;
using MealToken.Application.Interfaces;
using MealToken.Application.Services;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Infrastructure.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly MealTokenDbContext _tenantContext;
		private readonly PlatformDbContext _platformContext;
		private readonly ITenantContext _currentTenant;
		private readonly ILogger<UserRepository> _logger;

		public UserRepository(
			MealTokenDbContext tenantContext,
			PlatformDbContext platformContext,
			ITenantContext currentTenant,
			ILogger<UserRepository> logger)
		{
			_tenantContext = tenantContext;
			_platformContext = platformContext;
			_currentTenant = currentTenant;
			_logger = logger;
		}

		// User CRUD operations
		public async Task<User> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
		{
			return await _tenantContext.Users
				.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
		}

		public async Task<List<User>> GetAllUsersAsync()
		{
			return await _tenantContext.Users
				.Where(u => u.IsActive)
				.OrderBy(u => u.FullName)
				.ToListAsync();
		}

		public async Task<User> GetUserByIdAsync(int userId)
		{
			return await _tenantContext.Users.FindAsync(userId);
		}

		public async Task CreateUserAsync(User user)
		{
			user.CreatedAt = DateTime.UtcNow;
			await _tenantContext.Users.AddAsync(user);
			await _tenantContext.SaveChangesAsync();

			_logger.LogInformation($"User created: {user.Username} for tenant: {_currentTenant.TenantId}");
		}

		public async Task UpdateUserAsync(User user)
		{
			_tenantContext.Users.Update(user);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task<User> CheckUserByUsernameOrEmailAsync(string userName, string email)
		{
			return await _tenantContext.Users
				.FirstOrDefaultAsync(u =>u.Username == userName || u.Email == email);
		}
        
        public async Task<User> CheckUserByUsernameOrEmailExceptIdAsync(int userId, string userName, string email)
		{
			return await _tenantContext.Users
				.FirstOrDefaultAsync(u => u.UserID != userId && (u.Username == userName || u.Email == email));
		}

		public async Task UpdateLastLoginAsync(int userId)
		{
			var user = await _tenantContext.Users.FindAsync(userId);
			if (user != null)
			{
				user.LastLoginAt = DateTime.UtcNow;
				user.LoginCount++;
				await _tenantContext.SaveChangesAsync();
			}
		}

		// UserToken operations
		public async Task AddUserTokenAsync(UserToken userToken)
		{
			userToken.CreatedAt = DateTime.UtcNow;
			await _tenantContext.UserToken.AddAsync(userToken);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task<UserToken> GetUserTokenAsync(int userId, string hashedenteredMfa)
		{
			return await _tenantContext.UserToken
				.FirstOrDefaultAsync(ut => ut.UserID == userId &&
										  ut.Token == hashedenteredMfa &&
										  ut.TokenType == "MFA" &&
										  (ut.IsUsed == false) &&
										  ut.ExpiresAt >= DateTime.UtcNow);
		}

		public async Task<UserToken> GetUserTokenAsync(string hashedRefToken)
		{
			return await _tenantContext.UserToken
				.FirstOrDefaultAsync(ut => ut.Token == hashedRefToken &&
										  ut.TokenType == "RefreshToken" &&
										  (ut.IsUsed == false) &&
										  ut.ExpiresAt >= DateTime.UtcNow);
		}

		public async Task UpdateUserTokenAsync(UserToken userToken)
		{
			_tenantContext.UserToken.Update(userToken);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task UpdateUserTokenUsedAsync(string hashedRefToken)
		{
			var token = await _tenantContext.UserToken
			.FirstOrDefaultAsync(ut => ut.Token == hashedRefToken &&
							   ut.TokenType == "RefreshToken" &&
							   (ut.IsUsed == false) &&
							   (ut.IsRevoked == false));

			if (token != null)
			{
				token.IsUsed = true;
				token.LastUsedAt = DateTime.UtcNow;
				await _tenantContext.SaveChangesAsync();
			}
		}

		// MFA operations
		public async Task<MfaSetting> GetMFATypeAsync(int userId)
		{
			return await _tenantContext.MfaSetting
				.FirstOrDefaultAsync(m => m.UserID == userId);
		}

		public async Task<MfaSetting> GetMfaSettingByIdAsync(int userId)
		{
			return await _tenantContext.MfaSetting
				.FirstOrDefaultAsync(m => m.UserID == userId);
		}

		public async Task AddMfaSettingAsync(MfaSetting mfaSetting)
		{
			await _tenantContext.MfaSetting.AddAsync(mfaSetting);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task UpdateMfaTypeAsync(MfaSetting mfaSetting)
		{
			_tenantContext.MfaSetting.Update(mfaSetting);
			await _tenantContext.SaveChangesAsync();
		}

		// Login tracking
		public async Task AddLoginTrackAsync(LoginTrack loginTrack)
		{
			try
			{
				loginTrack.LoginTime = DateTime.UtcNow;
				await _tenantContext.LoginTrack.AddAsync(loginTrack);
				await _tenantContext.SaveChangesAsync();

				_logger.LogInformation($"Login tracked for user: {loginTrack.UserID} in tenant: {_currentTenant.TenantId}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error tracking login for tenant: {_currentTenant.TenantId}");
				throw;
			}
		}

		// External login operations
		public async Task AddExternalLoginAsync(ExternalLogin externalLogin)
		{
			await _tenantContext.ExternalLogin.AddAsync(externalLogin);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task<ExternalLogin> CheckExLoginAsync(User user, string hashedproviderKey)
		{
			return await _tenantContext.ExternalLogin
				.FirstOrDefaultAsync(el => el.UserID == user.UserID && el.ProviderKey == hashedproviderKey);
		}
		public async Task AddSendTokenAsync(SendToken sendToken)
		{
			sendToken.SendAt = DateTime.UtcNow;
			await _tenantContext.SendToken.AddAsync(sendToken);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task AddNotificatonLogAsync(SentNotification log)
		{
			await _tenantContext.SentNotification.AddAsync(log);
			await _tenantContext.SaveChangesAsync();
		}

		// User role operations
		public async Task<string> GetUserRoleNameAsync(int userRoleId)
		{
			var userRole = await _platformContext.UserRole.FindAsync(userRoleId);
			return userRole?.UserRoleName ?? "Unknown";
		}

		// Platform operations (use PlatformDbContext)
		public async Task<Message> GetMessageAsync(string messageName)
		{
			return await _platformContext.Message
				.FirstOrDefaultAsync(m => m.MessageName == messageName);
		}

		public async Task CreateUserRequestAsync(UserRequest userRequest)
		{
			await _tenantContext.UserRequest.AddAsync(userRequest);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task<UserRequest> GetUserRequestByIdAsync(int requestId)
		{
			return await _tenantContext.UserRequest.FindAsync(requestId);
		}
		public async Task<List<UserRequest>> GetPendingRequestsAsync()
		{

			var pendingRequests = await _tenantContext.Set<UserRequest>()
				.Where(ur => ur.Status == UserRequestStatus.Pending)
				.OrderBy(ur => ur.CreatedAt)
				.ToListAsync();
			// Token and notification operations

			return pendingRequests;
		}
		public async Task<UserRequest> CheckPendingRequestAsync(string username, string email)
		{
			var existingRequest = await _tenantContext.Set<UserRequest>()
				.FirstOrDefaultAsync(ur =>
					(ur.Username == username || ur.Email == email) &&
					ur.Status == UserRequestStatus.Pending);
			return existingRequest;
		}
		public async Task UpdateRequestStatusAsync(int requestId, UserRequestStatus status, int reviewerId, string? comments, string? rejectReason)
		{
				var userRequest = await _tenantContext.Set<UserRequest>()
					.FirstOrDefaultAsync(ur => ur.UserRequestId == requestId);

				if (userRequest == null)
				{ 
					throw new InvalidOperationException($"User request with ID {requestId} not found");
				}

				userRequest.Status = status;
				userRequest.ReviewedBy = reviewerId;
				userRequest.ReviewedAt = DateTime.UtcNow;
				userRequest.RejectionReason = rejectReason;

			if (status == UserRequestStatus.Approved)
				{
					userRequest.ReviewComments = comments;
				}
				else if (status == UserRequestStatus.Rejected)
				{
					userRequest.RejectionReason = comments;
				}

				_tenantContext.Set<UserRequest>().Update(userRequest);
				await _tenantContext.SaveChangesAsync();
			
		}

		public async Task<List<UserRole>> GetUserRolesAsync()
		{
			return await _platformContext.UserRole
				.Where(d => d.IsActive)
				.ToListAsync();
		}
        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            return await _platformContext.Department
                .Where(d => d.IsActive)
                .ToListAsync();
        }
        public async Task<List<User>> GetAdminUsersAsync()
		{
			return await _tenantContext.Users
				.Where(u => u.UserRoleId == 1) 
				.Where(d => d.IsActive)
				.ToListAsync();
		}
        public async Task AddUserDepartmentsAync(IEnumerable<UserDepartment> departments)
        {
            await _tenantContext.UserDepartment.AddRangeAsync(departments);
            await _tenantContext.SaveChangesAsync();
        }
        public async Task<List<UserDepartment>> GetUserDepartmentsbyRequestAsync(int requestId)
        {
            return await _tenantContext.UserDepartment
                .Where(u => u.UserRequestId == requestId && u.RequestStatus == UserRequestStatus.Pending)
                .ToListAsync();
        }
        public async Task UpdateUserDepartmentsAync(IEnumerable<UserDepartment> departments)
        {
          _tenantContext.UserDepartment.UpdateRange(departments);
           await _tenantContext.SaveChangesAsync();
        }
        public async Task<List<int>> GetRequestsbByDepartmentsAsync(List<int> departmentIds)
        {
            return await _tenantContext.UserDepartment
                .Where(p => departmentIds.Contains(p.DepartmentId) && p.RequestStatus == UserRequestStatus.Pending)
				.Select(p => p.UserRequestId)
                .Distinct()
                .ToListAsync();
        }
        public async Task<List<int?>> GetUsersByDepartmentsAsync(List<int> departmentIds)
        {
            return await _tenantContext.UserDepartment
                .Where(p => departmentIds.Contains(p.DepartmentId) && p.RequestStatus == UserRequestStatus.Approved)
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync();
        }
        public async Task DeleteUserDepartmentAsync(IEnumerable<UserDepartment> departments)
        {

            _tenantContext.UserDepartment.RemoveRange(departments);
            await _tenantContext.SaveChangesAsync();

        }
        public async Task<List<UserDepartment>> GetUserDepartmentsbyUserAsync(int userId)
        {
            return await _tenantContext.UserDepartment
                .Where(u => u.UserId == userId && u.RequestStatus == UserRequestStatus.Approved)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetUserIdsByDepartmentsAsync(IEnumerable<int> departmentIds)
        {
            if (departmentIds == null || !departmentIds.Any())
                return Enumerable.Empty<int>();

            var deptList = departmentIds.Distinct().ToList();

            var userIds = await _tenantContext.UserDepartment
                .Where(u => deptList.Contains(u.DepartmentId) && u.UserId != null)
                .Select(u => u.UserId.Value)
                .Distinct()
                .ToListAsync();

            return userIds;
        }
        public async Task<IEnumerable<(int UserId, string UserName)>> GetUserNamesByIdsAsync(IEnumerable<int> userIds)
        {
			if (userIds == null || !userIds.Any())
                    return new List<(int, string)>();

                var idList = userIds.Distinct().ToList();
                return await _tenantContext.Users
                    .Where(u => idList.Contains(u.UserID))
                    .Select(u => new ValueTuple<int, string>(u.UserID, u.FullName))
                    .ToListAsync();

        }
		public async Task AddUserHistorysAync(UserHistory userHistory)
		{
			await _tenantContext.UserHistory.AddAsync(userHistory);
			await _tenantContext.SaveChangesAsync();
		}
	}
}
