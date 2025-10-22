using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Models;
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
        public async Task<List<MealConsumptionWithDetails>> GetMealConsumptioninWeekAsync(DateOnly startDate, DateOnly endDate)
        {
            // Step 1: Query the first context (_tenantContext) and bring the initial data into memory.
            // We select into an anonymous type to get just what we need.
            var consumptionsFromTenantDb = await (
                from mc in _tenantContext.MealConsumption
                join p in _tenantContext.Person on mc.PersonId equals p.PersonId
				where mc.Date >= startDate
			          && mc.Date <= endDate
			          && mc.TockenIssued
				select new
                {
                    Consumption = mc, // Keep the original consumption object
                    p.DepartmentId,
                    p.DesignationId
                })
                .ToListAsync();

            // If there's no data, return an empty list early.
            if (!consumptionsFromTenantDb.Any())
            {
                return new List<MealConsumptionWithDetails>();
            }

            // Step 2: Get the unique IDs needed for the second context.
            var departmentIds = consumptionsFromTenantDb.Select(c => c.DepartmentId).Distinct().ToList();
            var designationIds = consumptionsFromTenantDb.Select(c => c.DesignationId).Distinct().ToList();

            // Step 3: Query the second context (_platformContext) for departments and designations.
            // Fetching into dictionaries makes the lookup in the next step very fast.
            var departments = await _platformContext.Department
                .Where(d => departmentIds.Contains(d.DepartmnetId))
                .ToDictionaryAsync(d => d.DepartmnetId, d => d.Name);

            var designations = await _platformContext.Designation
                .Where(des => designationIds.Contains(des.DesignationId))
                .ToDictionaryAsync(des => des.DesignationId, des => des.Title);

            // Step 4: Join the data in memory to build the final list.
            var result = consumptionsFromTenantDb.Select(c => new MealConsumptionWithDetails
            {
                MealConsumptionId = c.Consumption.MealConsumptionId,
                Date = c.Consumption.Date,
                PersonId = c.Consumption.PersonId,
                PersonName = c.Consumption.PersonName,
                Gender = c.Consumption.Gender,
                // Look up the name from the dictionaries. Provide a default value if not found.
                DepartmentName = departments.TryGetValue(c.DepartmentId, out var deptName) ? deptName : "N/A",
                DesignationName = designations.TryGetValue(c.DesignationId??11, out var desigTitle) ? desigTitle : "N/A",
                MealTypeName = c.Consumption.MealTypeName,
                SubTypeName = c.Consumption.SubTypeName,
                EmployeeCost = c.Consumption.EmployeeCost,
                CompanyCost = c.Consumption.CompanyCost,
                SupplierCost = c.Consumption.SupplierCost
            }).ToList();

            // Apply sorting to the final in-memory list
            return result
                .OrderBy(m => m.Date)
                .ThenBy(m => m.MealTypeName)
                .ThenBy(m => m.SubTypeName)
                .ThenBy(m => m.PersonId)
                .ToList();
        }
        public async Task<int> GetMealsServedThisMonthAsync()
        {
            var today = DateTime.Now;
            var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            return await _tenantContext.MealConsumption
				.Where(m => m.Date >= firstDayOfMonth
				             && m.Date <= lastDayOfMonth
				             && m.TockenIssued)
				.CountAsync();
        }

        public async Task<int> GetMealsServedLastMonthAsync()
        {
            var today = DateTime.Now;
            var firstDayOfLastMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
            var lastDayOfLastMonth = new DateOnly(today.Year, today.Month, 1).AddDays(-1);

            return await _tenantContext.MealConsumption
                .Where(m => m.Date >= firstDayOfLastMonth
                         && m.Date <= lastDayOfLastMonth
						 && m.TockenIssued)
                .CountAsync();
        }

        public async Task<int> GetActiveEmployeesCountAsync()
        {
            return await _tenantContext.Person
                .Where(e => e.PersonType == PersonType.Employer && e.IsActive == true)
                .CountAsync();
        }

        public async Task<int> GetActiveVisitorsCountAsync()
        {
           
            return await _tenantContext.Person
                .Where(v => v.PersonType == PersonType.Visitor && v.IsActive == true)
                .CountAsync();
        }

        public async Task<int> GetPendingRequestsCountAsync()
        {
            return await _tenantContext.Request
                .Where(r => r.Status == UserRequestStatus.Pending)
                .CountAsync();
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
    }
}
