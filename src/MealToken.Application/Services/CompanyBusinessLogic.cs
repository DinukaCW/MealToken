using Authentication.Interfaces;
using Authentication.Models.DTOs;
using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Interfaces;
using MealToken.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Services
{
    public class CompanyBusinessLogic : ICompanyBusinessLogic
    {
        private readonly IEncryptionService _encryption;
        private readonly IBusinessRepository _businessData;
        private readonly ILogger<TokenProcessService> _logger;
        private readonly ITenantContext _tenantContext;
        private readonly IAdminRepository _adminData;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenProcessService _tokenProcessService;

        public CompanyBusinessLogic(IEncryptionService encryptionService,
            IBusinessRepository businessRepository,
            ILogger<TokenProcessService> logger,
            ITenantContext tenantContext,
            IAdminRepository adminData,
            IHttpContextAccessor httpContextAccessor,
            ITokenProcessService tokenProcessService)
        {
            _encryption = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _businessData = businessRepository ?? throw new ArgumentNullException(nameof(businessRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _adminData = adminData ?? throw new ArgumentNullException(nameof(adminData));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _tokenProcessService = tokenProcessService;
        }

        public async Task<ServiceResult> HemasCompanyLogic(MealDeviceRequest request)
        {
            // Validate person
            var person = await _adminData.GetPersonByNumberAsync(request.PersonNumber);
            if (person == null)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Person not found"
                };
            }

            if (person.IsActive != true)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Person is not active"
                };
            }

            // Prepare meal token request
            var mealtokenRequest = new MealTokenRequest
            {
                PersonId = person.PersonId,
                RequestDate = DateOnly.FromDateTime(request.DateTime),
                RequestTime = TimeOnly.FromDateTime(request.DateTime),
                FunctionKey = request.FunctionKey
            };

            // Get meal schedule
            var schedule = await _tokenProcessService.GetMealDistributionAsync(mealtokenRequest);
            if (!schedule.Success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = schedule.Message
                };
            }

            var scheduleMeals = schedule.MealInfo;
            if (scheduleMeals == null)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "No meal schedule found for this time"
                };
            }

            // Check if meal already issued
            var exMealConsumption = await _businessData.GetMealCosumptionAsync(
                scheduleMeals.MealTypeId,
                person.PersonId,
                mealtokenRequest.RequestDate,
                mealtokenRequest.RequestTime
            );

            if (exMealConsumption != null && exMealConsumption.TockenIssued)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Meal token already issued for this meal type"
                };
            }

            // Validate device
            int deviceId = await _businessData.GetDeviceBySerialNoAsync(request.DeviceSerialNo);
            if (deviceId == 0)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Invalid device serial number"
                };
            }

            // Get meal history for shift detection
            var todaysMeals = await _businessData.GetMealConsumptionByDateAsync(
                person.PersonId,
                mealtokenRequest.RequestDate
            );

            var lastMealIn13Hours = await _businessData.GetMealConsumptionInLast13HoursAsync(
                person.PersonId
            );

            // Define time boundaries
            var dayStart = new TimeOnly(7, 0);        // 7:00 AM
            var dayEnd = new TimeOnly(19, 0);         // 7:00 PM
            var extendedDayEnd = new TimeOnly(22, 15); // 10:15 PM

            // Identify shift
            Shift detectedShift = IdentifyShift(
                mealtokenRequest.RequestTime,
                deviceId,
                todaysMeals,
                lastMealIn13Hours,
                dayStart,
                dayEnd,
                extendedDayEnd
            );

            PayStatus payStatus = PayStatus.Free; // Set a safe default (or null, depending on your wider logic)

            if (person.PersonType == PersonType.Employer)
            {
                var policy = await _businessData.GetPayPolicyAsync(detectedShift, scheduleMeals.MealTypeId);

                if (policy != null)
                {
                    if (person.Gender == "Male")
                    {
                        payStatus = policy.IsMalePaid ? PayStatus.Paid : PayStatus.Free;
                    }
                    else if (person.Gender == "Female")
                    {

                        payStatus = policy.IsFemalePaid ? PayStatus.Paid : PayStatus.Free;
                    }

                }
                else
                {
                    payStatus = PayStatus.Free;
                }
            }

            var mealCost = await _adminData.GetMealCostByDetailAsync(scheduleMeals.SupplierId,scheduleMeals.MealTypeId, scheduleMeals.MealSubTypeId);

            // Create meal consumption record
            var mealConsumption = new MealConsumption
            {
                TenantId = person.TenantId, // Assuming person has TenantId
                PersonId = person.PersonId,
                Date = mealtokenRequest.RequestDate,
                Time = mealtokenRequest.RequestTime,
                MealTypeId = scheduleMeals.MealTypeId,
                SubTypeId = scheduleMeals.MealSubTypeId, // If applicable
                MealCostId = mealCost.MealCostId, // If applicable
                DeviceId = deviceId,
                ShiftName = detectedShift,
                PayStatus = payStatus,
                TockenIssued = true
            };

            // Save to database
            await _businessData.CreateMealConsumptionAsync(mealConsumption);
            string department = await _adminData.GetDepartmentByIdAsync(person.DepartmentId);
            var tokenResponse = new TokenResponse
            {
                Date = mealtokenRequest.RequestDate,
                Time = mealtokenRequest.RequestTime,
                Shift = detectedShift.ToString(),
                EmpNo = request.PersonNumber,
                Department = department,
                TokenType = payStatus.ToString(),
                MealConsumptionId = mealConsumption.MealConsumptionId
            };
            return new ServiceResult
            {
                Success = true,
                Message = $"Meal issued successfully. Shift: {detectedShift}, Status: {payStatus}",
                Data = tokenResponse
            };
        }

        private Shift IdentifyShift(
            TimeOnly currentTime,
            int deviceId,
            List<MealConsumption> todaysMeals,
            MealConsumption lastMealIn13Hours,
            TimeOnly dayStart,
            TimeOnly dayEnd,
            TimeOnly extendedDayEnd)
        {
            // Determine current time period
            bool isInDayHours = currentTime >= dayStart && currentTime < dayEnd;
            bool isInExtendedDayHours = currentTime >= dayEnd && currentTime < extendedDayEnd;
            bool isInNightHours = currentTime >= extendedDayEnd || currentTime < dayStart;

            // Check if person had meals in different shift periods today
            bool hadDayShiftMeal = todaysMeals?.Any(m =>
                m.Time >= dayStart &&
                m.Time < dayEnd &&
                (m.ShiftName == Shift.DayShift || m.ShiftName == Shift.DayShiftExtended)
            ) ?? false;

            bool hadNightShiftMeal = todaysMeals?.Any(m =>
                (m.Time >= dayEnd || m.Time < dayStart) &&
                m.ShiftName == Shift.NightShift
            ) ?? false;

            // Check if had early morning night shift meal (before 7 AM today)
            bool hadEarlyMorningNightMeal = todaysMeals?.Any(m =>
                m.Time < dayStart &&
                m.ShiftName == Shift.NightShift
            ) ?? false;

            // SCENARIO 1: Current request is during DAY HOURS (7:00 AM - 7:00 PM)
            if (isInDayHours)
            {
                if (deviceId == 1) // Day shift device
                {
                    // Check if they had night shift meal recently
                    // This means: Night shift worker who continued working into day
                    if (hadEarlyMorningNightMeal ||
                        hadNightShiftMeal ||
                        lastMealIn13Hours?.ShiftName == Shift.NightShift)
                    {
                        // Started in night shift, now working in day
                        return Shift.NightandDayShift;
                    }

                    // Normal day shift
                    return Shift.DayShift;
                }
                else if (deviceId == 2) // Night shift device during day hours
                {
                    // Using night device during day = night worker extending into day
                    return Shift.NightandDayShift;
                }
            }

            // SCENARIO 2: Current request is during EXTENDED DAY HOURS (7:00 PM - 10:15 PM)
            if (isInExtendedDayHours)
            {
                if (deviceId == 1) // Day device in extended hours
                {
                    // If they had day shift meal earlier today
                    if (hadDayShiftMeal)
                    {
                        // Day worker working late = Extended day shift
                        return Shift.DayShiftExtended;
                    }

                    // Check if last meal in 13 hours was during day shift
                    if (lastMealIn13Hours != null &&
                        lastMealIn13Hours.Time >= dayStart &&
                        lastMealIn13Hours.Time < dayEnd &&
                        (lastMealIn13Hours.ShiftName == Shift.DayShift ||
                         lastMealIn13Hours.ShiftName == Shift.DayShiftExtended))
                    {
                        return Shift.DayShiftExtended;
                    }

                    // No day meal history = probably early night shift starting
                    return Shift.DayShiftExtended;
                }
                else if (deviceId == 2) // Night device in extended hours
                {
                    // Night device = normal night shift
                    return Shift.NightShift;
                }
            }

            // SCENARIO 3: Current request is during NIGHT HOURS (10:15 PM - 7:00 AM)
            if (isInNightHours)
            {
                if (deviceId == 2) // Night shift device
                {
                    // Normal night shift
                    return Shift.NightShift;
                }
                else if (deviceId == 1) // Day device during night hours
                {
                    // If they had a day meal earlier, they're extending into night
                    if (hadDayShiftMeal ||
                        (lastMealIn13Hours?.ShiftName == Shift.DayShift) ||
                        (lastMealIn13Hours?.ShiftName == Shift.DayShiftExtended))
                    {
                        // Day worker extending into night
                        return Shift.DayandNightShift;
                    }

                    // No day meal history = might be night shift starting early or using wrong device
                    // Default to night-to-day pattern (benefit of doubt)
                    return Shift.DayandNightShift;
                }
            }

            // Fallback: Should rarely reach here
            return isInDayHours ? Shift.DayShift : Shift.NightShift;
        }

       
    }
}
