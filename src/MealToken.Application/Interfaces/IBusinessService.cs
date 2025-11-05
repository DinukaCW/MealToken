using Authentication.Models.DTOs;
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
	public interface IBusinessService
	{
		Task<List<DeviceDto>> GetClientDeviceDetails(int clientId);
		Task<ServiceResult> CreateScheduleAsync(SheduleDTO scheduleDto);
		Task<ServiceResult> UpdateScheduleAsync(int scheduleId, SheduleDTO updateDto);
		Task<ServiceResult> DeleteScheduleAsync(int scheduleId);
		Task<ServiceResult> GetScheduleListAsync();
		Task<ServiceResult> GetScheduleByIdAsync(int scheduleId);
		Task<ServiceResult> GetScheduleCreationDetailsAsync();
		Task<ServiceResult> CreateMealRequestAsync(RequestDto requestDto);
		Task<ServiceResult> UpdateMealRequestAsync(int mealRequestId, RequestDto requestDto);
		Task<ServiceResult> GetRequestDetailsAsync(int requestId);
		Task<ServiceResult> GetPendingRequestListAsync();
		Task<ServiceResult> GetApprovedRequestListAsync();
		Task<ServiceResult> GetRequestListByIdAsync();
		Task<ServiceResult> UpdateRequestStatusAsync(int requestId, UserRequestStatus newStatus, int approverId, string? rejectReason);
		Task<ServiceResult> ProcessLogicAsync(int tenantId, MealDeviceRequest request);
		Task<ServiceResult> UpdateMealConsumption(int mealConsumptionId, bool status, string jobStatus);

    }
}
