using Authentication.Models.Entities;
using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Models;
using MealToken.Domain.Models.Reports;
using MealToken.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MealToken.Infrastructure.Repositories.BusinessRepository;

namespace MealToken.Infrastructure.Repositories
{
	public class BusinessRepository : IBusinessRepository
	{
		private readonly MealTokenDbContext _tenantContext;
		private readonly PlatformDbContext _platformContext;
		private readonly ITenantContext _currentTenant;
		private readonly ILogger<AdminRepository> _logger;

		public BusinessRepository(
			MealTokenDbContext tenantContext,
			PlatformDbContext platformContext,
			ITenantContext currentTenant,
			ILogger<AdminRepository> logger)
		{
			_tenantContext = tenantContext;
			_platformContext = platformContext;
			_currentTenant = currentTenant;
			_logger = logger;
		}

		public async Task<List<ClientDevice>> GetClientDevicesAsync(int clientId)
		{
			return await _tenantContext.ClientDevice
				.Where(s => s.IsActive)
				.Where(s => s.TenantId == clientId)
				.ToListAsync();
		
		}
		public async Task CreateSheduleAsync(Schedule shedule)
		{
			await _tenantContext.Schedule.AddAsync(shedule);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task UpdateSheduleAsync(Schedule shedule)
		{
			_tenantContext.Schedule.Update(shedule);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task CreateSheduleMealAsync(IEnumerable<ScheduleMeal> sheduleMeals)
		{
			await _tenantContext.ScheduleMeal.AddRangeAsync(sheduleMeals);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task UpdateSheduleMealAsync(ScheduleMeal sheduleMeal)
		{
			_tenantContext.ScheduleMeal.Update(sheduleMeal);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task CreateShedulePersonAsync(IEnumerable<SchedulePerson> shedulePersons)
		{
			await _tenantContext.SchedulePerson.AddRangeAsync(shedulePersons);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task UpdateShedulePersonAsync(SchedulePerson shedulePerson)
		{
			_tenantContext.SchedulePerson.Update(shedulePerson);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task CreateSheduleDateAsync(IEnumerable<ScheduleDate> sheduleDate)
		{
			await _tenantContext.ScheduleDate.AddRangeAsync(sheduleDate);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task UpdateSheduleDateAsync(ScheduleDate sheduleDate)
		{
			_tenantContext.ScheduleDate.Update(sheduleDate);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task<Schedule> GetSheduleByNameAsync(string sheduleName)
		{
			return await _tenantContext.Schedule
				.FirstOrDefaultAsync(e => e.SheduleName == sheduleName);
		}
		public async Task<List<Schedule>> GetPersonSchedulesForDateAsync(int personId, DateOnly date)
		{
			return await _tenantContext.Schedule
				.Where(s => s.IsActive)
				.Where(s => s.MealShedulePeople.Any(sp => sp.PersonId == personId))
				.Where(s => s.SheduleDates.Any(sd => sd.Date == date))
				.ToListAsync();
		}

		public async Task<List<ScheduleMeal>> GetScheduleMealsAsync(int scheduleId)
		{
			return await _tenantContext.ScheduleMeal
				.Where(sm => sm.SheduleId == scheduleId && sm.IsAvailable)
				.ToListAsync();
		}
        public async Task<Schedule> GetScheduleByIdAsync(int scheduleId)
        {
            return await _tenantContext.Schedule
                .FirstOrDefaultAsync(s => s.IsActive && s.SheduleId == scheduleId);
        }

        public async Task<List<ScheduleDate>> GetScheduleDatesAsync(int scheduleId)
		{
			return await _tenantContext.ScheduleDate
				.Where(e => e.SheduleId == scheduleId)
				.ToListAsync();
		}
		public async Task SaveChangesAsync()
		{
			
			await _tenantContext.SaveChangesAsync();
		}
		public async Task UpdateScheduleAsync(Schedule schedule)
		{
			_tenantContext.Schedule.Update(schedule);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task DeleteScheduleDatesAsync(int scheduleId)
		{
			var scheduleDates = await _tenantContext.ScheduleDate
				.Where(sd => sd.SheduleId == scheduleId)
				.ToListAsync();

			if (scheduleDates.Any())
			{
				_tenantContext.ScheduleDate.RemoveRange(scheduleDates);
				await _tenantContext.SaveChangesAsync();
			}
		}
		public async Task DeleteScheduleMealsAsync(int scheduleId)
		{
			var scheduleMeals = await _tenantContext.ScheduleMeal
				.Where(sd => sd.SheduleId == scheduleId)
				.ToListAsync();

			if (scheduleMeals.Any())
			{
				_tenantContext.ScheduleMeal.RemoveRange(scheduleMeals);
				await _tenantContext.SaveChangesAsync();
			}
		}
		public async Task DeleteSchedulePersonsAsync(int scheduleId)
		{
			var schedulePerson = await _tenantContext.SchedulePerson
				.Where(sd => sd.SheduleId == scheduleId)
				.ToListAsync();

			if (schedulePerson.Any())
			{
				_tenantContext.SchedulePerson.RemoveRange(schedulePerson);
				await _tenantContext.SaveChangesAsync();
			}
		}
		public async Task DeleteScheduleAsync(int scheduleId)
		{
			var schedule = await _tenantContext.Schedule.FindAsync(scheduleId);
			if (schedule != null)
			{
				_tenantContext.Schedule.Remove(schedule);
				await _tenantContext.SaveChangesAsync();
			}
		}
		public async Task<List<Schedule>> GetSchedulesAsync()
		{
			return await _tenantContext.Schedule
				.Where(s => s.IsActive)
				.ToListAsync();
		}
		public async Task<List<SchedulePerson>> GetSchedulePeopleAsync(int scheduleId)
		{
			return await _tenantContext.SchedulePerson
				.Where(sm => sm.SheduleId == scheduleId)
				.ToListAsync();
		}
        public async Task<Request> GetRequestByDetailsAsync(DateOnly date, string eventType, string description)
        {
            return await _tenantContext.Request
                .FirstOrDefaultAsync(e =>
                   e.EventDate == date &&
				   e.EventType == eventType &&
				   e.Description == description);
        }
        public async Task CreateRequestAsync(Request request)
        {
            await _tenantContext.Request.AddAsync(request);
            await _tenantContext.SaveChangesAsync();
        }
        public async Task UpdateRequestAsync(Request request)
        {
            _tenantContext.Request.Update(request);
            await _tenantContext.SaveChangesAsync();
        }
        public async Task CreateRequestMealAsync(IEnumerable<RequestMeal> requestMeals)
        {
            await _tenantContext.RequestMeal.AddRangeAsync(requestMeals);
            await _tenantContext.SaveChangesAsync();
        }
        public async Task UpdateRequestMealAsync(RequestMeal requestMeal)
        {
            _tenantContext.RequestMeal.Update(requestMeal);
            await _tenantContext.SaveChangesAsync();
        }
        public async Task<Request> GetRequestByIdAsync(int requestId)
        {
            return await _tenantContext.Request.FindAsync(requestId);
        }
        public async Task DeleteRequestMealsAsync(int requestId)
        {
            var requestMeals = await _tenantContext.RequestMeal
                .Where(sd => sd.RequestMealId == requestId)
                .ToListAsync();

            if (requestMeals.Any())
            {
                _tenantContext.RequestMeal.RemoveRange(requestMeals);
                await _tenantContext.SaveChangesAsync();
            }
        }
        public async Task<List<RequestMeal>> GetRequestMealsAsync(int requestId)
        {
            return await _tenantContext.RequestMeal
                .Where(sm => sm.RequestId == requestId)
                .ToListAsync();
        }
        public async Task<List<Request>> GetRequesListAsync()
        {
            return await _tenantContext.Request
                .ToListAsync();
        }
        public async Task<List<Request>> GetPendingRequestListAsync()
        {
            return await _tenantContext.Request
				.Where(r => r.Status == UserRequestStatus.Pending)
                .ToListAsync();
        }
        public async Task<List<Request>> GetApprovedRequestListAsync()
        {
            return await _tenantContext.Request
                .Where(r => r.Status == UserRequestStatus.Approved)
                .ToListAsync();
        }
        public async Task<List<Request>> GetRequesListByIdAsync(int requesterId)
        {
            return await _tenantContext.Request
				.Where(r => r.RequesterId == requesterId)
                .ToListAsync();
        }
        public async Task<List<SchedulePerson>> GetListofShedulesforPersonAsync(int personId)
        {
            return await _tenantContext.SchedulePerson
                .Where(sp => sp.PersonId == personId)
                .ToListAsync();
        }
        public async Task<List<ScheduleDate>> GetListofShedulesforDateAsync(DateOnly date)
        {
            return await _tenantContext.ScheduleDate
               .Where(sp => sp.Date == date)
               .ToListAsync();
        }
        public async Task<List<ScheduleMeal>> GetListofShedulesforTimeAsync(TimeOnly time)
        {
            return await _tenantContext.ScheduleMeal
                .Where(s => s.TokenIssueStartTime.HasValue
                            && s.TokenIssueEndTime.HasValue
                            && time >= s.TokenIssueStartTime.Value
                            && time <= s.TokenIssueEndTime.Value)
                .ToListAsync();
        }
        public async Task<int> GetDeviceBySerialNoAsync(string serialNo)
        {
			return await _tenantContext.ClientDevice
				.Where(s =>s.SerialNo == serialNo)
				.Select(s => s.ClientDeviceId)
				.FirstOrDefaultAsync();
        }
        public async Task<DeviceShift?> GetDeviceShiftBySerialNoAsync(string serialNo)
        {
            return await _tenantContext.ClientDevice
                .Where(s => s.SerialNo == serialNo)
                .Select(s => s.DeviceShift)
                .FirstOrDefaultAsync();
        }
        public async Task<MealConsumption?> GetMealConsumptionAsync(int mealTypeId, int personId, DateOnly date)
        {

            return await _tenantContext.MealConsumption
                .Where(s => s.MealTypeId == mealTypeId && s.PersonId == personId)
                .Where(s => s.Date == date && s.TockenIssued)
                .FirstOrDefaultAsync();
        }



        public async Task<MealConsumption?> GetMealConsumptionInLast13HoursAsync(int personId)
        {
          
            var thirteenHoursAgo = DateTimeOffset.Now.AddHours(-13);
            var thresholdDate = DateOnly.FromDateTime(thirteenHoursAgo.DateTime);
            var thresholdTime = TimeOnly.FromDateTime(thirteenHoursAgo.DateTime);

			return await _tenantContext.MealConsumption
				.Where(s => s.PersonId == personId)
				.Where(s => s.Date > thresholdDate || (s.Date == thresholdDate && s.Time >= thresholdTime))
                .OrderByDescending(s => s.Date)
                .ThenByDescending(s => s.Time)
                .FirstOrDefaultAsync();
        }
        public async Task<List<MealConsumption>> GetMealConsumptionByDateAsync( int personId, DateOnly date)
        {
            return await _tenantContext.MealConsumption
                .Where(m => m.PersonId == personId && m.Date == date)
                .OrderBy(m => m.Time)
                .ToListAsync();
        }
        public async Task CreateMealConsumptionAsync(MealConsumption mealConsumption)
        {
            await _tenantContext.MealConsumption.AddAsync(mealConsumption);
            await _tenantContext.SaveChangesAsync();
        }
        public async Task UpdateMealConsumptionAsync(MealConsumption mealConsumption)
        {
            _tenantContext.MealConsumption.Update(mealConsumption);
            await _tenantContext.SaveChangesAsync();
        }
        public async Task<PayStatusByShift> GetPayPolicyAsync(Shift shift,int mealtypeId)
        {
			return await _tenantContext.PayStatusByShiftPolicy
				.Where(p => p.ShiftType == shift && p.MealTypeId == mealtypeId)
				.FirstOrDefaultAsync();
        }
        public async Task<MealConsumption> GetMealConsumptionByIdAsync(int consumptionId)
        {
            return await _tenantContext.MealConsumption
                .Where(p => p.MealConsumptionId == consumptionId)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteSchedulePersonsByIdsAsync(List<int> assignmentIds)
        {
            if (assignmentIds == null || !assignmentIds.Any())
                throw new ArgumentException("No assignment IDs provided", nameof(assignmentIds));

            var schedulePersons = await _tenantContext.SchedulePerson
                .Where(p => assignmentIds.Contains(p.ShedulePersonId))
                .ToListAsync();

            if (schedulePersons.Any())
            {
                _tenantContext.SchedulePerson.RemoveRange(schedulePersons);
                await _tenantContext.SaveChangesAsync();
            }
        }
        public async Task DeleteScheduleDatesByIdsAsync(List<int> dateIds)
        {
            if (dateIds == null || !dateIds.Any())
                throw new ArgumentException("No date IDs provided", nameof(dateIds));

            var scheduleDates = await _tenantContext.ScheduleDate
                .Where(d => dateIds.Contains(d.SheduleDateId))
                .ToListAsync();

            if (scheduleDates.Any())
            {
                _tenantContext.ScheduleDate.RemoveRange(scheduleDates);
                await _tenantContext.SaveChangesAsync();
            }
        }
       
        public async Task CreateMealConsumptionBatchAsync(IEnumerable<MealConsumption> mealConsumptions)
        {
            await _tenantContext.MealConsumption.AddRangeAsync(mealConsumptions);
            await _tenantContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<RequestMeal>> GetRequestMealsByRequestIdsAsync(IEnumerable<int> requestIds)
        {

                if (requestIds == null || !requestIds.Any())
                    return new List<RequestMeal>();

                var idList = requestIds.Distinct().ToList();
                return await _tenantContext.RequestMeal
                    .Where(rm => idList.Contains(rm.RequestId))
                    .AsNoTracking()
                    .ToListAsync();
  
        }
		public async Task<List<int>> GetScheduleByDateAsync(DateOnly date)
		{
			return await _tenantContext.ScheduleDate
				.Where(e => e.Date == date)
                .Select(e => e.SheduleId)
                .Distinct()
				.ToListAsync();
		}
		public async Task<List<TodayScheduleDto>> GetMealsByScheduleIdsAsync(IEnumerable<int> scheduleIds)
		{
			return await _tenantContext.ScheduleMeal
				.Where(sm => scheduleIds.Contains(sm.SheduleId) && sm.IsAvailable)
				.Join(_tenantContext.Schedule,
					sm => sm.SheduleId,
					s => s.SheduleId,
					(sm, s) => new { ScheduleMeal = sm, Schedule = s })
				.Join(_tenantContext.MealType,
					x => x.ScheduleMeal.MealTypeId,
					mt => mt.MealTypeId,
					(x, mt) => new { x.ScheduleMeal, x.Schedule, MealType = mt })
				.Select(x => new TodayScheduleDto
				{
					MealType = x.MealType.TypeName,
					StartTime = x.ScheduleMeal.TokenIssueStartTime.Value,
					EndTime = x.ScheduleMeal.TokenIssueEndTime.Value,
					ScheduleId = x.ScheduleMeal.SheduleId,
					ScheduleName = x.Schedule.SheduleName,
					
				})
				.ToListAsync();
		}
		public async Task<bool> CheckIfConsumptionExistsAsync(int requestId, int mealTypeId, int? subTypeId)
		{
			return await _tenantContext.RequestMealConsumption
				.AnyAsync(rmc => rmc.RequestId == requestId &&
								 rmc.MealTypeId == mealTypeId &&
								 rmc.SubTypeId == subTypeId);
		}
		public async Task CreateRequestMealConsumptionAsync(RequestMealConsumption consumption)
		{
			await _tenantContext.RequestMealConsumption.AddAsync(consumption);
			await _tenantContext.SaveChangesAsync();
			
		}
		public async Task CreateRequestMealConsumptionBulkAsync(List<RequestMealConsumption> consumptions)
		{
			await _tenantContext.RequestMealConsumption.AddRangeAsync(consumptions);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task<List<User>> GetUsersByDepartmentsAsync(List<int> departmentIds)
		{
			return await _tenantContext.UserDepartment
				.Where(ud => departmentIds.Contains(ud.DepartmentId)
							 && ud.RequestStatus == UserRequestStatus.Approved)
				.Join(
					_tenantContext.Users,
					ud => ud.UserId,
					u => u.UserID,
					(ud, u) => u // Select only the User object
				)
				.Where(u => u.UserRoleId == 1 || u.UserRoleId == 2)
				.Distinct()
				.ToListAsync();
		}
		public async Task CreateManulTokenPrintAsync(ManualTokenPrinted manualToken)
		{
			await _tenantContext.ManualTokenPrinted.AddAsync(manualToken);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task<Schedule> GetScheduleByNameAsync(string scheduleName)
		{
			return await _tenantContext.Schedule
				.Where(p => p.SheduleName == scheduleName)
				.FirstOrDefaultAsync();
		}
		public async Task<ManualTokenPrinted> GetManualTokenPrintedByConsumptionIdAsync(int consumptionId)
		{
			return await _tenantContext.ManualTokenPrinted
				.Where(p => p.ManualTokenPrintedId == consumptionId)
				.FirstOrDefaultAsync();
		}
		public async Task UpdateManualTokenPrintedAsync(ManualTokenPrinted manualToken)
		{
			_tenantContext.ManualTokenPrinted.Update(manualToken);
			await _tenantContext.SaveChangesAsync();
		}
	}
}
