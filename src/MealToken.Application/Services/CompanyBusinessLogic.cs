using Authentication.Interfaces;
using Authentication.Models.DTOs;
using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Interfaces;
using MealToken.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAParser;

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
			{
				// Validate person
				var person = await _adminData.GetPersonByNumberAsync(request.PersonNumber);
				if (person == null)
				{
					return new ServiceResult { Success = false, Message = "Person not found" };
				}

				if (person.IsActive != true)
				{
					return new ServiceResult { Success = false, Message = "Person is not active" };
				}

				// Prepare meal token request - extract date/time once
				var requestDate = DateOnly.FromDateTime(request.DateTime);
				var requestTime = TimeOnly.FromDateTime(request.DateTime);

				var mealtokenRequest = new MealTokenRequest
				{
					PersonId = person.PersonId,
					RequestDate = requestDate,
					RequestTime = requestTime,
					FunctionKey = request.FunctionKey
				};

				// Get meal schedule with addon meals
				var schedule = await _tokenProcessService.GetMealDistributionAsync(mealtokenRequest);
				if (!schedule.Success)
				{
					return new ServiceResult { Success = false, Message = schedule.Message };
				}

				var scheduleMeals = schedule.MealInfo;
				if (scheduleMeals == null)
				{
					return new ServiceResult { Success = false, Message = "No meal schedule found for this time" };
				}

				// Check if meal already issued (Employer only)
				if (person.PersonType == PersonType.Employer)
				{
					var exMealConsumption = await _businessData.GetMealConsumptionAsync(
						scheduleMeals.MealTypeId,
						person.PersonId,
						requestDate
					);

					if (exMealConsumption?.TockenIssued == true)
					{
						return new ServiceResult { Success = false, Message = "Meal token already issued for this meal type" };
					}
				}

				// Validate device - parallel independent queries
				var deviceId = await _businessData.GetDeviceBySerialNoAsync(request.DeviceSerialNo);
				if (deviceId == 0)
				{
					return new ServiceResult { Success = false, Message = "Invalid device serial number" };
				}

				var deviceShift = await _businessData.GetDeviceShiftBySerialNoAsync(request.DeviceSerialNo) ?? DeviceShift.Day;

				// Get meal history for shift detection
				var todaysMeals = await _businessData.GetMealConsumptionByDateAsync(person.PersonId, requestDate);
				var lastMealIn13Hours = await _businessData.GetMealConsumptionInLast13HoursAsync(person.PersonId);

				// Define time boundaries (const values)
				var dayStart = new TimeOnly(7, 0);
				var dayEnd = new TimeOnly(19, 0);
				var extendedDayEnd = new TimeOnly(22, 15);

				// Validate shift
				var shiftResult = ValidateAndIdentifyShift(
					requestTime,
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
					_logger.LogWarning("Shift validation failed: {Message}, Person: {PersonId}, Device: {DeviceShift}",
						shiftResult.Message, person.PersonId, deviceShift);
					return shiftResult;
				}

				// Determine pay status
				var payStatus = await DeterminPayStatusAsync(person, detectedShift, scheduleMeals.MealTypeId);

				// Get meal cost
				var mealCost = await _adminData.GetMealCostByDetailAsync(
					scheduleMeals.SupplierId,
					scheduleMeals.MealTypeId,
					scheduleMeals.MealSubTypeId
				);

				if (mealCost == null)
				{
					return new ServiceResult { Success = false, Message = "Meal cost not found for this meal configuration" };
				}

				// Calculate contributions
				var isFreeMeal = payStatus == PayStatus.Free;
				var employeeCost = mealCost.EmployeeCost;
				var companyCost = mealCost.CompanyCost;

				decimal empContribution = isFreeMeal ? 0 : employeeCost;
				decimal companyContribution = isFreeMeal ? (mealCost.SupplierCost) : companyCost;

				// Create main meal consumption record
				var mealConsumption = new MealConsumption
				{
					TenantId = person.TenantId,
					PersonId = person.PersonId,
					PersonName = person.Name,
					Gender = person.Gender,
					Date = requestDate,
					Time = requestTime,
					ScheduleId = scheduleMeals.ScheduleId,
					ScheduleName = scheduleMeals.ScheduleName,
					AddOnMeal = false,
					MealTypeId = scheduleMeals.MealTypeId,
					MealTypeName = scheduleMeals.MealTypeName,
					SubTypeId = scheduleMeals.MealSubTypeId,
					SubTypeName = scheduleMeals.MealSubTypeName,
					MealCostId = mealCost.MealCostId,
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

				// Save main meal
				await _businessData.CreateMealConsumptionAsync(mealConsumption);

				// Create addon meal consumption records
				List<MealConsumption> addonMealConsumptions = null;
				if (scheduleMeals.MealAddOns?.Any() == true)
				{
					addonMealConsumptions = await CreateAddOnMealConsumptionsAsync(
						scheduleMeals.MealAddOns,
						mealConsumption,
						person,
						detectedShift,
						mealtokenRequest
					);

					if (addonMealConsumptions?.Any() == true)
					{
						await _businessData.CreateMealConsumptionBatchAsync(addonMealConsumptions);
						_logger.LogInformation("Created {AddOnCount} addon meal consumption records for PersonId: {PersonId}",
							addonMealConsumptions.Count, person.PersonId);
					}
				}

				// Prepare token response
				var department = await _adminData.GetDepartmentByIdAsync(person.DepartmentId) ?? "N/A";

				// Build meal type display text
				var mealTypeDisplay = scheduleMeals.MealSubTypeId.HasValue
					? scheduleMeals.MealSubTypeName
					: scheduleMeals.MealTypeName;

				// Build addon meals text
				var addonMealsText = addonMealConsumptions?.Any() == true
					? " With " + string.Join(", ", addonMealConsumptions.Select(a => a.SubTypeName))
					: string.Empty;

				var tokenResponse = new TokenResponse
				{
					Date = requestDate,
					Time = requestTime,
					MealType = mealTypeDisplay + addonMealsText,
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
				_logger.LogError(ex, "Error processing meal request for Person: {PersonNumber}, Device: {DeviceSerialNo}",
					request.PersonNumber, request.DeviceSerialNo);
				return new ServiceResult
				{
					Success = false,
					Message = "An error occurred while processing your meal request. Please contact support."
				};
			}
		}

		public async Task<ServiceResult> ManualPrintTokenLostAsync(int personId,DateTime reqDateTime)
		{
			_logger.LogInformation("ManualPrintTokenLost process started for PersonId: {PersonId}", personId);

			try
			{
				var person = await _adminData.GetPersonByIdAsync(personId);
				if (person == null)
				{
					_logger.LogWarning("ManualPrintTokenLost failed - Person not found. PersonId: {PersonId}", personId);
					return new ServiceResult { Success = false, Message = "Person not found." };
				}
				_logger.LogInformation("Request date and time prepared for PersonId: {PersonId}", personId);

				var requestDate = DateOnly.FromDateTime(reqDateTime);
				var requestTime = TimeOnly.FromDateTime(reqDateTime);
				var mealtokenRequest = new MealTokenRequest
				{
					PersonId = personId,
					RequestDate = requestDate,
					RequestTime = requestTime,
					FunctionKey = null
				};

				_logger.LogInformation("Fetching meal distribution for PersonId: {PersonId}", personId);
				var schedule = await _tokenProcessService.GetMealDistributionAsync(mealtokenRequest);

				if (!schedule.Success)
				{
					_logger.LogWarning("Meal distribution retrieval failed for PersonId: {PersonId}. Message: {Message}", personId, schedule.Message);
					return new ServiceResult { Success = false, Message = schedule.Message };
				}

				_logger.LogInformation("Fetching existing meal consumption for PersonId: {PersonId}, MealTypeId: {MealTypeId}, Date: {Date}",
					personId, schedule.MealInfo.MealTypeId, requestDate);


				var exConsumption = await _businessData.GetMealConsumptionAsync(
					schedule.MealInfo.MealTypeId,
					person.PersonId,
					requestDate
				);

				if (exConsumption == null)
				{
					_logger.LogWarning("No existing meal consumption found for PersonId: {PersonId}, MealTypeId: {MealTypeId}, Date: {Date}",
						personId, schedule.MealInfo.MealTypeId, requestDate);

					return new ServiceResult
					{
						Success = false,
						Message = "No existing meal consumption found for this person within this meal type."
					};
				}
				var scheduleMeals = schedule.MealInfo;

				var mealTypeDisplay = scheduleMeals.MealSubTypeId.HasValue
					? scheduleMeals.MealSubTypeName
					: scheduleMeals.MealTypeName;

				var addonMealsText = scheduleMeals.MealAddOns.Any() == true
					? " With " + string.Join(", ", scheduleMeals.MealAddOns.Select(a => a.AddOnName))
					: string.Empty;

				exConsumption.TockenIssued = false;
				

				await _businessData.UpdateMealConsumptionAsync(exConsumption);
				var tokenResponse = new TokenResponse
				{
					Date = requestDate,
					Time = requestTime,
					MealType = mealTypeDisplay + addonMealsText,
					Shift = exConsumption.ShiftName.ToString(),
					EmpNo = person.PersonNumber,
					EmpName = person.Name,
					Gender = person.Gender,
					Department = await _adminData.GetDepartmentByIdAsync(person.DepartmentId),
					TokenType = exConsumption.PayStatus.ToString(),
					Contribution = exConsumption.EmployeeCost,
					MealConsumptionId = exConsumption.MealConsumptionId
				};
				var manualToken = new ManualTokenPrinted
				{
					TenantId = person.TenantId,
					PersonId = person.PersonId,
					PrintedDate = DateTime.UtcNow,
					MealConsumptionId = exConsumption.MealConsumptionId,
					Reason = "Lost Token",
					TokenIssued = false,
				};

				await _businessData.CreateManulTokenPrintAsync(manualToken);
				_logger.LogInformation("Meal token retrieved successfully for PersonId: {PersonId}", personId);

				return new ServiceResult
				{
					Success = true,
					Message = "Meal token retrieved successfully.",
					Data = tokenResponse
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred in ManualPrintTokenLost for PersonId: {PersonId}. Message: {Message}", personId, ex.Message);

				return new ServiceResult
				{
					Success = false,
					Message = "An unexpected error occurred while processing the token retrieval."
				};
			}
		}
		public async Task<ServiceResult> ManualPrintTokenOtherAsync(ManualPrintRequest printRequest)
		{
			_logger.LogInformation("ManualPrintTokenOther process started for PersonId: {PersonId}", printRequest.PersonId);

			try
			{
				// Validate Person
				var person = await _adminData.GetPersonByIdAsync(printRequest.PersonId);
				if (person == null)
				{
					_logger.LogWarning("ManualPrintTokenOther failed - Person not found. PersonId: {PersonId}", printRequest.PersonId);
					return new ServiceResult { Success = false, Message = "Person not found." };
				}

				// Validate Supplier
				var supplier = await _adminData.GetSupplierByIdAsync(printRequest.SupplierId);
				if (supplier == null)
				{
					_logger.LogWarning("ManualPrintTokenOther failed - Supplier not found. SupplierId: {SupplierId}", printRequest.SupplierId);
					return new ServiceResult { Success = false, Message = "Supplier not found." };
				}

				// Validate Meal Type
				var mealType = await _adminData.GetMealTypeByIdAsync(printRequest.MealTypeId);
				if (mealType == null)
				{
					_logger.LogWarning("ManualPrintTokenOther failed - MealType not found. MealTypeId: {MealTypeId}", printRequest.MealTypeId);
					return new ServiceResult { Success = false, Message = "Meal type not found." };
				}

				// Validate SubType if provided
				MealSubType subType = null;
				if (printRequest.MealSubTypeId.HasValue)
				{
					subType = await _adminData.GetMealSubTypesByIdAsync(printRequest.MealSubTypeId.Value);
					if (subType == null)
					{
						_logger.LogWarning("ManualPrintTokenOther failed - MealSubType not found. MealSubTypeId: {MealSubTypeId}", printRequest.MealSubTypeId);
						return new ServiceResult { Success = false, Message = "Meal subtype not found." };
					}
				}

				// Prepare request date/time once
				var requestDate = DateOnly.FromDateTime(printRequest.RequestDate);
				var requestTime = TimeOnly.FromDateTime(printRequest.RequestDate);

				// Get related data
				var mealCost = await _adminData.GetMealCostByDetailAsync(printRequest.SupplierId, printRequest.MealTypeId, printRequest.MealSubTypeId);
				if (mealCost == null)
				{
					_logger.LogWarning("ManualPrintTokenOther failed - Meal cost details not found for SupplierId: {SupplierId}, MealTypeId: {MealTypeId}",
						printRequest.SupplierId, printRequest.MealTypeId);
					return new ServiceResult { Success = false, Message = "Meal cost details not found." };
				}

				var payStatus = await DeterminPayStatusAsync(person, printRequest.Shift, printRequest.MealTypeId);
				var schedule = await _businessData.GetScheduleByNameAsync("Default");

				// Calculate contribution
				bool isFreeMeal = payStatus == PayStatus.Free;
				decimal empContribution = isFreeMeal ? 0 : mealCost.EmployeeCost;
				decimal companyContribution = isFreeMeal ? (mealCost.SupplierCost) : mealCost.CompanyCost;

				// Build meal consumption record
				var mealConsumption = new MealConsumption
				{
					TenantId = person.TenantId,
					PersonId = person.PersonId,
					PersonName = person.Name,
					Gender = person.Gender,
					Date = requestDate,
					Time = requestTime,
					ScheduleId = schedule.SheduleId,
					ScheduleName = schedule.SheduleName,
					AddOnMeal = false,
					MealTypeId = mealType.MealTypeId,
					MealTypeName = mealType.TypeName,
					SubTypeId = subType?.MealSubTypeId,
					SubTypeName = subType?.SubTypeName,
					MealCostId = mealCost.MealCostId,
					SupplierCost = mealCost.SupplierCost,
					SellingPrice = mealCost.SellingPrice,
					CompanyCost = companyContribution,
					EmployeeCost = empContribution,
					DeviceId = null,
					DeviceSerialNo = string.Empty,
					ShiftName = printRequest.Shift,
					PayStatus = payStatus,
					TockenIssued = false
				};

				await _businessData.CreateMealConsumptionAsync(mealConsumption);

				var manualToken = new ManualTokenPrinted
				{
					TenantId = person.TenantId,
					PersonId = person.PersonId,
					PrintedDate = DateTime.UtcNow,
					MealConsumptionId = mealConsumption.MealConsumptionId,
					Reason = printRequest.Reason,
					TokenIssued = false,
				};

				await _businessData.CreateManulTokenPrintAsync(manualToken);
				// Build response
				var department = await _adminData.GetDepartmentByIdAsync(person.DepartmentId);
				var mealTypeDisplay = mealConsumption.SubTypeId.HasValue
					? mealConsumption.SubTypeName
					: mealConsumption.MealTypeName;

				var tokenResponse = new TokenResponse
				{
					Date = requestDate,
					Time = requestTime,
					MealType = mealTypeDisplay,
					Shift = printRequest.Shift.ToString(),
					EmpNo = person.PersonNumber,
					EmpName = person.Name,
					Gender = person.Gender,
					Department = department,
					TokenType = payStatus.ToString(),
					Contribution = empContribution,
					MealConsumptionId = mealConsumption.MealConsumptionId
				};

				_logger.LogInformation("ManualPrintTokenOther succeeded for PersonId: {PersonId}, MealConsumptionId: {MealConsumptionId}",
					printRequest.PersonId, mealConsumption.MealConsumptionId);

				return new ServiceResult
				{
					Success = true,
					Message = "Meal token retrieved successfully.",
					Data = tokenResponse
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred in ManualPrintTokenOther for PersonId: {PersonId}. Message: {Message}",
					printRequest.PersonId, ex.Message);

				return new ServiceResult
				{
					Success = false,
					Message = "An unexpected error occurred while processing the meal token."
				};
			}
			finally
			{
				_logger.LogInformation("ManualPrintTokenOther process completed for PersonId: {PersonId}", printRequest.PersonId);
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
            // Day hours: 7 AM - 7 PM
            bool isInDayHours = currentTime >= dayStart && currentTime < dayEnd;

            // Extended day hours: 7 PM - 11:59 PM OR 12 AM - 6 AM
            bool isInExtendedDayHours = currentTime >= dayEnd || currentTime < extendedDayEnd;

            // Night hours: 6 AM - 7 AM (only 1 hour window before day starts)
            bool isInNightHours = currentTime >= extendedDayEnd && currentTime < dayStart;

			var breakfast = new TimeOnly(9, 25);
			// Check meal history
			bool hadDayShiftMeal = todaysMeals?.Any(m =>
                m.Time >= dayStart &&
                m.Time < dayEnd &&
                (m.ShiftName == Shift.DayShift ||
                 m.ShiftName == Shift.DayShiftExtended)
            ) ?? false;

            bool hadExtendedDayMeal = todaysMeals?.Any(m =>
                (m.Time >= dayEnd || m.Time < extendedDayEnd) &&
                m.ShiftName == Shift.DayShiftExtended
            ) ?? false;

            bool hadNightShiftMeal = todaysMeals?.Any(m =>
                (m.Time >= dayEnd || m.Time < dayStart) &&
                (m.ShiftName == Shift.NightShift ||
                 m.ShiftName == Shift.NightandDayShift)
            ) ?? false;

            bool hadEarlyMorningNightMeal = todaysMeals?.Any(m =>
                m.Time >= extendedDayEnd &&
                m.Time < dayStart &&
                (m.ShiftName == Shift.NightShift ||
                 m.ShiftName == Shift.NightandDayShift)
            ) ?? false;

            if (isInDayHours)
            {
                if (deviceShift == DeviceShift.Day)
                {
                    // Check if continuing from night shift
                    if (hadEarlyMorningNightMeal ||
                        hadNightShiftMeal ||
                        lastMealIn13Hours?.ShiftName == Shift.NightShift ||
                        lastMealIn13Hours?.ShiftName == Shift.NightandDayShift)
                    {
                        // Night worker continuing into day
                        detectedShift = Shift.NightandDayShift;
                        return new ServiceResult { Success = true };
                    }

                    // Normal day shift
                    detectedShift = Shift.DayShift;
                    return new ServiceResult { Success = true };
                }
                else if (deviceShift == DeviceShift.Night)
                {

					if (currentTime < breakfast)
					{
						detectedShift = Shift.NightShift;
						return new ServiceResult { Success = true };
					}
					else if (hadEarlyMorningNightMeal ||
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
                    if (hadDayShiftMeal || hadExtendedDayMeal)
                    {
                        // Day worker working extended hours (until 6 AM next day)
                        detectedShift = Shift.DayShiftExtended;
                        return new ServiceResult { Success = true };
                    }

                    // Check if last meal was during day shift
                    if (lastMealIn13Hours != null &&
                        lastMealIn13Hours.Time >= dayStart &&
                        lastMealIn13Hours.Time < dayEnd &&
                        (lastMealIn13Hours.ShiftName == Shift.DayShift ||
                         lastMealIn13Hours.ShiftName == Shift.DayShiftExtended))
                    {
                        detectedShift = Shift.DayShiftExtended;
                        return new ServiceResult { Success = true };
                    }

                    // No day shift history but using day device in extended hours
                    // Could be starting extended shift without earlier meals
                    // Allow it but default to extended day
                    detectedShift = Shift.DayShiftExtended;
                    return new ServiceResult { Success = true };
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
                     hadExtendedDayMeal ||
                     lastMealIn13Hours?.ShiftName == Shift.DayShift ||
                     lastMealIn13Hours?.ShiftName == Shift.DayShiftExtended)
                    {
                        // This shouldn't happen often - extended day ends at 6 AM
                        // But if someone has day meal history, they're on extended day
                        detectedShift = Shift.DayShiftExtended;
                        return new ServiceResult { Success = true };
                    }

                    // Normal night shift
                    detectedShift = Shift.NightShift;
                    return new ServiceResult { Success = true };
                }
                else if (deviceShift == DeviceShift.Day)
                {

                    // Check if continuing from day shift (legitimate use)
                    if (hadDayShiftMeal ||
                        lastMealIn13Hours?.ShiftName == Shift.DayShift ||
                        lastMealIn13Hours?.ShiftName == Shift.DayShiftExtended)
                    {
                        detectedShift = Shift.DayShiftExtended;
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

		private async Task<PayStatus> DeterminPayStatusAsync(Person person, Shift shift, int mealTypeId)
		{
			// Early exit for non-employers
			if (person.PersonType != PersonType.Employer)
			{
				return PayStatus.Free;
			}

			var policy = await _businessData.GetPayPolicyAsync(shift, mealTypeId);

			// Early exit if no policy or both genders are free
			if (policy == null || (!policy.IsMalePaid && !policy.IsFemalePaid))
			{
				if (policy == null)
				{
					_logger.LogWarning("No payment policy found for Shift: {Shift}, MealType: {MealTypeId}",
						shift, mealTypeId);
				}
				return PayStatus.Free;
			}

			// Determine pay status based on gender
			return person.Gender switch
			{
				"Male" => policy.IsMalePaid ? PayStatus.Paid : PayStatus.Free,
				"Female" => policy.IsFemalePaid ? PayStatus.Paid : PayStatus.Free,
				_ => PayStatus.Paid // Unknown gender defaults to paid (safer)
			};
		}

		private async Task<List<MealConsumption>> CreateAddOnMealConsumptionsAsync(
            List<MealAddOnDto> addOns,
            MealConsumption mainMeal,
            Person person,
            Shift shift,
            MealTokenRequest request)
        {
            var addonConsumptions = new List<MealConsumption>();

            foreach (var addOn in addOns)
            {
                // Get addon meal cost
                var addonMealCost = await _adminData.GetMealCostByDetailAsync(
                    addOn.SupplierId,
                    addOn.AddOnMealTypeId,
                    addOn.AddOnSubTypeId
                );

                if (addonMealCost == null)
                {
                    _logger.LogWarning("Meal cost not found for AddOnId: {AddOnId}, SkippingAddon", addOn.AddOnSubTypeId);
                    continue;
                }

                // Addon meals are always free for employees
                var addonConsumption = new MealConsumption
                {
                    TenantId = person.TenantId,
                    PersonId = person.PersonId,
                    PersonName = person.Name,
                    Gender = person.Gender,
                    Date = request.RequestDate,
                    Time = request.RequestTime,
                    ScheduleId = mainMeal.ScheduleId,
                    ScheduleName = mainMeal.ScheduleName,
                    AddOnMeal = true,
                    MealTypeId = addOn.AddOnMealTypeId,
                    MealTypeName = addOn.AddonType.ToString(),
                    SubTypeId = addOn.AddOnSubTypeId,
                    SubTypeName = addOn.AddOnName,
                    MealCostId = addonMealCost.MealCostId,
                    SupplierCost = addonMealCost.SupplierCost,
                    SellingPrice = addonMealCost.SellingPrice,
                    CompanyCost = addonMealCost.SupplierCost, // Company pays full cost for addons
                    EmployeeCost = 0, // Employee pays nothing for addon meals
                    DeviceId = mainMeal.DeviceId,
                    DeviceSerialNo = mainMeal.DeviceSerialNo,
                    ShiftName = shift,
                    PayStatus = PayStatus.Free, // Addon meals are always free
                    TockenIssued = false
                };

                addonConsumptions.Add(addonConsumption);
            }

            return addonConsumptions;
        }

    }
}
