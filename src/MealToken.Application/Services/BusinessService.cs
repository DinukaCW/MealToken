using Authentication.Helpers;
using Authentication.Interfaces;
using Authentication.Models.DTOs;
using Authentication.Models.Entities;
using Authentication.Services.Notification;
using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Interfaces;
using MealToken.Domain.Models;
using MealToken.Domain.Models.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MealToken.Application.Services
{
	public class BusinessService : IBusinessService
	{
		private readonly IEncryptionService _encryption;
		private readonly IBusinessRepository _businessData;
		private readonly ILogger<BusinessService> _logger;
		private readonly ITenantContext _tenantContext;
		private readonly IAdminRepository _adminData;
		private readonly ICompanyBusinessLogic _companyBusinessLogic;
        private readonly IUserContext _userContext;
        private readonly IUserRepository _userData;
        private readonly IMessageService _messageService;
        private readonly IEmailNotification _emailNotification;

		public BusinessService(
			IEncryptionService encryptionService,
			IBusinessRepository businessRepository,
			ILogger<BusinessService> logger,
			ITenantContext tenantContext,
			IAdminRepository adminData,
			ICompanyBusinessLogic companyBusinessLogic,
            IUserContext userContext,
            IUserRepository userData,
            IMessageService messageService,
            IEmailNotification emailNotification)
		{
			_encryption = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
			_businessData = businessRepository ?? throw new ArgumentNullException(nameof(businessRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
			_adminData = adminData ?? throw new ArgumentNullException(nameof(adminData));
			_companyBusinessLogic = companyBusinessLogic;
            _userContext = userContext;
            _userData = userData;
            _messageService = messageService;
            _emailNotification = emailNotification;

		}

		public async Task<List<DeviceDto>> GetClientDeviceDetails(int clientId)
		{
			if (clientId <= 0)
			{
				throw new ArgumentException("Client ID must be positive", nameof(clientId));
			}

			try
			{
				// Fetch all client devices
				var clientDevices = await _businessData.GetClientDevicesAsync(clientId);

				if (clientDevices == null || !clientDevices.Any())
				{
					_logger.LogWarning("No ClientDevices found TenantId: {TenantId}",
						clientId, _tenantContext.TenantId);
					return new List<DeviceDto>();
				}

				// Filter out inactive devices (if required)
				var activeDevices = clientDevices.Where(d => d.IsActive).ToList();

				if (!activeDevices.Any())
				{
					_logger.LogWarning("All ClientDevices are inactive. TenantId: {TenantId}",
						clientId, _tenantContext.TenantId);
					return new List<DeviceDto>();
				}

				// Map ClientDevice → DeviceDto
				var result = activeDevices.Select(d => new DeviceDto
				{
					DeviceName = d.DeviceName,
					IpAddress = d.IpAddress,
					Port = d.Port,
					MachineNumber = d.MachineNumber,
					SerialNo = d.SerialNo,
					PrinterName = d.PrinterName,
					IsActive = d.IsActive,
					ReceiptHeightPixels = d.ReceiptHeightPixels,
					ReceiptWidthPixels = d.ReceiptWidthPixels
				}).ToList();

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving client device details. ClientId: {ClientId}", clientId);
				throw new ApplicationException("Failed to retrieve client device details", ex);
			}
		}


        public async Task<ServiceResult> CreateScheduleAsync(SheduleDTO scheduleDto)
        {
            _logger.LogInformation("Starting schedule creation. ScheduleName: {ScheduleName}", scheduleDto?.ScheduleName);

            // Track the created schedule entity so we can delete it if later steps fail.
            Schedule? createdSchedule = null;
            bool success = false;

            try
            {
                // 1. Initial Validation and Context Check
                var validationResult = await ValidateScheduleInput(scheduleDto);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                if (!_tenantContext.TenantId.HasValue)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Invalid tenant context"
                    };
                }

                // 2. Create Parent Entity
                createdSchedule = await CreateScheduleEntity(scheduleDto);

                // 3. Create Dates
                var dateResult = await CreateScheduleDates(createdSchedule, scheduleDto.ScheduleDates);
                if (!dateResult.Success)
                {
                    // Logging failure, cleanup will be done in the finally block
                    _logger.LogError("Failed to create schedule dates. ScheduleId: {ScheduleId}", createdSchedule.SheduleId);
                    return dateResult;
                }

                // 4. Create Meals
                var mealResult = await CreateScheduleMeals(createdSchedule, scheduleDto.MealTypes);
                if (!mealResult.Success)
                {
                    _logger.LogError("Failed to create schedule meals. ScheduleId: {ScheduleId}", createdSchedule.SheduleId);
                    return mealResult;
                }

                // 5. Validate and Assign Persons
                if (scheduleDto.AssignedPersonIds?.Any() == true)
                {
                    var conflictResult = await ValidateTokenTimeConflictsAsync(
                        scheduleDto.AssignedPersonIds,
                        scheduleDto.ScheduleDates,
                        scheduleDto.MealTypes);

                    if (!conflictResult.Success)
                    {
                        _logger.LogError("Conflict validation failed. ScheduleId: {ScheduleId}", createdSchedule.SheduleId);
                        return conflictResult;
                    }

                    var assignmentResult = await AssignPersonsToSchedule(createdSchedule, scheduleDto.AssignedPersonIds);
                    if (!assignmentResult.Success)
                    {
                        _logger.LogError("Failed to assign persons. ScheduleId: {ScheduleId}", createdSchedule.SheduleId);
                        return assignmentResult;
                    }
                }

                // 6. Final Success and Return
                success = true; // Mark the operation as fully successful

                _logger.LogInformation(
                    "Schedule created successfully. ScheduleId: {ScheduleId}, Name: {Name}",
                    createdSchedule.SheduleId, createdSchedule.SheduleName
                );

                return new ServiceResult
                {
                    Success = true,
                    Message = "Schedule created successfully",
                    ObjectId = createdSchedule.SheduleId,
                    Data = new
                    {
                        ScheduleId = createdSchedule.SheduleId,
                        ScheduleName = createdSchedule.SheduleName,
                        DatesCount = scheduleDto.ScheduleDates?.Count ?? 0,
                        MealTypesCount = scheduleDto.MealTypes?.Count ?? 0,
                        AssignedPersonsCount = scheduleDto.AssignedPersonIds?.Count ?? 0
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during schedule creation. ScheduleName: {ScheduleName}",
                    scheduleDto?.ScheduleName);
                return new ServiceResult
                {
                    Success = false,
                    Message = "An unexpected error occurred while creating the schedule."
                };
            }
            finally
            {
                
                if (!success && createdSchedule != null)
                {
                    _logger.LogWarning("Initiating rollback/cleanup for failed schedule creation. ScheduleId: {ScheduleId}", createdSchedule.SheduleId);

                    // Assuming DeleteScheduleAsync handles deleting the Schedule and all its related 
                    // entities (Dates, Meals, Persons, etc.) cascadingly.
                    await DeleteScheduleAsync(createdSchedule.SheduleId);

                    _logger.LogInformation("Rollback complete. Schedule {ScheduleId} deleted.", createdSchedule.SheduleId);
                }
            }
        }

        private async Task<ServiceResult> ValidateScheduleInput(SheduleDTO scheduleDto)
		{
			if (scheduleDto == null)
			{
				return new ServiceResult
				{
					Success = false,
					Message = "Schedule data cannot be null"
				};
			}

			if (string.IsNullOrWhiteSpace(scheduleDto.ScheduleName))
			{
				return new ServiceResult
				{
					Success = false,
					Message = "Schedule name is required"
				};
			}

			var existingSchedule = await _businessData.GetSheduleByNameAsync(scheduleDto.ScheduleName);
			if (existingSchedule != null)
			{
				return new ServiceResult
				{
					Success = false,
					Message = $"Schedule with name '{scheduleDto.ScheduleName}' already exists"
				};
			}

			return new ServiceResult { Success = true };
		}

		private async Task<Schedule> CreateScheduleEntity(SheduleDTO scheduleDto)
		{
			var schedule = new Schedule
			{
				TenantId = _tenantContext.TenantId.Value,
				SheduleName = scheduleDto.ScheduleName,
				ShedulePeriod = scheduleDto.SchedulePeriod,
				IsActive = true,
				Note = scheduleDto.Note
			};

			await _businessData.CreateSheduleAsync(schedule);
			return schedule;
		}

		private async Task<ServiceResult> CreateScheduleDates(Schedule schedule, List<DateOnly> dates)
		{
			if (dates == null || !dates.Any())
			{
				return new ServiceResult
				{
					Success = false,
					Message = "Schedule must have at least one date"
				};
			}

			var scheduleDates = dates.Select(date => new ScheduleDate
			{
				TenantId = _tenantContext.TenantId.Value,
				SheduleId = schedule.SheduleId,
				Date = date
			}).ToList();

			await _businessData.CreateSheduleDateAsync(scheduleDates);

			return new ServiceResult { Success = true };
		}

		private async Task<ServiceResult> CreateScheduleMeals(Schedule schedule, List<SheduleMealDto> mealTypes)
		{
			if (mealTypes == null || !mealTypes.Any())
			{
				return new ServiceResult
				{
					Success = false,
					Message = "Schedule must have at least one meal type"
				};
			}

			var allScheduleMeals = new List<ScheduleMeal>();
			var functionKeysUsed = new HashSet<string>();

			foreach (var mealTypeConfig in mealTypes)
			{
				var mealType = await _adminData.GetMealTypeByIdAsync(mealTypeConfig.MealTypeId);
				if (mealType == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = $"Invalid meal type ID: {mealTypeConfig.MealTypeId}"
					};
				}

				var result = await ProcessMealType(schedule, mealType, mealTypeConfig, functionKeysUsed);
				if (!result.Success)
				{
					return result;
				}

				allScheduleMeals.AddRange((List<ScheduleMeal>)result.Data);
			}

			await _businessData.CreateSheduleMealAsync(allScheduleMeals);
			return new ServiceResult { Success = true };
		}

		private async Task<ServiceResult> ProcessMealType(
			Schedule schedule,
			MealType mealType,
			SheduleMealDto mealTypeConfig,
			HashSet<string> functionKeysUsed)
		{
			var meals = new List<ScheduleMeal>();

			if (mealTypeConfig.SubMealTypes?.Any() == true)
			{
				foreach (var subMeal in mealTypeConfig.SubMealTypes)
				{
					var subType = await _adminData.GetMealSubTypesByIdAsync(subMeal.MealSubTypeId);
					if (subType == null)
					{
						continue;
					}
                    var mealCost = await _adminData.GetMealCostByDetailAsync(
						   subMeal.SupplierId,
						   mealType.MealTypeId,
						   subType.MealSubTypeId
					 );

                    if (mealCost == null)
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = $"Meal cost not configured for supplier ID {subMeal.SupplierId}, " +
                                      $"meal type '{mealType.TypeName}', and sub type '{subType.SubTypeName}'. " +
                                      $"Please configure the meal cost before creating this schedule."
                        };
                    }
                    if (!string.IsNullOrEmpty(subType.Functionkey))
					{
						
						functionKeysUsed.Add(subType.Functionkey);
					}

					meals.Add(CreateScheduleMeal(schedule, mealType, subType, subMeal.SupplierId));
				}
			}
			else if (mealTypeConfig.SupplierId.HasValue)
			{
				meals.Add(CreateScheduleMeal(schedule, mealType, null, mealTypeConfig.SupplierId.Value));
			}
			else
			{
				return new ServiceResult
				{
					Success = false,
					Message = $"Supplier is required for meal type '{mealType.TypeName}'"
				};
			}

			return new ServiceResult { Success = true, Data = meals };
		}

		private ScheduleMeal CreateScheduleMeal(
			Schedule schedule,
			MealType mealType,
			MealSubType subType = null,
			int supplierId = 0)
		{
			return new ScheduleMeal
			{
				TenantId = _tenantContext.TenantId.Value,
				SheduleId = schedule.SheduleId,
				MealTypeId = mealType.MealTypeId,
				MealSubTypeId = subType?.MealSubTypeId,
				SupplierId = supplierId,
				IsFunctionKeysEnable = !string.IsNullOrEmpty(subType?.Functionkey),
				FunctionKey = subType?.Functionkey,
				TokenIssueStartTime = mealType.TokenIssueStartTime,
				TokenIssueEndTime = mealType.TokenIssueEndTime,
				IsAvailable = true
			};
		}

		private async Task<ServiceResult> AssignPersonsToSchedule(Schedule schedule, List<int> personIds)
		{
			var schedulePersons = new List<SchedulePerson>();
			var invalidPersonIds = new List<int>();
			var validPersonIds = new List<int>();

			foreach (var personId in personIds)
			{
				var person = await _adminData.GetPersonByIdAsync(personId);
				if (person == null)
				{
					invalidPersonIds.Add(personId);
					continue;
				}

				schedulePersons.Add(new SchedulePerson
				{
					TenantId = _tenantContext.TenantId.Value,
					PersonId = personId,
					SheduleId = schedule.SheduleId
				});
				validPersonIds.Add(personId);
			}

			if (schedulePersons.Any())
			{
				await _businessData.CreateShedulePersonAsync(schedulePersons);
			}

			if (invalidPersonIds.Any())
			{
				_logger.LogWarning(
					"Some person assignments failed. ScheduleId: {ScheduleId}, InvalidIds: {InvalidIds}",
					schedule.SheduleId,
					string.Join(", ", invalidPersonIds)
				);
			}

			return new ServiceResult { Success = true };
		}

		private async Task<ServiceResult> ValidateTokenTimeConflictsAsync(
			List<int> assignedPersonIds,
			List<DateOnly> scheduleDates,
			List<SheduleMealDto> mealTypes)
		{
			var conflicts = new List<TokenTimeConflict>();

			foreach (var personId in assignedPersonIds)
			{
				foreach (var date in scheduleDates)
				{
					var existingSchedules = await _businessData.GetPersonSchedulesForDateAsync(personId, date);
					if (!existingSchedules.Any())
						continue;

					await ValidatePersonScheduleConflicts(
						personId,
						date,
						mealTypes,
						existingSchedules,
						conflicts);
				}
			}

			if (conflicts.Any())
			{
				return new ServiceResult
				{
					Success = false,
					Message = FormatTokenConflictMessage(conflicts),
					Data = conflicts
				};
			}

			return new ServiceResult { Success = true };
		}

		private async Task ValidatePersonScheduleConflicts(
			int personId,
			DateOnly date,
			List<SheduleMealDto> newMealTypes,
			IEnumerable<Schedule> existingSchedules,
			List<TokenTimeConflict> conflicts)
		{
			foreach (var newMealType in newMealTypes)
			{
				var newMealTypeDetails = await _adminData.GetMealTypeByIdAsync(newMealType.MealTypeId);
				if (newMealTypeDetails == null) continue;

				var newTokenTime = GetTokenTimes(newMealTypeDetails);

				foreach (var existingSchedule in existingSchedules)
				{
					var existingMeals = await _businessData.GetScheduleMealsAsync(existingSchedule.SheduleId);
					await CheckMealConflicts(
						personId,
						date,
						newMealTypeDetails,
						newTokenTime,
						existingSchedule,
						existingMeals,
						conflicts);
				}
			}
		}

		private async Task CheckMealConflicts(
			int personId,
			DateOnly date,
			MealType newMealType,
			(TimeOnly Start, TimeOnly End) newTokenTime,
			Schedule existingSchedule,
			IEnumerable<ScheduleMeal> existingMeals,
			List<TokenTimeConflict> conflicts)
		{
			foreach (var existingMeal in existingMeals)
			{
				var existingMealType = await _adminData.GetMealTypeByIdAsync(existingMeal.MealTypeId);
				if (existingMealType == null) continue;

				var existingTokenTime = GetTokenTimes(existingMealType, existingMeal);

				if (TokenTimesOverlap(newTokenTime.Start, newTokenTime.End,
					existingTokenTime.Start, existingTokenTime.End))
				{
					var person = await _adminData.GetPersonByIdAsync(personId);
					conflicts.Add(CreateTokenConflict(
						person,
						date,
						newMealType,
						newTokenTime,
						existingSchedule,
						existingMealType,
						existingTokenTime));
				}
			}
		}

		private (TimeOnly Start, TimeOnly End) GetTokenTimes(MealType mealType, ScheduleMeal scheduleMeal = null)
		{
			var start = scheduleMeal?.TokenIssueStartTime ??
					   mealType.TokenIssueStartTime ??
					   TimeOnly.MinValue;

			var end = scheduleMeal?.TokenIssueEndTime ??
					 mealType.TokenIssueEndTime ??
					 TimeOnly.MaxValue;

			return (start, end);
		}

		private TokenTimeConflict CreateTokenConflict(
			Person person,
			DateOnly date,
			MealType newMealType,
			(TimeOnly Start, TimeOnly End) newTokenTime,
			Schedule existingSchedule,
			MealType existingMealType,
			(TimeOnly Start, TimeOnly End) existingTokenTime)
		{
			return new TokenTimeConflict
			{
				PersonId = person?.PersonId ?? 0,
				PersonName = person?.Name ?? "Unknown",
				ConflictDate = date,
				NewMealType = newMealType.TypeName,
				NewTokenTime = $"{newTokenTime.Start:HH:mm} - {newTokenTime.End:HH:mm}",
				ExistingScheduleId = existingSchedule.SheduleId,
				ExistingScheduleName = existingSchedule.SheduleName,
				ExistingMealType = existingMealType.TypeName,
				ExistingTokenTime = $"{existingTokenTime.Start:HH:mm} - {existingTokenTime.End:HH:mm}"
			};
		}

		private bool TokenTimesOverlap(
			TimeOnly tokenStart1,
			TimeOnly tokenEnd1,
			TimeOnly tokenStart2,
			TimeOnly tokenEnd2)
		{
			return tokenStart1 < tokenEnd2 && tokenStart2 < tokenEnd1;
		}

		private string FormatTokenConflictMessage(List<TokenTimeConflict> conflicts)
		{
			if (conflicts.Count == 1)
			{
				var conflict = conflicts.First();
				return $"Token time conflict: {conflict.PersonName} already has token issuing time " +
					   $"{conflict.ExistingTokenTime} for {conflict.ExistingMealType} on " +
					   $"{conflict.ConflictDate:yyyy-MM-dd} which overlaps with new {conflict.NewMealType} " +
					   $"token time {conflict.NewTokenTime}";
			}

			var groupedByPerson = conflicts.GroupBy(c => c.PersonName);
			var conflictSummary = string.Join("; ",
				groupedByPerson.Select(g => $"{g.Key}: {g.Count()} token conflicts"));

			return $"Multiple token time conflicts found: {conflictSummary}. " +
				   "Persons cannot have overlapping token issuing times on the same date.";
		}

        public async Task<ServiceResult> UpdateScheduleAsync(int scheduleId, SheduleDTO updateDto)
        {
            using var scope = _logger.BeginScope("Schedule Update - {CorrelationId}", Guid.NewGuid());
            _logger.LogInformation("Starting schedule update. ScheduleId: {ScheduleId}, ScheduleName: {ScheduleName}",
                scheduleId, updateDto?.ScheduleName);

            try
            {
                // 1. Validate input and get existing schedule
                if (updateDto == null)
                {
                    return new ServiceResult { Success = false, Message = "Update data cannot be null" };
                }

                var existingSchedule = await _businessData.GetScheduleByIdAsync(scheduleId);
                if (existingSchedule == null)
                {
                    return new ServiceResult { Success = false, Message = $"Schedule with ID {scheduleId} not found" };
                }

                // 2. Validate name change if applicable
                if (!string.IsNullOrWhiteSpace(updateDto.ScheduleName) &&
                    updateDto.ScheduleName != existingSchedule.SheduleName)
                {
                    var nameConflict = await _businessData.GetSheduleByNameAsync(updateDto.ScheduleName);
                    if (nameConflict != null && nameConflict.SheduleId != scheduleId)
                    {
                        return new ServiceResult { Success = false, Message = $"Schedule with name '{updateDto.ScheduleName}' already exists" };
                    }
                }

                // 3. Update basic schedule properties
                await UpdateScheduleProperties(existingSchedule, updateDto);

                if (updateDto.ScheduleDates != null)
                {
                    var dateResult = await UpdateScheduleDates(existingSchedule, updateDto.ScheduleDates);
                    if (!dateResult.Success) return dateResult;
                }

                // Update meal types if provided (including an empty list to clear all meals)
                if (updateDto.MealTypes != null)
                {
                    var mealResult = await UpdateScheduleMeals(existingSchedule, updateDto.MealTypes);
                    if (!mealResult.Success) return mealResult;
                }

                // --- Core Fix: Centralized Conflict Validation and Assignment Update ---

                // Determine the FINAL state of the schedule for validation/saving.
                // If DTO provides a value, use it; otherwise, fetch the existing value from DB.
                var finalDates = updateDto.ScheduleDates ??
                                 (await _businessData.GetScheduleDatesAsync(scheduleId)).Select(d => d.Date).ToList();

                var finalMeals = updateDto.MealTypes ??
                                 (await _businessData.GetScheduleMealsAsync(scheduleId))
                                    .Select(m => new SheduleMealDto { MealTypeId = m.MealTypeId }).ToList();

                var finalAssignedPersons = updateDto.AssignedPersonIds ??
                                           (await _businessData.GetSchedulePeopleAsync(scheduleId)).Select(p => p.PersonId).ToList();

                // 4. Validate conflicts against the FINAL state
                if (finalAssignedPersons.Any() && finalDates.Any() && finalMeals.Any())
                {
                    // Validate against the determined final lists
                    var conflictResult = await ValidateTokenTimeConflictsForUpdate(
                        scheduleId,
                        finalAssignedPersons,
                        finalDates,
                        finalMeals
                    );

                    if (!conflictResult.Success)
                    {
                        return conflictResult;
                    }
                }

                // 5. Update person assignments (only if the list was provided in the DTO)
                if (updateDto.AssignedPersonIds != null) // Check for '!= null' to allow clearing the list
                {
                    var assignmentResult = await UpdatePersonAssignments(existingSchedule, updateDto.AssignedPersonIds);
                    if (!assignmentResult.Success)
                    {
                        return assignmentResult;
                    }
                }

                // 6. Save changes and return success
                await _businessData.SaveChangesAsync();

                _logger.LogInformation(
                    "Schedule updated successfully. ScheduleId: {ScheduleId}, Name: {Name}",
                    existingSchedule.SheduleId,
                    existingSchedule.SheduleName
                );

                return new ServiceResult
                {
                    Success = true,
                    Message = "Schedule updated successfully",
                    ObjectId = existingSchedule.SheduleId,
                    Data = new
                    {
                        ScheduleId = existingSchedule.SheduleId,
                        ScheduleName = existingSchedule.SheduleName,
                        DatesCount = finalDates.Count,
                        MealTypesCount = finalMeals.Count,
                        AssignedPersonsCount = finalAssignedPersons.Count
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule. ScheduleId: {ScheduleId}", scheduleId);
                return new ServiceResult
                {
                    Success = false,
                    Message = "An unexpected error occurred while updating the schedule."
                };
            }
        }

        private async Task<ServiceResult> ValidateTokenTimeConflictsForUpdate(
			int currentScheduleId,
			List<int> assignedPersonIds,
			List<DateOnly> scheduleDates,
			List<SheduleMealDto> mealTypes)
		{
			var conflicts = new List<TokenTimeConflict>();

			foreach (var personId in assignedPersonIds)
			{
				foreach (var date in scheduleDates)
				{
					var existingSchedules = await _businessData.GetPersonSchedulesForDateAsync(personId, date);

					// Filter out the current schedule being updated
					existingSchedules = existingSchedules.Where(s => s.SheduleId != currentScheduleId).ToList();

					if (!existingSchedules.Any())
						continue;

					await ValidatePersonScheduleConflicts(
						personId,
						date,
						mealTypes,
						existingSchedules,
						conflicts);
				}
			}

			if (conflicts.Any())
			{
				return new ServiceResult
				{
					Success = false,
					Message = FormatTokenConflictMessage(conflicts),
					Data = conflicts
				};
			}

			return new ServiceResult { Success = true };
		}

		private async Task UpdateScheduleProperties(Schedule schedule, SheduleDTO updateDto)
		{
			if (!string.IsNullOrWhiteSpace(updateDto.ScheduleName))
			{
				schedule.SheduleName = updateDto.ScheduleName;
			}

			if (updateDto.SchedulePeriod != null)
			{
				schedule.ShedulePeriod = updateDto.SchedulePeriod;
			}

			if (updateDto.Note != null) // Allow empty string to clear notes
			{
				schedule.Note = updateDto.Note;
			}

			await _businessData.UpdateScheduleAsync(schedule);
		}

		private async Task<ServiceResult> UpdateScheduleDates(Schedule schedule, List<DateOnly> newDates)
		{
			if (!newDates.Any())
			{
				return new ServiceResult
				{
					Success = false,
					Message = "Schedule must have at least one date"
				};
			}

            await _businessData.DeleteScheduleDatesAsync(schedule.SheduleId);

            // 2. Add new dates (only if the list is NOT empty)
            if (newDates.Any())
            {
                var scheduleDates = newDates.Select(date => new ScheduleDate
                {
                    TenantId = _tenantContext.TenantId.Value,
                    SheduleId = schedule.SheduleId,
                    Date = date
                }).ToList();

                await _businessData.CreateSheduleDateAsync(scheduleDates);
            }

            // We return success because the operation (delete/replace) completed successfully.
            return new ServiceResult { Success = true };
        }

		private async Task<ServiceResult> UpdateScheduleMeals(Schedule schedule, List<SheduleMealDto> newMealTypes)
		{
			if (!newMealTypes.Any())
			{
				return new ServiceResult
				{
					Success = false,
					Message = "Schedule must have at least one meal type"
				};
			}

            // Remove existing meals
            await _businessData.DeleteScheduleMealsAsync(schedule.SheduleId);

            // Create new meals (only if the list is NOT empty)
            if (newMealTypes.Any())
            {
                // Assuming CreateScheduleMeals handles the insertion logic based on the list
                return await CreateScheduleMeals(schedule, newMealTypes);
            }

            // If newMealTypes is empty, we only did the delete, which is success.
            return new ServiceResult { Success = true };
        }

		private async Task<ServiceResult> UpdatePersonAssignments(Schedule schedule, List<int> newPersonIds)
		{
			// Remove existing assignments
			await _businessData.DeleteSchedulePersonsAsync(schedule.SheduleId);

			// Create new assignments
			return await AssignPersonsToSchedule(schedule, newPersonIds);
		}

		public async Task<ServiceResult> DeleteScheduleAsync(int scheduleId)
		{
			using var scope = _logger.BeginScope("Schedule Deletion - {CorrelationId}", Guid.NewGuid());
			_logger.LogInformation("Starting schedule deletion. ScheduleId: {ScheduleId}", scheduleId);

			try
			{
				if (scheduleId <= 0)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Invalid schedule ID"
					};
				}

				var schedule = await _businessData.GetScheduleByIdAsync(scheduleId);
				if (schedule == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = $"Schedule with ID {scheduleId} not found"
					};
				}

				// Check if schedule can be deleted (e.g., no active tokens)
				/*var canDelete = await _businessData.CanDeleteScheduleAsync(scheduleId);
				if (!canDelete)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Schedule cannot be deleted because it has active tokens or other dependencies"
					};
				}*/

				// Delete all related records first
				await _businessData.DeleteSchedulePersonsAsync(scheduleId);
				await _businessData.DeleteScheduleMealsAsync(scheduleId);
				await _businessData.DeleteScheduleDatesAsync(scheduleId);

				// Finally delete the schedule
				await _businessData.DeleteScheduleAsync(scheduleId);

				_logger.LogInformation("Schedule deleted successfully. ScheduleId: {ScheduleId}, Name: {Name}",
					scheduleId, schedule.SheduleName);

				return new ServiceResult
				{
					Success = true,
					Message = "Schedule deleted successfully",
					ObjectId = scheduleId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Critical error during schedule deletion. ScheduleId: {ScheduleId}", scheduleId);
				return new ServiceResult
				{
					Success = false,
					Message = "An unexpected error occurred while deleting the schedule."
				};
			}
		}

		public async Task<ServiceResult> GetScheduleListAsync()
		{
			try
			{
				_logger.LogInformation("Retrieving schedule list");
				var schedules = await _businessData.GetSchedulesAsync();
				var scheduleDtos = new List<ScheduleDetails>();

				foreach (var schedule in schedules)
				{
					var scheduleDetails = await BuildScheduleDetailsAsync(schedule);
					scheduleDtos.Add(scheduleDetails);
				}

				return new ServiceResult
				{
					Success = true,
					Message = "Schedules retrieved successfully",
					Data = scheduleDtos
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving schedule list");
				return new ServiceResult
				{
					Success = false,
					Message = "An error occurred while retrieving schedules"
				};
			}
		}

		public async Task<ServiceResult> GetScheduleByIdAsync(int scheduleId)
		{
			try
			{
				_logger.LogInformation("Retrieving schedule details. ScheduleId: {ScheduleId}", scheduleId);

				var schedule = await _businessData.GetScheduleByIdAsync(scheduleId);
				if (schedule == null)
				{
					_logger.LogWarning("Schedule not found. ScheduleId: {ScheduleId}", scheduleId);
					return new ServiceResult
					{
						Success = false,
						Message = "Schedule not found"
					};
				}

				var scheduleDetails = await BuildScheduleDetailsAsync(schedule);

				return new ServiceResult
				{
					Success = true,
					Message = "Schedule details retrieved successfully",
					Data = scheduleDetails
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving schedule details. ScheduleId: {ScheduleId}", scheduleId);
				return new ServiceResult
				{
					Success = false,
					Message = "An error occurred while retrieving schedule details"
				};
			}
		}
        /*
		private async Task<ScheduleDetails> BuildScheduleDetailsAsync(Schedule schedule)
		{
			var dates = await _businessData.GetScheduleDatesAsync(schedule.SheduleId);
			var meals = await _businessData.GetScheduleMealsAsync(schedule.SheduleId);
			var persons = await _businessData.GetSchedulePeopleAsync(schedule.SheduleId);

			var mealTypes = new List<MealTypeD>();
			var subTypes = new List<SubMealTypeD>();

			foreach (var meal in meals)
			{
				var supplier = await _adminData.GetSupplierByIdAsync(meal.SupplierId);

                if (meal.MealSubTypeId.HasValue)
                {
                    var subType = await _adminData.GetMealSubTypesByIdAsync(meal.MealSubTypeId.Value);
                    if (subType != null)
                    {
                        // Fetch meal type for subtype
                        var mealType = await _adminData.GetMealTypeByIdAsync(meal.MealTypeId);

                        // Add meal type to list if it exists
                        if (mealType != null)
                        {
                            mealTypes.Add(new MealTypeD
                            {
                                MealTypeId = mealType.MealTypeId,
                                MealTypeName = mealType.TypeName,
                                Supplier = supplier != null ? new SupplierD
                                {
                                    SupplierId = supplier.SupplierId,
                                    SupplierName = supplier.SupplierName
                                } : null
                            });
                        }

                        subTypes.Add(new SubMealTypeD
                        {
                            MealSubTypeId = subType.MealSubTypeId,
                            MealSubTypeName = subType.SubTypeName,
                            MealTypeId = meal.MealTypeId,
                            Supplier = supplier != null ? new SupplierD
                            {
                                SupplierId = supplier.SupplierId,
                                SupplierName = supplier.SupplierName
                            } : null
                        });
                    }
                }
                else
                {
                    var mealType = await _adminData.GetMealTypeByIdAsync(meal.MealTypeId);
                    if (mealType != null)
                    {
                        mealTypes.Add(new MealTypeD
                        {
                            MealTypeId = mealType.MealTypeId,
                            MealTypeName = mealType.TypeName,
                            Supplier = supplier != null ? new SupplierD
                            {
                                SupplierId = supplier.SupplierId,
                                SupplierName = supplier.SupplierName
                            } : null
                        });
                    }
                }
            }

            var assignedPersons = new List<PeopleD>();
			foreach (var person in persons)
			{
				var personDetails = await _adminData.GetPersonByIdAsync(person.PersonId);
				if (personDetails != null)
				{
					assignedPersons.Add(new PeopleD
					{
						PersonId = personDetails.PersonId,
						FullName = personDetails.Name
					});
				}
			}

			return new ScheduleDetails
			{
				ScheduleId = schedule.SheduleId,
				ScheduleName = schedule.SheduleName,
				SchedulePeriod = schedule.ShedulePeriod,
				ScheduleDates = dates.Select(d => d.Date).ToList(),
				MealTypes = mealTypes,
				SubTypes = subTypes,
				AssignedPersons = assignedPersons
			};
		}
        */
        private async Task<ScheduleDetails> BuildScheduleDetailsAsync(Schedule schedule)
        {
            // Fetch all data sequentially (avoid DBContext threading issues)
            var dates = await _businessData.GetScheduleDatesAsync(schedule.SheduleId);
            var meals = await _businessData.GetScheduleMealsAsync(schedule.SheduleId);
            var persons = await _businessData.GetSchedulePeopleAsync(schedule.SheduleId);

            // Extract unique IDs to batch fetch
            var mealTypeIds = meals.Select(m => m.MealTypeId).Distinct().ToList();
            var supplierIds = meals.Select(m => m.SupplierId).Distinct().ToList();
            var subTypeIds = meals.Where(m => m.MealSubTypeId.HasValue).Select(m => m.MealSubTypeId.Value).Distinct().ToList();
            var personIds = persons.Select(p => p.PersonId).Distinct().ToList();

            // Batch fetch all related data
            var mealTypesDict = (await _adminData.GetMealTypesByIdsAsync(mealTypeIds))
                .ToDictionary(mt => mt.MealTypeId);

            var suppliersDict = (await _adminData.GetSuppliersByIdsAsync(supplierIds))
                .ToDictionary(s => s.SupplierId);

            var subTypesDict = subTypeIds.Any()
                ? (await _adminData.GetMealSubTypesByIdsAsync(subTypeIds)).ToDictionary(st => st.MealSubTypeId)
                : new Dictionary<int, MealSubType>();

            var personsDict = (await _adminData.GetPeopleByIdsAsync(personIds))
                .ToDictionary(p => p.PersonId);

            // Build meal types and subtypes
            var mealTypesSet = new HashSet<int>();
            var mealTypes = new List<MealTypeD>();
            var subTypes = new List<SubMealTypeD>();

            foreach (var meal in meals)
            {
                var supplier = supplierIds.Contains(meal.SupplierId) ? suppliersDict.GetValueOrDefault(meal.SupplierId) : null;
                var supplierDto = supplier != null ? new SupplierD
                {
                    SupplierId = supplier.SupplierId,
                    SupplierName = supplier.SupplierName
                } : null;

                // Add meal type if not already added (avoid duplicates)
                if (mealTypesDict.TryGetValue(meal.MealTypeId, out var mealType) && mealTypesSet.Add(meal.MealTypeId))
                {
                    mealTypes.Add(new MealTypeD
                    {
                        MealTypeId = mealType.MealTypeId,
                        MealTypeName = mealType.TypeName,
                        Supplier = supplierDto
                    });
                }

                // Add subtypes
                if (meal.MealSubTypeId.HasValue && subTypesDict.TryGetValue(meal.MealSubTypeId.Value, out var subType))
                {
                    subTypes.Add(new SubMealTypeD
                    {
                        MealSubTypeId = subType.MealSubTypeId,
                        MealSubTypeName = subType.SubTypeName,
                        MealTypeId = meal.MealTypeId,
                        Supplier = supplierDto
                    });
                }
            }

            // Build assigned persons
            var assignedPersons = personsDict.Values
                .Select(p => new PeopleD
                {
                    PersonId = p.PersonId,
                    FullName = p.Name
                })
                .ToList();

            return new ScheduleDetails
            {
                ScheduleId = schedule.SheduleId,
                ScheduleName = schedule.SheduleName,
                SchedulePeriod = schedule.ShedulePeriod,
                ScheduleDates = dates.Select(d => d.Date).ToList(),
                MealTypes = mealTypes,
                SubTypes = subTypes,
                AssignedPersons = assignedPersons
            };
        }
        public async Task<ServiceResult> GetScheduleCreationDetailsAsync()
		{
			try
			{
				_logger.LogInformation("Starting to retrieve schedule creation details");

                // Filter at database level instead of in memory
                var persons = await _adminData.GetPersonsByDepartmentsAsync(_userContext.DepartmentIds);
                var mealTypes = await _adminData.GetMealTypesAsync();
				var suppliers = await _adminData.GetAllSupplierAsync();
				var subMealTypes = await _adminData.GetMealSubTypesListAsync(); // 👈 new fetch


				var scheduleDetails = new SchedulecreationDetails
				{
					Persons = persons?.Select(p => new PeopleReturn
					{
						PersonId = p.PersonId,
						PersonType = p.PersonType,
						Name = p.Name
					})?.ToList() ?? new List<PeopleReturn>(),

					MealTypes = mealTypes?.Select(mt => new MealTypeReturn
					{
						MealTypeId = mt.MealTypeId,
						MealTypeName = mt.MealTypeName,
						SubTypes = subMealTypes
							.Where(st => st.MealTypeId == mt.MealTypeId) // 👈 filter by MealTypeId
							.Select(st => new SubMealTypeReturn
							{
								MealSubTypeId = st.MealSubTypeId,
								MealSubTypeName = st.SubTypeName
							})
							.ToList()
					})?.ToList() ?? new List<MealTypeReturn>(),

					Suppliers = suppliers?.Select(s => new SupplierD
					{
						SupplierId = s.SupplierId,
						SupplierName = s.SupplierName
					})?.ToList() ?? new List<SupplierD>()
				};

				_logger.LogInformation("Successfully retrieved schedule creation details. " +
					"Persons: {PersonCount}, MealTypes: {MealTypeCount}, Suppliers: {SupplierCount}",
					scheduleDetails.Persons.Count,
					scheduleDetails.MealTypes.Count,
					scheduleDetails.Suppliers.Count);

				return new ServiceResult
				{
					Success = true,
					Message = "Schedule creation details retrieved successfully",
					Data = scheduleDetails
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving schedule creation details");
				return new ServiceResult
				{
					Success = false,
					Message = "An error occurred while retrieving schedule creation details"
				};
			}
		}

		public async Task<List<MealRequestDetails>> GetMealRequestCreationDetailsAsync()
		{
			try
			{
				_logger.LogInformation("Retrieving meal costs list");
				var mealCosts = await _adminData.GetAllMealCostsAsync();

				if (mealCosts == null || !mealCosts.Any())
				{
					_logger.LogInformation("No meal costs found in the database.");
					return new List<MealRequestDetails>();
				}

				var mealCostDtos = new List<MealCostDetails>();

				foreach (var mc in mealCosts)
				{
					try
					{
						var dto = new MealCostDetails
						{
							MealCostId = mc.MealCostId,
							SupplierId = mc.SupplierId,
							SupplierName = await _adminData.GetSupplierNameAsync(mc.SupplierId) ?? "Unknown",
							MealTypeId = mc.MealTypeId,
							MealTypeName = await _adminData.GetMealTypeNameAsync(mc.MealTypeId) ?? "Unknown",
							MealSubTypeId = mc.MealSubTypeId,
							MealSubTypeName = mc.MealSubTypeId.HasValue
								? await _adminData.GetMealSubTypeNameAsync(mc.MealSubTypeId.Value)
								: null,
							SupplierCost = mc.SupplierCost,
							SellingPrice = mc.SellingPrice,
							CompanyCost = mc.CompanyCost,
							EmployeeCost = mc.EmployeeCost,
							Description = mc.Description
						};
						mealCostDtos.Add(dto);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Error processing meal cost record. MealCostId: {MealCostId}", mc.MealCostId);
					}
				}

				// Group by MealTypeId
				var groupedResults = mealCostDtos
					.GroupBy(mc => mc.MealTypeId)
					.Select(group => new MealRequestDetails
					{
						MealTypeId = group.Key,
						MealTypeName = group.First().MealTypeName,
						MealSubTypeDetails = group
							.OrderBy(mc => mc.MealSubTypeName ?? string.Empty)
							.ToList()
					})
					.ToList();

				_logger.LogInformation("Retrieved {MealTypeGroupCount} grouped meal costs successfully.", groupedResults.Count);

				return groupedResults;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving meal costs list.");
				return new List<MealRequestDetails>();
			}
		}
        public async Task<ServiceResult> CreateMealRequestAsync(RequestDto requestDto)
        {
            try
            {
                _logger.LogInformation("Starting meal request creation. EventDate: {EventDate}, EventType: {EventType}",
                    requestDto?.EventDate, requestDto?.EventType);

                if (requestDto == null)
                {
                    _logger.LogWarning("Null request DTO provided");
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Request data cannot be null"
                    };
                }

                var exRequest = await _businessData.GetRequestByDetailsAsync(requestDto.EventDate, requestDto.EventType, requestDto.Description);
                if (exRequest != null) // Should check for existence, not null!
                {
                    _logger.LogWarning("Duplicate meal request detected. ExistingRequestId: {RequestId}",
                        exRequest.MealRequestId);
                    return new ServiceResult
                    {
                        Success = false,
                        Message = $"A similar request already exists for this event date and type"
                    };
                }
                var mealCostIds = requestDto.RequestMeals.Select(m => m.MealCostId).Distinct().ToList();

                // Step 2: Fetch all matching MealCost objects in a single database trip.
                var mealCostsFromDb = await _adminData.GetMealCostsByIdsAsync(mealCostIds);
                var mealCostMap = mealCostsFromDb.ToDictionary(mc => mc.MealCostId);

                // Step 3: Now validate in memory (which is extremely fast).
                foreach (var meal in requestDto.RequestMeals)
                {
                    if (mealCostMap.TryGetValue(meal.MealCostId, out var mealCost))
                    {
                        if (mealCost.MealTypeId != meal.MealTypeId || mealCost.MealSubTypeId != meal.SubTypeId)
                        {
                            return new ServiceResult
                            {
                                Success = false,
                                Message = $"Mismatch found for MealCostId '{meal.MealCostId}'. The provided MealTypeId or SubTypeId is incorrect."
                            };
                        }
                    }
                    else
                    {
                        // Handle case where the ID wasn't found in our bulk fetch.
                        return new ServiceResult
                        {
                            Success = false,
                            Message = $"Invalid MealCostId: No meal cost found for ID '{meal.MealCostId}'."
                        };
                    }
                }

                var request = new Request
                {
                    TenantId = _tenantContext.TenantId.Value,
                    EventDate = requestDto.EventDate,
                    EventType = requestDto.EventType,
                    Description = requestDto.Description,
                    NoofAttendees = requestDto.NoOfAttendess,
                    Status = UserRequestStatus.Pending,
                    RequesterId = _userContext.UserId.Value
                };

                await _businessData.CreateRequestAsync(request);

                var requestMeals = new List<RequestMeal>();
                foreach (var meal in requestDto.RequestMeals ?? Enumerable.Empty<RequestMealDto>())
                {
                    requestMeals.Add(new RequestMeal
                    {
                        TenantId = _tenantContext.TenantId.Value,
						RequestId = request.MealRequestId,
                        MealTypeId = meal.MealTypeId,
                        SubTypeId = meal.SubTypeId,
                        MealCostId = meal.MealCostId,
                        Quantity = meal.Quantity
                    });
                }

                await _businessData.CreateRequestMealAsync(requestMeals);
                await SendRequestSendEmailAsync(request);

				return new ServiceResult
                {
                    Success = true,
                    Message = "Meal request created successfully",
                    ObjectId = request.MealRequestId,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error creating meal request. EventDate: {EventDate}, EventType: {EventType}, ErrorType: {ErrorType}",
                    requestDto?.EventDate, requestDto?.EventType, ex.GetType().Name);

                return new ServiceResult
                {
                    Success = false,
                    Message = "An error occurred while creating the meal request. Please contact support if this continues.",
                };
            }
        }
        public async Task<ServiceResult> UpdateMealRequestAsync(int mealRequestId, RequestDto requestDto)
        {
            try
            {
                _logger.LogInformation("Starting meal request update. MealRequestId: {MealRequestId}, EventDate: {EventDate}, EventType: {EventType}",
                    mealRequestId, requestDto?.EventDate, requestDto?.EventType);

                if (requestDto == null)
                {
                    _logger.LogWarning("Null request DTO provided for update");
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Request data cannot be null"
                    };
                }

                var existingRequest = await _businessData.GetRequestByIdAsync(mealRequestId);
                if (existingRequest == null)
                {
                    _logger.LogWarning("No existing meal request found for update. MealRequestId: {MealRequestId}", mealRequestId);
                    return new ServiceResult
                    {
                        Success = false,
                        Message = $"No meal request found with ID {mealRequestId}"
                    };
                }

                // Update fields
                existingRequest.EventDate = requestDto.EventDate;
                existingRequest.EventType = requestDto.EventType;
                existingRequest.Description = requestDto.Description;
                existingRequest.NoofAttendees = requestDto.NoOfAttendess;
                existingRequest.Status = existingRequest.Status;
                existingRequest.RequesterId = _userContext.UserId.Value;

                await _businessData.UpdateRequestAsync(existingRequest);

                // Update RequestMeals (this logic may vary based on requirements)
                var updatedMeals = new List<RequestMeal>();
                foreach (var meal in requestDto.RequestMeals ?? Enumerable.Empty<RequestMealDto>())
                {
                    updatedMeals.Add(new RequestMeal
                    {
                        TenantId = _tenantContext.TenantId.Value,
						RequestId = mealRequestId,
                        MealTypeId = meal.MealTypeId,
                        SubTypeId = meal.SubTypeId.Value,
                        MealCostId = meal.MealCostId,
                        Quantity = meal.Quantity
                    });
                }
				await _businessData.DeleteRequestMealsAsync(mealRequestId);
               await _businessData.CreateRequestMealAsync(updatedMeals);

                return new ServiceResult
                {
                    Success = true,
                    Message = "Meal request updated successfully",
                    ObjectId = mealRequestId,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error updating meal request. MealRequestId: {MealRequestId}, EventDate: {EventDate}, EventType: {EventType}, ErrorType: {ErrorType}",
                    mealRequestId, requestDto?.EventDate, requestDto?.EventType, ex.GetType().Name);

                return new ServiceResult
                {
                    Success = false,
                    Message = "An error occurred while updating the meal request. Please contact support if this continues.",
                };
            }
        }

        public async Task<ServiceResult> GetRequestDetailsAsync(int requestId)
        {
            try
            {
                _logger.LogInformation("Fetching request details. RequestId: {RequestId}", requestId);

                var request = await _businessData.GetRequestByIdAsync(requestId);
                if (request == null)
                {
                    _logger.LogWarning("Request not found. RequestId: {RequestId}", requestId);
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Request not found"
                    };
                }

                var requestDto = new RequestDto
                {
                    EventDate = request.EventDate,
                    EventType = request.EventType,
                    Description = request.Description,
                    NoOfAttendess = request.NoofAttendees
                };

                var meals = await _businessData.GetRequestMealsAsync(requestId);
                requestDto.RequestMeals = meals.Select(m => new RequestMealDto
                {
                    MealTypeId = m.MealTypeId,
                    SubTypeId = m.SubTypeId,
                    MealCostId = m.MealCostId,
                    Quantity = m.Quantity
                }).ToList();

                return new ServiceResult
                {
                    Success = true,
                    Data = requestDto,
                    Message = "Request details retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching request details. RequestId: {RequestId}, ErrorType: {ErrorType}",
                    requestId, ex.GetType().Name);

                return new ServiceResult
                {
                    Success = false,
                    Message = "An error occurred while retrieving request details. Please contact support if this continues."
                };
            }
        }
        public async Task<ServiceResult> GetPendingRequestListAsync()
        {
            try
            {
                _logger.LogInformation("Fetching pending request list for user departments");

                // Get all pending requests
                var pendingRequests = await _businessData.GetPendingRequestListAsync();
                if (pendingRequests == null || !pendingRequests.Any())
                {
                    _logger.LogInformation("No pending requests found");
                    return new ServiceResult
                    {
                        Success = true,
                        Data = new List<RequestReturn>(),
                        Message = "No requests found"
                    };
                }
                // Get requester IDs based on user's departments
                var requesterIds = await _userData.GetUserIdsByDepartmentsAsync(_userContext.DepartmentIds);
                if (requesterIds == null || !requesterIds.Any())
                {
                    _logger.LogInformation("No requesters found in the user's departments");
                    return new ServiceResult
                    {
                        Success = true,
                        Data = new List<RequestReturn>(),
                        Message = "No requests found for your departments"
                    };
                }
                // Filter pending requests by requester departments
                var filteredRequests = pendingRequests
                    .Where(r => requesterIds.Contains(r.RequesterId))
                    .ToList();

                if (!filteredRequests.Any())
                {
                    _logger.LogInformation("No pending requests found for requesters in user's departments");
                    return new ServiceResult
                    {
                        Success = true,
                        Data = new List<RequestReturn>(),
                        Message = "No requests found for your departments"
                    };
                }

                // Batch fetch all requester names
                var requestersDict = (await _userData.GetUserNamesByIdsAsync(
                    filteredRequests.Select(r => r.RequesterId).Distinct().ToList()
                )).ToDictionary(u => u.UserId, u => u.UserName);

                // Batch fetch all meals for filtered requests
                var requestIds = filteredRequests.Select(r => r.MealRequestId).ToList();
                var allRequestMeals = await _businessData.GetRequestMealsByRequestIdsAsync(requestIds);

                // Group meals by request ID for faster lookup
                var mealsByRequestId = allRequestMeals
                    .GroupBy(m => m.RequestId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Map the requests to DTOs
                var requestDtos = new List<RequestReturn>();
                foreach (var request in filteredRequests)
                {
                    var requestDto = new RequestReturn
                    {
                        RequestId = request.MealRequestId,
                        EventDate = request.EventDate,
                        EventType = request.EventType,
                        Description = request.Description,
                        NoOfAttendess = request.NoofAttendees,
                        RequesterId = request.RequesterId,
                        Requester = requestersDict.ContainsKey(request.RequesterId)
                            ? requestersDict[request.RequesterId]
                            : "Unknown"
                    };

                    // Get meals for this request
                    if (mealsByRequestId.TryGetValue(request.MealRequestId, out var meals))
                    {
                        requestDto.RequestMeals = meals.Select(m => new RequestMealDto
                        {
                            MealTypeId = m.MealTypeId,
                            SubTypeId = m.SubTypeId,
                            MealCostId = m.MealCostId,
                            Quantity = m.Quantity
                        }).ToList();
                    }
                    else
                    {
                        requestDto.RequestMeals = new List<RequestMealDto>();
                    }

                    requestDtos.Add(requestDto);
                }

                return new ServiceResult
                {
                    Success = true,
                    Data = requestDtos,
                    Message = "Request list retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching request list. ErrorType: {ErrorType}",
                    ex.GetType().Name);
                return new ServiceResult
                {
                    Success = false,
                    Message = "An error occurred while retrieving request list. Please contact support if this continues."
                };
            }
        }
        public async Task<ServiceResult> GetApprovedRequestListAsync()
        {
            try
            {
                _logger.LogInformation("Fetching approved request list.");

                // Get the requests based on search parameters
                var requests = await _businessData.GetApprovedRequestListAsync();
                if (requests == null || !requests.Any())
                {
                    _logger.LogInformation("No requests found");
                    return new ServiceResult
                    {
                        Success = true,
                        Data = new List<RequestReturn>(),
                        Message = "No requests found"
                    };
                }

                var requesterIds = await _userData.GetUserIdsByDepartmentsAsync(_userContext.DepartmentIds);
                if (requesterIds == null || !requesterIds.Any())
                {
                    _logger.LogInformation("No requesters found in the user's departments");
                    return new ServiceResult
                    {
                        Success = true,
                        Data = new List<RequestReturn>(),
                        Message = "No requests found for your departments"
                    };
                }
                // Filter pending requests by requester departments
                var filteredRequests = requests
                    .Where(r => requesterIds.Contains(r.RequesterId))
                    .ToList();

                if (!filteredRequests.Any())
                {
                    _logger.LogInformation("No pending requests found for requesters in user's departments");
                    return new ServiceResult
                    {
                        Success = true,
                        Data = new List<RequestReturn>(),
                        Message = "No requests found for your departments"
                    };
                }

                // Batch fetch all requester names
                var requestersDict = (await _userData.GetUserNamesByIdsAsync(
                    filteredRequests.Select(r => r.RequesterId).Distinct().ToList()
                )).ToDictionary(u => u.UserId, u => u.UserName);

                // Batch fetch all meals for filtered requests
                var requestIds = filteredRequests.Select(r => r.MealRequestId).ToList();
                var allRequestMeals = await _businessData.GetRequestMealsByRequestIdsAsync(requestIds);

                // Group meals by request ID for faster lookup
                var mealsByRequestId = allRequestMeals
                    .GroupBy(m => m.RequestId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Map the requests to DTOs
                var requestDtos = new List<RequestReturn>();
                foreach (var request in filteredRequests)
                {
                    var requestDto = new RequestReturn
                    {
                        RequestId = request.MealRequestId,
                        EventDate = request.EventDate,
                        EventType = request.EventType,
                        Description = request.Description,
                        NoOfAttendess = request.NoofAttendees,
                        RequesterId = request.RequesterId,
                        Requester = requestersDict.ContainsKey(request.RequesterId)
                            ? requestersDict[request.RequesterId]
                            : "Unknown"
                    };

                    // Get meals for this request
                    if (mealsByRequestId.TryGetValue(request.MealRequestId, out var meals))
                    {
                        requestDto.RequestMeals = meals.Select(m => new RequestMealDto
                        {
                            MealTypeId = m.MealTypeId,
                            SubTypeId = m.SubTypeId,
                            MealCostId = m.MealCostId,
                            Quantity = m.Quantity
                        }).ToList();
                    }
                    else
                    {
                        requestDto.RequestMeals = new List<RequestMealDto>();
                    }

                    requestDtos.Add(requestDto);
                }


                return new ServiceResult
                {
                    Success = true,
                    Data = requestDtos,
                    Message = "Request list retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching request list. ErrorType: {ErrorType}",
                    ex.GetType().Name);

                return new ServiceResult
                {
                    Success = false,
                    Message = "An error occurred while retrieving request list. Please contact support if this continues."
                };
            }
        }

        public async Task<ServiceResult> GetRequestListByIdAsync()
        {
            try
            {
                _logger.LogInformation("Fetching request list for user.");

				int userId = _userContext.UserId.Value;
				var user = await _adminData.GetUserByIdAsync(userId);
                // Get the requests based on search parameters
                var requests = await _businessData.GetRequesListAsync();
                if (requests == null || !requests.Any())
                {
                    _logger.LogInformation("No requests found");
                    return new ServiceResult
                    {
                        Success = true,
                        Data = new List<RequestReturn>(),
                        Message = "No requests found"
                    };
                }

                var requesterIds = await _userData.GetUserIdsByDepartmentsAsync(_userContext.DepartmentIds);
                if (requesterIds == null || !requesterIds.Any())
                {
                    _logger.LogInformation("No requesters found in the user's departments");
                    return new ServiceResult
                    {
                        Success = true,
                        Data = new List<RequestReturn>(),
                        Message = "No requests found for your departments"
                    };
                }
                // Filter pending requests by requester departments
                var filteredRequests = requests
                    .Where(r => requesterIds.Contains(r.RequesterId))
                    .ToList();

                if (!filteredRequests.Any())
                {
                    _logger.LogInformation("No pending requests found for requesters in user's departments");
                    return new ServiceResult
                    {
                        Success = true,
                        Data = new List<RequestReturn>(),
                        Message = "No requests found for your departments"
                    };
                }

                // Batch fetch all requester names
                var requestersDict = (await _userData.GetUserNamesByIdsAsync(
                    filteredRequests.Select(r => r.RequesterId).Distinct().ToList()
                )).ToDictionary(u => u.UserId, u => u.UserName);

                // Batch fetch all meals for filtered requests
                var requestIds = filteredRequests.Select(r => r.MealRequestId).ToList();
                var allRequestMeals = await _businessData.GetRequestMealsByRequestIdsAsync(requestIds);

                // Group meals by request ID for faster lookup
                var mealsByRequestId = allRequestMeals
                    .GroupBy(m => m.RequestId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Map the requests to DTOs
                var requestDtos = new List<RequestReturn>();
                foreach (var request in filteredRequests)
                {
                    var requestDto = new RequestReturn
                    {
                        RequestId = request.MealRequestId,
                        EventDate = request.EventDate,
                        EventType = request.EventType,
                        Description = request.Description,
                        NoOfAttendess = request.NoofAttendees,
                        RequesterId = request.RequesterId,
                        Requester = requestersDict.ContainsKey(request.RequesterId)
                            ? requestersDict[request.RequesterId]
                            : "Unknown"
                    };

                    // Get meals for this request
                    if (mealsByRequestId.TryGetValue(request.MealRequestId, out var meals))
                    {
                        requestDto.RequestMeals = meals.Select(m => new RequestMealDto
                        {
                            MealTypeId = m.MealTypeId,
                            SubTypeId = m.SubTypeId,
                            MealCostId = m.MealCostId,
                            Quantity = m.Quantity
                        }).ToList();
                    }
                    else
                    {
                        requestDto.RequestMeals = new List<RequestMealDto>();
                    }

                    requestDtos.Add(requestDto);
                }


                return new ServiceResult
                {
                    Success = true,
                    Data = requestDtos,
                    Message = "Request list retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching request list. ErrorType: {ErrorType}",
                    ex.GetType().Name);

                return new ServiceResult
                {
                    Success = false,
                    Message = "An error occurred while retrieving request list. Please contact support if this continues."
                };
            }
        }
		public async Task<ServiceResult> UpdateRequestStatusAsync(int requestId, UserRequestStatus newStatus, int approverId, string? rejectReason)
		{
			try
			{
				_logger.LogInformation(
					"Updating request status. RequestId: {RequestId}, NewStatus: {NewStatus}, ApproverId: {ApproverId}",
					requestId, newStatus, approverId);

				var request = await _businessData.GetRequestByIdAsync(requestId);
				if (request == null)
				{
					_logger.LogWarning("Request not found for status update. RequestId: {RequestId}", requestId);
					return new ServiceResult
					{
						Success = false,
						Message = "Request not found"
					};
				}

				// Validate status transition
				if (!IsValidStatusTransition(request.Status, newStatus))
				{
					_logger.LogWarning(
						"Invalid status transition attempted. RequestId: {RequestId}, CurrentStatus: {CurrentStatus}, NewStatus: {NewStatus}",
						requestId, request.Status, newStatus);
					return new ServiceResult
					{
						Success = false,
						Message = "Invalid status transition"
					};
				}

				if (request.Status == newStatus)
				{
					_logger.LogInformation(
						"Request already in desired status. RequestId: {RequestId}, Status: {Status}",
						requestId, newStatus);
					return new ServiceResult
					{
						Success = true,
						Message = $"Request is already {newStatus}.",
						ObjectId = requestId
					};
				}

				// Update status
				request.Status = newStatus;
				request.ApproverOrRejectedId = approverId;

				await _businessData.UpdateRequestAsync(request);

				_logger.LogInformation(
					"Request status updated successfully. RequestId: {RequestId}, NewStatus: {NewStatus}",
					requestId, newStatus);

				// Post-update actions
				if (newStatus == UserRequestStatus.Approved)
				{
					await SendRequestApproveEmailAsync(request);

					var result = await CreateRequestMealConsumptionRecordsAsync(requestId);
					if (!result.Success)
					{
						_logger.LogWarning(
							"Failed to create meal consumption records after approval. RequestId: {RequestId}, Message: {Message}",
							requestId, result.Message);

						// Note: return a warning result but not rollback approval
						return new ServiceResult
						{
							Success = true,
							Message = $"Request approved but failed to create meal consumption records: {result.Message}",
							ObjectId = requestId
						};
					}
				}
				else if (newStatus == UserRequestStatus.Rejected)
				{
					await SendRequestRejectEmailAsync(request, rejectReason);
				}

				return new ServiceResult
				{
					Success = true,
					Message = $"Request status updated to {newStatus} successfully.",
					ObjectId = requestId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating request status. RequestId: {RequestId}", requestId);
				return new ServiceResult
				{
					Success = false,
					Message = "An error occurred while updating the request status."
				};
			}
		}


		public async Task<ServiceResult> ProcessLogicAsync(int tenantId, MealDeviceRequest request)
        {
            // It is generally better practice to use string identifiers (like "COMPANY_A") for tenants 
            // instead of hard-coded integers (like '1'), unless the 'tenantId' parameter is guaranteed 
            // to always be the database primary key. Assuming it's a string identifier now.

            switch (tenantId)
            {

                case 1: 
                          // Call the specific business logic handler for Company A (Hemas)
                    return await _companyBusinessLogic.HemasCompanyLogic(request);

                /*case "COMPANY_B":
                    // Placeholder for another company's logic
                    return await _companyBusinessLogic.HandleCompanyBLogic(request);
				*/
                default:
                    // Return a clear failure result if the tenant ID is unrecognized
                    return new ServiceResult
                    {
                        Success = false,
                        Message = $"Tenant ID '{tenantId}' is not configured for meal processing.",
                        
                    };
            }
        }

        public async Task<ServiceResult> UpdateMealConsumption(int mealConsumptionId, bool status, string jobStatus)
        {
            // Assume 'status' means whether the meal consumption record is finalized/validated (e.g., TockenIssued).

            // 1. Fetch the existing record from the database
            var consumptionRecord = await _businessData.GetMealConsumptionByIdAsync(mealConsumptionId);

            if (consumptionRecord == null)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = $"Meal Consumption record with ID {mealConsumptionId} not found.",
                };
            }
            consumptionRecord.TockenIssued = status;
			consumptionRecord.JobStatus = jobStatus;

            try
            {

				await _businessData.UpdateMealConsumptionAsync(consumptionRecord);
                // 4. Return success result
                return new ServiceResult
                {
                    Success = true,
                    Message = $"Meal Consumption ID {mealConsumptionId} successfully updated. TockenIssued: {status}.",
      
                };
            }
        
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = $"An error occurred while saving the update: {ex.Message}",
                };
            }
        }
		private async Task<ServiceResult> CreateRequestMealConsumptionRecordsAsync(int requestId)
		{
			try
			{
				// 1. Validate and get request
				var mealRequest = await _businessData.GetRequestByIdAsync(requestId);
				if (mealRequest == null)
				{
					_logger.LogWarning("Request not found for consumption creation. RequestId: {RequestId}", requestId);
					return new ServiceResult
					{
						Success = false,
						Message = "Request not found"
					};
				}

				// 2. Check if request is approved
				if (mealRequest.Status != UserRequestStatus.Approved)
				{
					_logger.LogWarning("Cannot create consumption for non-approved request. RequestId: {RequestId}, Status: {Status}",
						requestId, mealRequest.Status);
					return new ServiceResult
					{
						Success = false,
						Message = "Only approved requests can be converted to consumption records"
					};
				}

				// 3. Get request meals
				var requestMeals = await _businessData.GetRequestMealsAsync(requestId);
				if (!requestMeals.Any())
				{
					_logger.LogWarning("No meals found for request. RequestId: {RequestId}", requestId);
					return new ServiceResult
					{
						Success = false,
						Message = "No meal items found for this request"
					};
				}

				// 4. Get meal costs for all request meals
				var mealCostIds = requestMeals.Select(rm => rm.MealCostId).Distinct().ToList();
				var mealCosts = await _adminData.GetMealCostsByIdsAsync(mealCostIds);
				var mealCostDict = mealCosts.ToDictionary(mc => mc.MealCostId);

				// 5. Create consumption records
				var consumptionRecords = new List<RequestMealConsumption>();
				var skippedCount = 0;

				foreach (var requestMeal in requestMeals)
				{
					// Check if consumption already exists
					var exists = await _businessData.CheckIfConsumptionExistsAsync(
						requestId,
						requestMeal.MealTypeId,
						requestMeal.SubTypeId
					);

					if (exists)
					{
						skippedCount++;
						_logger.LogInformation("Consumption record already exists. RequestId: {RequestId}, MealTypeId: {MealTypeId}, SubTypeId: {SubTypeId}",
							requestId, requestMeal.MealTypeId, requestMeal.SubTypeId);
						continue; // Skip duplicates
					}

					// Get meal cost details
					if (!mealCostDict.TryGetValue(requestMeal.MealCostId, out var mealCost))
					{
						_logger.LogWarning("Meal cost not found. MealCostId: {MealCostId}", requestMeal.MealCostId);
						continue; // Skip if cost not found
					}

					// Calculate totals
					var totalEmployeeContribution = mealCost.EmployeeCost * requestMeal.Quantity;
					var totalCompanyContribution = mealCost.CompanyCost * requestMeal.Quantity;
					var totalSupplierContribution = mealCost.SupplierCost * requestMeal.Quantity;
					var totalSellingPrice = mealCost.SellingPrice * requestMeal.Quantity;

					// Create consumption record
					var consumption = new RequestMealConsumption
					{
						TenantId = requestMeal.TenantId,
						RequestId = requestId,
                        EventType = mealRequest.EventType,
                        EventDescription = mealRequest.Description,
						MealTypeId = requestMeal.MealTypeId,
						SubTypeId = requestMeal.SubTypeId,
						MealCostId = requestMeal.MealCostId,
						EventDate = mealRequest.EventDate,
						Quantity = (int)requestMeal.Quantity,
						SupplierId = mealCost.SupplierId,
						TotalEmployeeContribution = totalEmployeeContribution,
						TotalCompanyContribution = totalCompanyContribution,
						TotalSupplierCost = totalSupplierContribution,
						TotalSellingPrice = totalSellingPrice
					};

					consumptionRecords.Add(consumption);
				}

				// 6. Check if any records to create
				if (!consumptionRecords.Any())
				{
					_logger.LogInformation("All meal consumption records already exist. RequestId: {RequestId}", requestId);
					return new ServiceResult
					{
						Success = false,
						Message = skippedCount > 0
							? $"All {skippedCount} meal consumption records already exist"
							: "No meal consumption records to create"
					};
				}

				// 7. Save all consumption records
				await _businessData.CreateRequestMealConsumptionBulkAsync(consumptionRecords);

				_logger.LogInformation("Successfully created consumption records. RequestId: {RequestId}",requestId);

				return new ServiceResult
				{
					Success = true,
					Message = $"Successfully created consumption records"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating consumption records for RequestId: {RequestId}", requestId);
				return new ServiceResult
				{
					Success = false,
					Message = $"Error creating consumption records: {ex.Message}"
				};
			}
		}
		private bool IsValidStatusTransition(UserRequestStatus currentStatus, UserRequestStatus newStatus)
        {
           
            // A status can never transition back to itself unless it is intentionally allowed, 
            // but typically updates should change the state.
            if (currentStatus == newStatus)
            {
                return true;
            }

            switch (currentStatus)
            {
                case UserRequestStatus.Pending:
                    // A request can move from Pending to Approved, Rejected, or Expired.
                    return newStatus == UserRequestStatus.Approved ||
                           newStatus == UserRequestStatus.Rejected ||
                           newStatus == UserRequestStatus.Expired;

                case UserRequestStatus.Approved:
                    // Once a request is Approved, it is typically a terminal state and cannot be changed.
                    // If the request can be cancelled/revoked, add that status here (e.g., Canceled).
                    return false;

                case UserRequestStatus.Rejected:
                    // Once a request is Rejected, it is a terminal state and cannot be changed.
                    return false;

                case UserRequestStatus.Expired:
                    // Once a request has Expired (e.g., due to a timeout), it cannot be changed.
                    return false;

                default:
                    // Should not happen, but handles any future/unknown states safely.
                    return false;
            }
        }
		private async Task SendRequestSendEmailAsync(Request request)
		{
			try
			{
				if (request == null)
				{
					_logger.LogWarning("Request object is null. Cannot send request notification email.");
					return;
				}

				// Get all approvers (Admins + Department Heads)
				var approvers = await _businessData.GetUsersByDepartmentsAsync(_userContext.DepartmentIds);

				if (approvers == null || !approvers.Any())
				{
					_logger.LogWarning("No approvers found for departments {Departments}", string.Join(", ", _userContext.DepartmentIds));
					return;
				}

				// Collect approver emails
				var approverEmails = approvers
					.Select(u => _encryption.DecryptData(u.Email))
					.Where(email => !string.IsNullOrEmpty(email))
					.Distinct()
					.ToList();

				if (!approverEmails.Any())
				{
					_logger.LogWarning("No valid approver emails found for request {RequestId}", request.MealRequestId);
					return;
				}

				var requester = await _adminData.GetUserByIdAsync(request.RequesterId);
				var requesterEmail = _encryption.DecryptData(requester.Email);
				var requesterName = requester.FullName;

				// Subject and message for approvers
				var subject = $"New Request Pending Approval - {requesterName}";
				var message = _messageService.GenerateMealRequestMessage(requesterEmail, requesterName, request);

				var notificationRequest = new NotificationRequest
				{
					Emails = approverEmails,
					Subject = subject,
					Message = message,
					NotificationTypes = new List<NotificationRequest.NotificationType>
			        {
				        NotificationRequest.NotificationType.Email
			        }
				};

				var emailResult = await _emailNotification.SendEmail(notificationRequest);

				if (emailResult.IsSuccess)
				{
					_logger.LogInformation("Request notification email sent successfully for Meal request {RequestId}", request.MealRequestId);
				}
				else
				{
					_logger.LogWarning("Failed to send request notification email for meal request {RequestId}: {Error}",
						request.MealRequestId, string.Join(", ", emailResult.ErrorMessages));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending request notification email for meal request {RequestId}", request.MealRequestId);
			}
		}

		private async Task SendRequestApproveEmailAsync(Request request)
		{
			try
			{
				if (request == null)
				{
					_logger.LogWarning("Request object is null. Cannot send  notification email.");
					return;
				}

				var requester = await _adminData.GetUserByIdAsync(request.RequesterId);
				var requesterEmail = _encryption.DecryptData(requester.Email);
				var requesterName = requester.FullName;

				// Subject and message for approvers
				var subject = $"Meal Request Approved - {requesterName}";
				var message = _messageService.GenerateMealRequestApprovedMessage(request);
                var emails = new List<string> { requesterEmail };
				var notificationRequest = new NotificationRequest
				{
					Emails = emails,
					Subject = subject,
					Message = message,
					NotificationTypes = new List<NotificationRequest.NotificationType>
					{
						NotificationRequest.NotificationType.Email
					}
				};

				var emailResult = await _emailNotification.SendEmail(notificationRequest);

				if (emailResult.IsSuccess)
				{
					_logger.LogInformation("Approve notification email sent successfully for Meal request {RequestId}", request.MealRequestId);
				}
				else
				{
					_logger.LogWarning("Failed to send approve notification email for meal request {RequestId}: {Error}",
						request.MealRequestId, string.Join(", ", emailResult.ErrorMessages));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending aprrove notification email for meal request {RequestId}", request.MealRequestId);
			}
		}

		private async Task SendRequestRejectEmailAsync(Request request, string? rejectReason)
		{
			try
			{
				if (request == null)
				{
					_logger.LogWarning("Request object is null. Cannot send notification email.");
					return;
				}

				var requester = await _adminData.GetUserByIdAsync(request.RequesterId);
				var requesterEmail = _encryption.DecryptData(requester.Email);
				var requesterName = requester.FullName;

				// Subject and message for approvers
				var subject = $"Meal Request Rejected - {requesterName}";
				var message = _messageService.GenerateMealRequestRejectedMessage(request,rejectReason);
				var emails = new List<string> { requesterEmail };
				var notificationRequest = new NotificationRequest
				{
					Emails = emails,
					Subject = subject,
					Message = message,
					NotificationTypes = new List<NotificationRequest.NotificationType>
					{
						NotificationRequest.NotificationType.Email
					}
				};

				var emailResult = await _emailNotification.SendEmail(notificationRequest);

				if (emailResult.IsSuccess)
				{
					_logger.LogInformation("Reject notification email sent successfully for Meal request {RequestId}", request.MealRequestId);
				}
				else
				{
					_logger.LogWarning("Failed to send reject notification email for meal request {RequestId}: {Error}",
						request.MealRequestId, string.Join(", ", emailResult.ErrorMessages));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending reject notification email for meal request {RequestId}", request.MealRequestId);
			}
		}

	}

}