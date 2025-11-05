using Authentication.Models.DTOs;
using Authentication.Models.Entities;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Models;
using MealToken.Domain.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Interfaces
{
	public interface IBusinessRepository
	{
		Task<List<ClientDevice>> GetClientDevicesAsync(int clientId);
		Task CreateSheduleAsync(Schedule shedule);
		Task UpdateSheduleAsync(Schedule shedule);
		Task CreateSheduleMealAsync(IEnumerable<ScheduleMeal> sheduleMeals);
		Task UpdateSheduleMealAsync(ScheduleMeal sheduleMeal);
		Task CreateShedulePersonAsync(IEnumerable<SchedulePerson> shedulePersons);
		Task UpdateShedulePersonAsync(SchedulePerson shedulePerson);
		Task CreateSheduleDateAsync(IEnumerable<ScheduleDate> sheduleDate);
		Task UpdateSheduleDateAsync(ScheduleDate sheduleDate);
		Task<Schedule> GetSheduleByNameAsync(string sheduleName);
		Task<List<Schedule>> GetPersonSchedulesForDateAsync(int personId, DateOnly date);
		Task<List<ScheduleMeal>> GetScheduleMealsAsync(int scheduleId);
		Task<Schedule> GetScheduleByIdAsync(int scheduleId);
		Task<List<ScheduleDate>> GetScheduleDatesAsync(int scheduleId);
		Task SaveChangesAsync();
		Task UpdateScheduleAsync(Schedule schedule);
		Task DeleteScheduleDatesAsync(int scheduleId);
		Task DeleteScheduleMealsAsync(int scheduleId);
		Task DeleteSchedulePersonsAsync(int scheduleId);
		Task DeleteScheduleAsync(int scheduleId);
		Task<List<Schedule>> GetSchedulesAsync();
		Task<List<SchedulePerson>> GetSchedulePeopleAsync(int scheduleId);
		Task<Request> GetRequestByDetailsAsync(DateOnly date, string eventType, string description);
		Task CreateRequestAsync(Request request);
		Task UpdateRequestAsync(Request request);
		Task CreateRequestMealAsync(IEnumerable<RequestMeal> requestMeals);
		Task UpdateRequestMealAsync(RequestMeal requestMeal);
		Task<Request> GetRequestByIdAsync(int requestId);
		Task DeleteRequestMealsAsync(int requestId);
		Task<List<RequestMeal>> GetRequestMealsAsync(int requestId);
		Task<List<Request>> GetRequesListAsync();
		Task<List<Request>> GetPendingRequestListAsync();
		Task<List<Request>> GetApprovedRequestListAsync();
		Task<List<Request>> GetRequesListByIdAsync(int requesterId);
		Task<List<SchedulePerson>> GetListofShedulesforPersonAsync(int personId);
		Task<List<ScheduleDate>> GetListofShedulesforDateAsync(DateOnly date);
		Task<List<ScheduleMeal>> GetListofShedulesforTimeAsync(TimeOnly time);
        Task<int> GetDeviceBySerialNoAsync(string serialNo);
		Task<MealConsumption?> GetMealConsumptionAsync(int mealTypeId, int personId, DateOnly date);
        Task<MealConsumption> GetMealConsumptionInLast13HoursAsync(int personId);
		Task<List<MealConsumption>> GetMealConsumptionByDateAsync(int personId, DateOnly date);
		Task CreateMealConsumptionAsync(MealConsumption mealConsumption);
		Task UpdateMealConsumptionAsync(MealConsumption mealConsumption);
		Task<PayStatusByShift> GetPayPolicyAsync(Shift shift, int mealtypeId);
		Task<MealConsumption> GetMealConsumptionByIdAsync(int consumptionId);
		Task<DeviceShift?> GetDeviceShiftBySerialNoAsync(string serialNo);
		Task DeleteSchedulePersonsByIdsAsync(List<int> assignmentIds);
		Task DeleteScheduleDatesByIdsAsync(List<int> dateIds);
		Task CreateMealConsumptionBatchAsync(IEnumerable<MealConsumption> mealConsumptions);
		Task<IEnumerable<RequestMeal>> GetRequestMealsByRequestIdsAsync(IEnumerable<int> requestIds);
		Task<List<int>> GetScheduleByDateAsync(DateOnly date);
		Task<List<TodayScheduleDto>> GetMealsByScheduleIdsAsync(IEnumerable<int> scheduleIds);
		Task<bool> CheckIfConsumptionExistsAsync(int requestId, int mealTypeId, int? subTypeId);
		Task CreateRequestMealConsumptionAsync(RequestMealConsumption consumption);
		Task CreateRequestMealConsumptionBulkAsync(List<RequestMealConsumption> consumptions);
		Task<List<User>> GetUsersByDepartmentsAsync(List<int> departmentIds);

	}
}
