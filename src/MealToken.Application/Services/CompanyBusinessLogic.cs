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
            try
            { // Validate person
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
                var exMealConsumption = await _businessData.GetMealConsumptionAsync(
                    scheduleMeals.MealTypeId,
                    person.PersonId,
                    mealtokenRequest.RequestDate
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
                var deviceShift = await _businessData.GetDeviceShiftBySerialNoAsync(request.DeviceSerialNo) ?? DeviceShift.Day;
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

                var shiftResult = ValidateAndIdentifyShift(
                mealtokenRequest.RequestTime,
                deviceShift,
                todaysMeals,
                lastMealIn13Hours,
                dayStart,
                dayEnd,
                extendedDayEnd,
                out Shift detectedShift
                 );

                if (!shiftResult.Success)
                {
                    _logger.LogWarning($"Shift validation failed: {shiftResult.Message}, Person: {person.PersonId}, Device: {deviceShift}");
                    return shiftResult;
                }

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
                        else
                        {
                            // Unknown gender - default to paid (safer)
                            _logger.LogWarning($"Unknown gender for Person: {person.PersonId}, Gender: {person.Gender}");
                            payStatus = PayStatus.Paid;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No payment policy found for Shift: {detectedShift}, MealType: {scheduleMeals.MealTypeId}");
                        payStatus = PayStatus.Paid;
                    }
                }
             
                
                var mealCost = await _adminData.GetMealCostByDetailAsync(scheduleMeals.SupplierId, scheduleMeals.MealTypeId, scheduleMeals.MealSubTypeId);

                if (mealCost == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Meal cost not found for this meal configuration"
                    };
                }
                decimal empContribution = (payStatus == PayStatus.Free)
                    ? 0
                    : mealCost?.EmployeeCost ?? 0;

                decimal companyContribution = (payStatus == PayStatus.Free)
                    ? ((mealCost?.EmployeeCost ?? 0) + (mealCost?.CompanyCost ?? 0))
                    : mealCost?.CompanyCost ?? 0;

                var mealConsumption = new MealConsumption
                {
                    TenantId = person.TenantId, // Assuming person has TenantId
                    PersonId = person.PersonId,
                    PersonName = person.Name,
                    Date = mealtokenRequest.RequestDate,
                    SchduleId = scheduleMeals.ScheduleId,
                    SchduleName = scheduleMeals.ScheduleName,
                    Time = mealtokenRequest.RequestTime,
                    MealTypeId = scheduleMeals.MealTypeId,
                    MealTypeName = scheduleMeals.MealTypeName,
                    SubTypeId = scheduleMeals.MealSubTypeId, // If applicable
                    SubTypeName = scheduleMeals.MealSubTypeName, // If applicable
                    MealCostId = mealCost.MealCostId, // If applicable
                    SupplierCost = mealCost.SupplierCost,
                    SellingPrice = mealCost.SellingPrice,
                    CompanyCost = companyContribution,
                    EmployeeCost = empContribution,
                    DeviceId = deviceId,
                    DeviceSerialNo = request.DeviceSerialNo,
                    ShiftName = detectedShift,
                    PayStatus = payStatus,
                    TockenIssued = false                  
                };

                // Save to database
                await _businessData.CreateMealConsumptionAsync(mealConsumption);
                string department = await _adminData.GetDepartmentByIdAsync(person.DepartmentId) ?? "N/A";
                var tokenResponse = new TokenResponse
                {
                    Date = mealtokenRequest.RequestDate,
                    Time = mealtokenRequest.RequestTime,
                    MealType = scheduleMeals.MealSubTypeId != null
                            ? scheduleMeals.MealSubTypeName
                            : scheduleMeals.MealTypeName,
                    Shift = detectedShift.ToString(),
                    EmpNo = request.PersonNumber,
                    EmpName = person.Name,
                    Gender = person.Gender,
                    Department = department,
                    TokenType = payStatus.ToString(),
                    Contribution = empContribution,
                    MealConsumptionId = mealConsumption.MealConsumptionId
                };
                return new ServiceResult
                {
                    Success = true,
                    Message = $"Meal issued successfully. Shift: {detectedShift}, Status: {payStatus}",
                    Data = tokenResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing meal request for Person: {request.PersonNumber}, Device: {request.DeviceSerialNo}");
                return new ServiceResult
                {
                    Success = false,
                    Message = "An error occurred while processing your meal request. Please contact support."
                };
            }
    }

private ServiceResult ValidateAndIdentifyShift(
      TimeOnly currentTime,
      DeviceShift deviceShift,
      List<MealConsumption> todaysMeals,
      MealConsumption lastMealIn13Hours,
      TimeOnly dayStart,
      TimeOnly dayEnd,
      TimeOnly extendedDayEnd,
      out Shift detectedShift)
        {
            detectedShift = Shift.DayShift; // Default

            // Determine current time period
            bool isInDayHours = currentTime >= dayStart && currentTime < dayEnd;
            bool isInExtendedDayHours = currentTime >= dayEnd && currentTime < extendedDayEnd;
            bool isInNightHours = currentTime >= extendedDayEnd || currentTime < dayStart;

            // Check meal history
            bool hadDayShiftMeal = todaysMeals?.Any(m =>
                m.Time >= dayStart &&
                m.Time < dayEnd &&
                (m.ShiftName == Shift.DayShift ||
                 m.ShiftName == Shift.DayShiftExtended ||
                 m.ShiftName == Shift.DayandNightShift)
            ) ?? false;

            bool hadNightShiftMeal = todaysMeals?.Any(m =>
                (m.Time >= dayEnd || m.Time < dayStart) &&
                (m.ShiftName == Shift.NightShift ||
                 m.ShiftName == Shift.NightandDayShift ||
                 m.ShiftName == Shift.DayandNightShift)
            ) ?? false;

            bool hadEarlyMorningNightMeal = todaysMeals?.Any(m =>
                m.Time < dayStart &&
                (m.ShiftName == Shift.NightShift ||
                 m.ShiftName == Shift.NightandDayShift ||
                 m.ShiftName == Shift.DayandNightShift)
            ) ?? false;

            bool hadDayShiftExtendedMeal = todaysMeals?.Any(m =>
                m.Time >= dayEnd &&
                m.Time < extendedDayEnd &&
                (m.ShiftName == Shift.DayShiftExtended ||
                 m.ShiftName == Shift.DayandNightShift)
            ) ?? false;

            if (isInDayHours)
            {
                if (deviceShift == DeviceShift.Day)
                {
                    // Check if continuing from night shift
                    if (hadEarlyMorningNightMeal ||
                        hadNightShiftMeal ||
                        lastMealIn13Hours?.ShiftName == Shift.NightShift ||
                        lastMealIn13Hours?.ShiftName == Shift.NightandDayShift ||
                        lastMealIn13Hours?.ShiftName == Shift.DayandNightShift)
                    {
                        // Night worker continuing into day
                        // Dinner (9 PM yesterday) → ... → Lunch (12 PM today)
                        detectedShift = Shift.DayandNightShift;
                        return new ServiceResult { Success = true };
                    }

                    // Normal day shift
                    detectedShift = Shift.DayShift;
                    return new ServiceResult { Success = true };
                }
                else if (deviceShift == DeviceShift.Night)
                {
                 
                    if (hadEarlyMorningNightMeal ||
                        hadNightShiftMeal ||
                        lastMealIn13Hours?.ShiftName == Shift.NightShift ||
                        lastMealIn13Hours?.ShiftName == Shift.NightandDayShift)
                    {
                        detectedShift = Shift.NightandDayShift;
                        return new ServiceResult { Success = true };
                    }

                    // No night meal history = WRONG DEVICE
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Wrong device! Please use DAY device during day shift hours (7 AM - 7 PM)."
                    };
                }
            }

            if (isInExtendedDayHours)
            {
                if (deviceShift == DeviceShift.Day)
                {
                    // Check if they had day shift meal earlier
                    if (hadDayShiftMeal)
                    {
                        // Day worker working until 10 PM
                        // Breakfast (8 AM) → ... → Dinner (9 PM)
                        detectedShift = Shift.DayShiftExtended;
                        return new ServiceResult { Success = true };
                    }

                    // Check if last meal was during day
                    if (lastMealIn13Hours != null &&
                        lastMealIn13Hours.Time >= dayStart &&
                        lastMealIn13Hours.Time < dayEnd &&
                        (lastMealIn13Hours.ShiftName == Shift.DayShift ||
                         lastMealIn13Hours.ShiftName == Shift.DayShiftExtended))
                    {
                        detectedShift = Shift.DayShiftExtended;
                        return new ServiceResult { Success = true };
                    }

                    detectedShift = Shift.DayShiftExtended;
                }
                else if (deviceShift == DeviceShift.Night)
                {
                    // Night device during 7-10 PM = normal night shift start
                    detectedShift = Shift.NightShift;
                    return new ServiceResult { Success = true };
                }
            }

            if (isInNightHours)
            {
                if (deviceShift == DeviceShift.Night)
                {
                    // Check if continuing from day shift
                    if (hadDayShiftMeal ||
                        hadDayShiftExtendedMeal ||
                        lastMealIn13Hours?.ShiftName == Shift.DayShift ||
                        lastMealIn13Hours?.ShiftName == Shift.DayShiftExtended)
                    {
                        // Day worker continuing into night
                        // Breakfast (8 AM) → ... → Midnight Tea (12 AM)
                        detectedShift = Shift.DayandNightShift;
                        return new ServiceResult { Success = true };
                    }

                    // Normal night shift
                    detectedShift = Shift.NightShift;
                    return new ServiceResult { Success = true };
                }
                else if (deviceShift == DeviceShift.Day)
                {
                    // ⚠️ DEVICE VALIDATION: Day device during night hours

                    // Check if continuing from day shift (legitimate use)
                    if (hadDayShiftMeal ||
                        hadDayShiftExtendedMeal ||
                        lastMealIn13Hours?.ShiftName == Shift.DayShift ||
                        lastMealIn13Hours?.ShiftName == Shift.DayShiftExtended)
                    {
                        detectedShift = Shift.DayandNightShift;
                        return new ServiceResult { Success = true };
                    }

                    // Early morning (12 AM - 7 AM) could be night shift continuing
                    if (currentTime < dayStart)
                    {
                        // Check if they have night meal history
                        if (hadNightShiftMeal ||
                            lastMealIn13Hours?.ShiftName == Shift.NightShift)
                        {
                            return new ServiceResult
                            {
                                Success = false,
                                Message = "Wrong device! Please use NIGHT device during night shift hours (10:15 PM - 7:00 AM)."
                            };
                        }
                    }

                    // No valid history = WRONG DEVICE
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Wrong device! Please use NIGHT device during night shift hours (10:15 PM - 7:00 AM)."
                    };
                }
            }

            // Fallback
            detectedShift = isInDayHours ? Shift.DayShift : Shift.NightShift;
            return new ServiceResult { Success = true };
        }


    }
}
