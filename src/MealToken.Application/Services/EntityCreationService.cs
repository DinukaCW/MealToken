using Authentication.Interfaces;
using Authentication.Models.DTOs;
using Authentication.Models.Entities;
using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Services
{
	public class EntityCreationService : IEntityCreationService
	{
		private readonly IEncryptionService _encryption;
		private readonly IUserRepository _userData;
		private readonly ILogger<EntityCreationService> _logger;
		private readonly ITenantContext _tenantContext;
		private readonly IEmailNotification _emailNotification;
		private readonly IMessageService _messageService;
		private readonly IUserContext _userContext;
		public EntityCreationService(IEncryptionService encryptionService, IUserRepository userData, ILogger<EntityCreationService> logger, ITenantContext tenantContext, IEmailNotification emailNotification, IMessageService messageService, IUserContext userContext) 
		{ 
			_encryption = encryptionService;
			_userData = userData;
			_logger = logger;
			_tenantContext = tenantContext;
			_emailNotification = emailNotification;
			_messageService = messageService;
			_userContext = userContext;
		}
		// Submit user account creation request
		public async Task<ServiceResult> SubmitUserRequestAsync(UserDetails userDetails)
		{
			try
			{
				_logger.LogInformation("Submitting user request: {Username}", userDetails.Username);

				// Check for existing user
				var encryptedUsername = _encryption.EncryptData(userDetails.Username);
				var encryptedEmail = _encryption.EncryptData(userDetails.Email);

				var existingUser = await _userData.CheckUserByUsernameOrEmailAsync(encryptedUsername, encryptedEmail);
				if (existingUser != null)
				{
					_logger.LogWarning("User already exists: {Username}", userDetails.Username);
					return new ServiceResult
					{
						Success = false,
						Message = "Username or Email already exists."
					};
				}

				// Check for existing pending request
				var existingRequest = await _userData.CheckPendingRequestAsync(encryptedUsername, encryptedEmail);
				if (existingRequest != null)
				{
					_logger.LogWarning("Pending request already exists: {Username}", userDetails.Username);
					return new ServiceResult
					{
						Success = false,
						Message = "A pending request already exists for this username or email."
					};
				}

				// Validate password match
				if (userDetails.Password != userDetails.ReEnteredPassword)
				{
					_logger.LogWarning("Password mismatch for request: {Username}", userDetails.Username);
					return new ServiceResult
					{
						Success = false,
						Message = "Passwords do not match."
					};
				}

				// Create user request
				var userRequest = new UserRequest
				{
					TenantId = _tenantContext.TenantId.Value,
					Username = encryptedUsername,
					FullName = userDetails.FullName,
					PasswordHash = _encryption.EncryptData(userDetails.Password),
					UserRoleId = userDetails.UserRoleId,
					Email = encryptedEmail,
					PhoneNumber = _encryption.EncryptData(userDetails.PhoneNumber),
					Status = UserRequestStatus.Pending,
					CreatedAt = DateTime.UtcNow
				};

				await _userData.CreateUserRequestAsync(userRequest);
				
				var userDepartments = new List<UserDepartment>();
                foreach (var deptId in userDetails.DepartmentIds.Distinct())
				{
					userDepartments.Add(new UserDepartment
					{
						TenantId = userRequest.TenantId,
						UserRequestId = userRequest.UserRequestId,
						DepartmentId = deptId,
						RequestStatus = UserRequestStatus.Pending
					});
                }
				await _userData.AddUserDepartmentsAync(userDepartments);
				
                string userROle = await _userData.GetUserRoleNameAsync(userRequest.UserRoleId);

				// Get admin users and send email notifications
				var admins = await _userData.GetAdminUsersAsync();
				var userIds = await _userData.GetUsersByDepartmentsAsync(userDetails.DepartmentIds);
                var filteredAdmins = admins
					 .Where(admin => userIds.Contains(admin.UserID))
					  .ToList();
                if (filteredAdmins.Any())
				{
					// Extract admin email addresses (decrypt if they're encrypted)
					var adminEmails = new List<string>();
					foreach (var admin in filteredAdmins)
					{
						// Assuming admin.Email is encrypted, decrypt it
						var decryptedEmail = _encryption.DecryptData(admin.Email);
						if (!string.IsNullOrEmpty(decryptedEmail))
						{
							adminEmails.Add(decryptedEmail);
						}
					}

					if (adminEmails.Any())
					{
						// Create notification request for email
						var notificationRequest = new NotificationRequest
						{
							Emails = adminEmails,
							Subject = "New User Account Request - Approval Required",
							Message = _messageService.GenerateRequestMessage(userDetails,userRequest,userROle),
							NotificationTypes = new List<NotificationRequest.NotificationType>
							{
								NotificationRequest.NotificationType.Email
							}
						};

						// Send email notification
						var emailResult = await _emailNotification.SendEmail(notificationRequest);

						if (emailResult.IsSuccess)
						{
							_logger.LogInformation("Email notifications sent successfully for user request {RequestId}", userRequest.UserRequestId);
						}
						else
						{
							_logger.LogWarning("Failed to send email notifications for user request {RequestId}: {Error}",
								userRequest.UserRequestId, emailResult.ErrorMessages);
						}
					}
					else
					{
						_logger.LogWarning("No valid admin email addresses found for notifications");
					}
				}
				else
				{
					_logger.LogWarning("No admin users found to notify about user request {RequestId}", userRequest.UserRequestId);
				}

				_logger.LogInformation("User request submitted: {RequestId}", userRequest.UserRequestId);
				return new ServiceResult
				{
					Success = true,
					Message = "User account request submitted successfully. Please wait for approval."
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "User request submission error: {Username}", userDetails.Username);
				return new ServiceResult
				{
					Success = false,
					Message = "Request submission error. Please try again."
				};
			}
		}

		// Get pending user requests for admin review
		public async Task<List<UserRequestDto>> GetPendingUserRequestsAsync()
		{
			try
			{
				_logger.LogInformation("Get pending user requests");
				var pendingRequests = await _userData.GetPendingRequestsAsync();
				if (pendingRequests == null || !pendingRequests.Any())
				{
					_logger.LogInformation("No pending user requests found");
					return new List<UserRequestDto>();
				}
				var requestIds = await _userData.GetRequestsbByDepartmentsAsync(_userContext.DepartmentIds);
				if (requestIds == null || !requestIds.Any())
				{
                    _logger.LogInformation("No requests found for the user's departments.");
                    return new List<UserRequestDto>();
                }
                var filteredPendingRequests = pendingRequests
				  .Where(pr => requestIds.Contains(pr.UserRequestId)) // Assuming pr also has a UserRequestId
				 .ToList();
				var departmentsList =await _userData.GetAllDepartmentsAsync();
                var requestDtos = new List<UserRequestDto>();
                foreach (var request in filteredPendingRequests)
                {
                    var userRole = await _userData.GetUserRoleNameAsync(request.UserRoleId);
                    var departments = await _userData.GetUserDepartmentsbyRequestAsync(request.UserRequestId);

					var departmentDtos = departments.Select(d => new DepartmentD
					{
						DepartmentId = d.DepartmentId,
                        Name = departmentsList
								.FirstOrDefault(x => x.DepartmnetId == d.DepartmentId)?.Name
                    }).ToList();

                    requestDtos.Add(new UserRequestDto
                    {
                        RequestId = request.UserRequestId,
                        FullName = request.FullName,
                        UserRoleId = request.UserRoleId,
                        UserRole = userRole,
                        Departments = departmentDtos
                    });
                }

                return requestDtos;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving pending requests");
				return new List<UserRequestDto>(); // Fixed return type
			}
		}

		// Approve user request and create account
		public async Task<ServiceResult> ApproveUserRequestAsync(int reviewerId, ApproveRequestModel model)
		{
			try
			{
				_logger.LogInformation("Approving user request: {RequestId} by {ReviewerId}", model.RequestId, reviewerId);

				var userRequest = await _userData.GetUserRequestByIdAsync(model.RequestId);
				if (userRequest == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "User request not found."
					};
				}

				if (userRequest.Status != UserRequestStatus.Pending)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Request has already been processed."
					};
				}

				// Check if user still doesn't exist (double-check)
				var existingUser = await _userData.CheckUserByUsernameOrEmailAsync(userRequest.Username, userRequest.Email);
				if (existingUser != null)
				{
					// Update request status to rejected
					await _userData.UpdateRequestStatusAsync(model.RequestId, UserRequestStatus.Approved, reviewerId,model.Comments,"User already exists in system");

					return new ServiceResult
					{
						Success = false,
						Message = "User already exists in the system."
					};
				}

				// Create the user account
				var user = new User
				{
					TenantId = userRequest.TenantId,
					Username = userRequest.Username,
					FullName = userRequest.FullName,
					PasswordHash = userRequest.PasswordHash,
					UserRoleId = model.UserRoleId ?? userRequest.UserRoleId,
					Email = userRequest.Email,
					PhoneNumber = userRequest.PhoneNumber,
					IsActive = true,
					CreatedAt = DateTime.UtcNow
				};
				await _userData.CreateUserAsync(user);

				var mfa = new MfaSetting
				{
					TenantId = user.TenantId,
					UserID = user.UserID,
					IsMFAEnabled = true,
					PreferredMFAType = "Email",

				};
                var existingDepartments = await _userData.GetUserDepartmentsbyRequestAsync(userRequest.UserRequestId);
                var modelDepartmentIds = model.Departments;

                var toUpdate = new List<UserDepartment>();
                var toDelete = new List<UserDepartment>();

                foreach (var dept in existingDepartments)
                {
                    if (modelDepartmentIds.Contains(dept.DepartmentId))
                    {
                        dept.UserId = user.UserID;
                        dept.RequestStatus = UserRequestStatus.Approved;
                        toUpdate.Add(dept);
                    }
                    else
                    {
                        toDelete.Add(dept);
                    }
                }
                var existingDeptIds = existingDepartments.Select(d => d.DepartmentId).ToList();
                var newDeptIds = modelDepartmentIds.Except(existingDeptIds).ToList();

                var newDepartments = newDeptIds.Select(deptId => new UserDepartment
                {
                    TenantId = userRequest.TenantId,
                    UserRequestId = userRequest.UserRequestId,
                    UserId = user.UserID,
                    DepartmentId = deptId,
                    RequestStatus = UserRequestStatus.Approved,
                }).ToList();

                // Step 3: Apply changes efficiently
                if (toDelete.Any())
                    await _userData.DeleteUserDepartmentAsync(toDelete); // bulk delete

                if (newDepartments.Any())
                    await _userData.AddUserDepartmentsAync(newDepartments);

                if (toUpdate.Any())
                    await _userData.UpdateUserDepartmentsAync(toUpdate);

                await _userData.AddMfaSettingAsync(mfa);
				// Update request status to approved
				await _userData.UpdateRequestStatusAsync(model.RequestId, UserRequestStatus.Approved, reviewerId, model.Comments,null); 
				// Send approval email to the requester
				await SendApprovalEmail(userRequest, model.Comments);
				_logger.LogInformation("User request approved and account created: {UserId}", user.UserID);

				return new ServiceResult
				{
					Success = true,
					Message = "User request approved and account created successfully.",
					ObjectId = user.UserID
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "User request approval error: {RequestId}", model.RequestId);
				return new ServiceResult
				{
					Success = false,
					Message = "Request approval error. Please try again."
				};
			}
		}

		// Reject user request
		public async Task<ServiceResult> RejectUserRequestAsync(int requestId, int reviewerId, string? rejectionReason)
		{
			try
			{
				_logger.LogInformation("Rejecting user request: {RequestId} by {ReviewerId}", requestId, reviewerId);

				var userRequest = await _userData.GetUserRequestByIdAsync(requestId);
				if (userRequest == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "User request not found."
					};
				}

				if (userRequest.Status != UserRequestStatus.Pending)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Request has already been processed."
					};
				}

				await _userData.UpdateRequestStatusAsync(requestId, UserRequestStatus.Rejected, reviewerId, null, rejectionReason);
				// Send rejection email to the requester
				await SendRejectionEmail(userRequest, rejectionReason);
				_logger.LogInformation("User request rejected: {RequestId}", requestId);

				return new ServiceResult
				{
					Success = true,
					Message = "User request rejected successfully."
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "User request rejection error: {RequestId}", requestId);
				return new ServiceResult
				{
					Success = false,
					Message = "Request rejection error. Please try again."
				};
			}
		}
		public async Task<UserRequest> GetUserRequestByIdAsync(int requestId)
		{
			try
			{
				_logger.LogInformation("Retrieving user request: {RequestId}", requestId);

				var userRequest = await _userData.GetUserRequestByIdAsync(requestId);

				if (userRequest == null)
				{
					_logger.LogWarning("User request not found: {RequestId}", requestId);
				}

				return userRequest;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving user request: {RequestId}", requestId);
				return null;
			}
		}

		// Simplified Service Method - Returns List<UserListDto>
		public async Task<List<UserListDto>> GetUsersListAsync()
		{
			try
			{
				_logger.LogInformation("Retrieving all users list");

				var users = await _userData.GetAllUsersAsync();

				// Handle null/empty case
				if (users == null || !users.Any())
				{
					_logger.LogInformation("No users found in the database");
					return new List<UserListDto>();
				}

				var userDtos = users.Select(u => new UserListDto
				{
					UserID = u.UserID,
					Username = _encryption.DecryptData(u.Username),
					FullName = u.FullName,
					Email = _encryption.DecryptData(u.Email),
					PhoneNumber = _encryption.DecryptData(u.PhoneNumber),
					IsActive = u.IsActive,
					UserRoleId = u.UserRoleId
				}).ToList();

				_logger.LogInformation("Retrieved {UserCount} users successfully", userDtos.Count);

				return userDtos;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving users list");
				// You can either return empty list or rethrow the exception
				return new List<UserListDto>();
				// OR rethrow: throw;
			}
		}

		public async Task<UserListDto?> GetUserByIdAsync(int userId)
		{
			try
			{
				_logger.LogInformation("Retrieving user by User Id: {UserId}", userId);

				var user = await _userData.GetUserByIdAsync(userId);

				if (user == null)
				{
					_logger.LogWarning("User not found with ID: {UserId}", userId);
					return null;
				}

				var userDto = new UserListDto
				{
					UserID = user.UserID,
					Username = _encryption.DecryptData(user.Username),
					FullName = user.FullName,
					Email = _encryption.DecryptData(user.Email),
					PhoneNumber = _encryption.DecryptData(user.PhoneNumber),
					IsActive = user.IsActive,
					UserRoleId = user.UserRoleId
				};

				_logger.LogInformation("User retrieved successfully: {UserId}", userId);
				return userDto;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving user with ID: {UserId}", userId);
				throw;
			}
		}

        public async Task<ServiceResult> UpdateUserAsync(int userId, UserDetails updateDto)
        {
            try
            {
                _logger.LogInformation("Starting update for user with ID: {UserId}", userId);

                var existingUser = await _userData.GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", userId);
                    return new ServiceResult { Success = false, Message = "User not found." };
                }

                // 1. Validate the update request
                if (string.IsNullOrWhiteSpace(updateDto.FullName))
                {
                    return new ServiceResult { Success = false, Message = "Full name is required." };
                }

                var encryptedNewEmail = _encryption.EncryptData(updateDto.Email);

                if (!string.Equals(existingUser.Email, encryptedNewEmail, StringComparison.OrdinalIgnoreCase))
                {
                    var userWithEmail = await _userData.CheckUserByUsernameOrEmailAsync(null, encryptedNewEmail);
                    if (userWithEmail != null && userWithEmail.UserID != userId)
                    {
                        _logger.LogWarning("Email '{Email}' is already in use by another user.", updateDto.Email);
                        return new ServiceResult { Success = false, Message = "Email address is already in use." };
                    }
                }

                // 2. Update the primary user properties
                existingUser.FullName = updateDto.FullName;
                existingUser.UserRoleId = updateDto.UserRoleId;
                existingUser.Email = _encryption.EncryptData(updateDto.Email);
                existingUser.PhoneNumber = _encryption.EncryptData(updateDto.PhoneNumber);
                existingUser.IsActive = updateDto.IsActive;

                await _userData.UpdateUserAsync(existingUser);

                // 3. Update user's department associations
                await UpdateUserDepartmentsAsync(userId, updateDto.DepartmentIds, existingUser.TenantId);

                _logger.LogInformation("Successfully updated user: {UserId}", userId);
                return new ServiceResult
                {
                    Success = true,
                    Message = "User updated successfully.",
                    ObjectId = userId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating user with ID: {UserId}", userId);
                return new ServiceResult { Success = false, Message = "An error occurred during the update process. Please try again." };
            }
        }

        /// <summary>
        /// Manages the additions and deletions of user department associations.
        /// </summary>
        private async Task UpdateUserDepartmentsAsync(int userId, List<int> newDepartmentIds, int tenantId)
        {
            var existingDepartments = await _userData.GetUserDepartmentsbyUserAsync(userId);
            var existingDepartmentIds = existingDepartments.Select(d => d.DepartmentId).ToList();

            // Determine which departments to remove
            var departmentsToDelete = existingDepartments
                .Where(d => !newDepartmentIds.Contains(d.DepartmentId))
                .ToList();
            var departmentIdsToAdd = newDepartmentIds.Except(existingDepartmentIds).ToList();

            int? userRequestId = existingDepartments.FirstOrDefault()?.UserRequestId;

            if (departmentIdsToAdd.Any() && userRequestId == null)
            {
                throw new InvalidOperationException("Cannot add new departments without a valid UserRequestId context.");
            }

            var departmentsToAdd = departmentIdsToAdd.Select(deptId => new UserDepartment
            {
                TenantId = tenantId,
                UserId = userId,
                DepartmentId = deptId,
                UserRequestId = userRequestId.Value, // Now safely accessed
                RequestStatus = UserRequestStatus.Approved,
            }).ToList();

            // Apply the changes to the database
            if (departmentsToDelete.Any())
            {
                await _userData.DeleteUserDepartmentAsync(departmentsToDelete);
            }

            if (departmentsToAdd.Any())
            {
                await _userData.AddUserDepartmentsAync(departmentsToAdd);
            }
        }

        public async Task<List<UserRoleDto>> GetUserRolesAsync()
		{
			try
			{
				_logger.LogInformation("Retrieving list of user roles");

				var userRoles = await _userData.GetUserRolesAsync();

				if (userRoles == null || !userRoles.Any())
				{
					_logger.LogInformation("No user roles found");
					return new List<UserRoleDto>();
				}

				var userRoleDetails = userRoles.Select(ur => new UserRoleDto
				{
					UserRoleId = ur.UserRoleID,
					RoleName = ur.UserRoleName
				}).ToList();

				_logger.LogInformation("Retrieved {UserRoleCount} user roles successfully", userRoleDetails.Count);

				return userRoleDetails;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving user roles list");
				throw;
			}
		}
		private async Task SendApprovalEmail(UserRequest userRequest, string? comments)
		{
			try
			{
				// Decrypt user email
				var recipientEmail = _encryption.DecryptData(userRequest.Email);
				var recipientName = userRequest.FullName;
				var username = _encryption.DecryptData(userRequest.Username);

				if (string.IsNullOrEmpty(recipientEmail))
				{
					_logger.LogWarning("Could not decrypt email for user request {RequestId}", userRequest.UserRequestId);
					return;
				}

				var notificationRequest = new NotificationRequest
				{
					Emails = new List<string> { recipientEmail },
					Subject = "Account Request Approved - Welcome!",
					Message = _messageService.GenerateApprovalMessage(recipientName,username,recipientEmail,comments),
					NotificationTypes = new List<NotificationRequest.NotificationType>
					{
						NotificationRequest.NotificationType.Email
					}
				};

				var emailResult = await _emailNotification.SendEmail(notificationRequest);

				if (emailResult.IsSuccess)
				{
					_logger.LogInformation("Approval email sent successfully for user request {RequestId}", userRequest.UserRequestId);
				}
				else
				{
					_logger.LogWarning("Failed to send approval email for user request {RequestId}: {Error}",
						userRequest.UserRequestId, emailResult.ErrorMessages);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending approval email for user request {RequestId}", userRequest.UserRequestId);
			}
		}

		// Helper method to send rejection email
		private async Task SendRejectionEmail(UserRequest userRequest, string rejectionReason)
		{
			try
			{
				// Decrypt user email
				var recipientEmail = _encryption.DecryptData(userRequest.Email);
				var recipientName = userRequest.FullName;
				var username = _encryption.DecryptData(userRequest.Username);

				if (string.IsNullOrEmpty(recipientEmail))
				{
					_logger.LogWarning("Could not decrypt email for user request {RequestId}", userRequest.UserRequestId);
					return;
				}

				var notificationRequest = new NotificationRequest
				{
					Emails = new List<string> { recipientEmail },
					Subject = "Account Request Update - Action Required",
					Message = _messageService.GenerateRejectionMessage(recipientName,username,recipientEmail,rejectionReason),
					NotificationTypes = new List<NotificationRequest.NotificationType>
					{
						NotificationRequest.NotificationType.Email
					}
				};

				var emailResult = await _emailNotification.SendEmail(notificationRequest);

				if (emailResult.IsSuccess)
				{
					_logger.LogInformation("Rejection email sent successfully for user request {RequestId}", userRequest.UserRequestId);
				}
				else
				{
					_logger.LogWarning("Failed to send rejection email for user request {RequestId}: {Error}",
					userRequest.UserRequestId, emailResult.ErrorMessages);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending rejection email for user request {RequestId}", userRequest.UserRequestId);
			}
		}
        public async Task<List<DepartmentD>> GetListofDepartmentsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving list of departments");

                var departments = await _userData.GetAllDepartmentsAsync();

                if (departments == null || !departments.Any())
                {
                    _logger.LogInformation("No departments found");
                    return new List<DepartmentD>();
                }

                var departmentReturn = departments.Select(d => new DepartmentD
                {
                    // Note: Check if "DepartmnetId" is a typo in your source entity. 
                    // It should likely be "DepartmentId".
                    DepartmentId = d.DepartmnetId,
                    Name = d.Name
                }).ToList();

                _logger.LogInformation("Retrieved {DepartmentCount} departments successfully", departmentReturn.Count);

                return departmentReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving list of departments");
                throw; // Re-throwing the exception is good practice to let higher-level handlers manage it.
            }
        }
    }
}

