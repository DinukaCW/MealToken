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
        private readonly IBusinessRepository _businessData;
        private readonly ILogger<TokenProcessService> _logger;
        private readonly ITenantContext _tenantContext;
        private readonly IAdminRepository _adminData;
 
        public TokenProcessService(
            IBusinessRepository businessRepository,
            ILogger<TokenProcessService> logger,
            ITenantContext tenantContext,
            IAdminRepository adminData)
        {
            _businessData = businessRepository ?? throw new ArgumentNullException(nameof(businessRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _adminData = adminData ?? throw new ArgumentNullException(nameof(adminData));
        }

        public async Task<MealTokenResponse> GetMealDistributionAsync(MealTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Starting GetMealDistributionAsync for PersonId: {PersonId}, Date: {RequestDate}, Time: {RequestTime}",
                    request.PersonId, request.RequestDate, request.RequestTime);

                // Validate input
                if (request?.PersonId <= 0)
                {
                    _logger.LogWarning("Invalid PersonId: {PersonId}", request?.PersonId);
                    return new MealTokenResponse { Success = false, Message = "Invalid person ID" };
                }

                // Execute database queries sequentially to avoid DBContext threading issues
                var schedulesByPerson = await _businessData.GetListofShedulesforPersonAsync(request.PersonId);
                var schedulesByDate = await _businessData.GetListofShedulesforDateAsync(request.RequestDate);
                var scheduleMealsByTime = await _businessData.GetListofShedulesforTimeAsync(request.RequestTime);

                // Validate all results
                if (!schedulesByPerson.Any())
                {
                    _logger.LogWarning("No schedule assigned to PersonId: {PersonId}", request.PersonId);
                    return new MealTokenResponse { Success = false, Message = "No schedule assigned to this person" };
                }

                if (!schedulesByDate.Any())
                {
                    _logger.LogWarning("No schedules found on Date: {Date}", request.RequestDate);
                    return new MealTokenResponse { Success = false, Message = "No schedules found for the requested date" };
                }

                if (!scheduleMealsByTime.Any())
                {
                    _logger.LogWarning("No meals available for Time: {Time}", request.RequestTime);
                    return new MealTokenResponse { Success = false, Message = "No meals available for the requested time" };
                }

                // Find matching schedule efficiently using a single intersection
                var matchingScheduleIds = schedulesByPerson
                    .Select(s => s.SheduleId)
                    .Intersect(schedulesByDate.Select(s => s.SheduleId))
                    .Intersect(scheduleMealsByTime.Select(s => s.SheduleId))
                    .ToHashSet();

                if (!matchingScheduleIds.Any())
                {
                    _logger.LogWarning("No matching schedule found for PersonId: {PersonId}", request.PersonId);
                    return new MealTokenResponse { Success = false, Message = "No matching schedule found for the person, date, and time combination" };
                }

                // Select meal based on function key or default
                var selectedMeal = SelectMealByFunctionKeyOrDefault(scheduleMealsByTime, matchingScheduleIds, request.FunctionKey);
                if (selectedMeal == null)
                {
                    _logger.LogWarning("No available meal found for PersonId: {PersonId}", request.PersonId);
                    return new MealTokenResponse { Success = false, Message = "Invalid function key or meal not available for the requested time" };
                }

                // Get schedule details
                var selectedSchedule = await _businessData.GetScheduleByIdAsync(selectedMeal.SheduleId);
                if (selectedSchedule == null)
                {
                    _logger.LogWarning("Selected schedule {ScheduleId} is not active", selectedMeal.SheduleId);
                    return new MealTokenResponse { Success = false, Message = "Selected schedule is not active" };
                }

                var mealTypeName = await _adminData.GetMealTypeNameAsync(selectedMeal.MealTypeId);
                var addons = await _adminData.GetMealAddOnsAsync(selectedMeal.MealTypeId);

                var mealSubTypeName = selectedMeal.MealSubTypeId.HasValue
                    ? await _adminData.GetMealSubTypeNameAsync(selectedMeal.MealSubTypeId.Value)
                    : null;

                var addonMeals = await CreateAddOnsAsync(addons);

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
                        TokenIssueEndTime = selectedMeal.TokenIssueEndTime ?? TimeOnly.MaxValue,
                        MealAddOns = addonMeals
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

        private ScheduleMeal? SelectMealByFunctionKeyOrDefault(IEnumerable<ScheduleMeal> scheduleMeals, HashSet<int> matchingScheduleIds, string? functionKey)
        {
            var matchingMeals = scheduleMeals
                .Where(sm => matchingScheduleIds.Contains(sm.SheduleId) && sm.IsAvailable)
                .ToList();

            if (!matchingMeals.Any())
                return null;

			if (!string.IsNullOrEmpty(functionKey))
			{
				var matchedMeal = matchingMeals
					.FirstOrDefault(sm => sm.IsFunctionKeysEnable && sm.FunctionKey == functionKey);

				// If no valid meal found for the function key, return default meal instead
				if (matchedMeal != null)
					return matchedMeal;
			}

			return matchingMeals.First();
        }

        private async Task<List<MealAddOnDto>> CreateAddOnsAsync(IEnumerable<MealAddOn>? addons)
        {
            if (addons == null || !addons.Any())
                return new List<MealAddOnDto>();

            // Fetch meal type IDs sequentially to avoid DBContext threading issues
            int snacksId = await _adminData.GetMealTypeIdbyNameAsync("Snacks");
            int beverageId = await _adminData.GetMealTypeIdbyNameAsync("Beverages");

            var addOnList = new List<MealAddOnDto>(addons.Count());

            // Batch fetch all meal costs upfront
            var addonsByType = addons.GroupBy(a => a.AddOnType).ToDictionary(g => g.Key, g => g.ToList());
            var mealCostsMap = new Dictionary<(int mealTypeId, int subTypeId), MealCost?>();

            foreach (var addOn in addons)
            {
                int mealTypeId = addOn.AddOnType == AddOnType.Snacks ? snacksId : beverageId;
                var key = (mealTypeId, addOn.AddOnSubTypeId);

                if (!mealCostsMap.TryGetValue(key, out var mealCost))
                {
                    mealCost = await _adminData.GetMealCostByDetailsAsync(mealTypeId, addOn.AddOnSubTypeId);
                    mealCostsMap[key] = mealCost;
                }

                if (mealCost == null)
                {
                    _logger.LogWarning("No meal cost found for AddOnId {AddOnId} ({AddOnName})", addOn.AddOnSubTypeId, addOn.AddOnName);
                    continue;
                }

                addOnList.Add(new MealAddOnDto
                {
                    AddOnMealTypeId = mealTypeId,
                    AddOnSubTypeId = addOn.AddOnSubTypeId,
                    AddOnName = addOn.AddOnName,
                    AddonType = addOn.AddOnType,
                    SupplierId = mealCost.SupplierId
                });
            }

            return addOnList;
        }

    }
}
