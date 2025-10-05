using Authentication.Interfaces;
using Authentication.Models.DTOs;
using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Interfaces;
using MealToken.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Services
{
    public class TokenProcessService : ITokenProcessService
    {
        private readonly IEncryptionService _encryption;
        private readonly IBusinessRepository _businessData;
        private readonly ILogger<TokenProcessService> _logger;
        private readonly ITenantContext _tenantContext;
        private readonly IAdminRepository _adminData;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenProcessService(
            IEncryptionService encryptionService,
            IBusinessRepository businessRepository,
            ILogger<TokenProcessService> logger,
            ITenantContext tenantContext,
            IAdminRepository adminData,
            IHttpContextAccessor httpContextAccessor)
        {
            _encryption = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _businessData = businessRepository ?? throw new ArgumentNullException(nameof(businessRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _adminData = adminData ?? throw new ArgumentNullException(nameof(adminData));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<MealTokenResponse> GetMealDistributionAsync(MealTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Starting GetMealDistributionAsync for PersonId: {PersonId}, Date: {RequestDate}, Time: {RequestTime}",
                    request.PersonId, request.RequestDate, request.RequestTime);

                // Step 1: Get schedules filtered by person
                var schedulesByPerson = await _businessData.GetListofShedulesforPersonAsync(request.PersonId);
                if (!schedulesByPerson.Any())
                {
                    _logger.LogWarning("No schedule assigned to PersonId: {PersonId}", request.PersonId);
                    return new MealTokenResponse { Success = false, Message = "No schedule assigned to this person" };
                }

                // Step 2: Get schedules filtered by date
                var schedulesByDate = await _businessData.GetListofShedulesforDateAsync(request.RequestDate);
                if (!schedulesByDate.Any())
                {
                    _logger.LogWarning("No schedules found on Date: {Date}", request.RequestDate);
                    return new MealTokenResponse { Success = false, Message = "No schedules found for the requested date" };
                }

                // Step 3: Get schedule meals filtered by time
                var scheduleMealsByTime = await _businessData.GetListofShedulesforTimeAsync(request.RequestTime);
                if (!scheduleMealsByTime.Any())
                {
                    _logger.LogWarning("No meals available for Time: {Time}", request.RequestTime);
                    return new MealTokenResponse { Success = false, Message = "No meals available for the requested time" };
                }

                // Step 4: Find the intersection of all three lists to get the matching schedule
                var personScheduleIds = schedulesByPerson.Select(sp => sp.SheduleId).ToHashSet();
                var dateScheduleIds = schedulesByDate.Select(sd => sd.SheduleId).ToHashSet();
                var timeScheduleIds = scheduleMealsByTime.Select(sm => sm.SheduleId).ToHashSet();

                var matchingScheduleIds = personScheduleIds.Intersect(dateScheduleIds).Intersect(timeScheduleIds).ToList();
                if (!matchingScheduleIds.Any())
                {
                    _logger.LogWarning("No matching schedule found for PersonId: {PersonId}", request.PersonId);
                    return new MealTokenResponse { Success = false, Message = "No matching schedule found for the person, date, and time combination" };
                }

                // Step 5: Handle function key or get the first matching schedule
                int selectedScheduleId;
                ScheduleMeal? selectedMeal = null;

                if (!string.IsNullOrEmpty(request.FunctionKey))
                {
                    selectedMeal = scheduleMealsByTime
                        .FirstOrDefault(sm => matchingScheduleIds.Contains(sm.SheduleId) &&
                                              sm.IsAvailable &&
                                              sm.IsFunctionKeysEnable &&
                                              sm.FunctionKey == request.FunctionKey);

                    if (selectedMeal == null)
                    {
                        _logger.LogWarning("Invalid function key {FunctionKey} for PersonId: {PersonId}", request.FunctionKey, request.PersonId);
                        return new MealTokenResponse { Success = false, Message = "Invalid function key or meal not available for the requested time" };
                    }

                    selectedScheduleId = selectedMeal.SheduleId;
                }
                else
                {
                    selectedScheduleId = matchingScheduleIds.First();
                    selectedMeal = scheduleMealsByTime.FirstOrDefault(sm => sm.SheduleId == selectedScheduleId && sm.IsAvailable);

                    if (selectedMeal == null)
                    {
                        _logger.LogWarning("No available meal found for ScheduleId: {ScheduleId}", selectedScheduleId);
                        return new MealTokenResponse { Success = false, Message = "No available meal found in the selected schedule" };
                    }
                }

                // Step 6: Get the schedule details
                var selectedSchedule = await _businessData.GetScheduleByIdAsync(selectedScheduleId);
                if (selectedSchedule == null)
                {
                    _logger.LogWarning("Selected schedule {ScheduleId} is not active", selectedScheduleId);
                    return new MealTokenResponse { Success = false, Message = "Selected schedule is not active" };
                }

                // Step 7: Get meal type and subtype names
                var mealTypeName = await _adminData.GetMealTypeNameAsync(selectedMeal.MealTypeId);

                string? mealSubTypeName = null;
                if (selectedMeal.MealSubTypeId.HasValue)
                {
                    mealSubTypeName = await _adminData.GetMealSubTypeNameAsync(selectedMeal.MealSubTypeId.Value);
                }

                // Step 8: Build response
                _logger.LogInformation("Meal distribution found successfully for PersonId: {PersonId}, ScheduleId: {ScheduleId}",
                    request.PersonId, selectedSchedule.SheduleId);

                return new MealTokenResponse
                {
                    Success = true,
                    Message = "Meal distribution found successfully",
                    MealInfo = new MealDistributionInfo
                    {
                        ScheduleId = selectedSchedule.SheduleId,
                        ScheduleName = selectedSchedule.SheduleName,
                        MealTypeId = selectedMeal.MealTypeId,
                        MealSubTypeId = selectedMeal.MealSubTypeId,
                        MealTypeName = mealTypeName,
                        MealSubTypeName = mealSubTypeName,
                        SupplierId = selectedMeal.SupplierId,
                        TokenIssueStartTime = selectedMeal.TokenIssueStartTime ?? TimeOnly.MinValue,
                        TokenIssueEndTime = selectedMeal.TokenIssueEndTime ?? TimeOnly.MaxValue
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetMealDistributionAsync for PersonId: {PersonId}", request.PersonId);
                return new MealTokenResponse
                {
                    Success = false,
                    Message = "An error occurred while processing the meal distribution request"
                };
            }
        }
    }
}
