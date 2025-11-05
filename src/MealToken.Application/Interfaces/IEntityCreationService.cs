using Authentication.Models.DTOs;
using MealToken.Domain.Entities;
using MealToken.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Interfaces
{
	public interface IEntityCreationService
	{
		Task<ServiceResult> SubmitUserRequestAsync(UserDetails userDetails);
		Task<List<UserRequestDto>> GetPendingUserRequestsAsync();
		Task<ServiceResult> ApproveUserRequestAsync(int reviewerId, ApproveRequestModel model);
        Task<ServiceResult> RejectUserRequestAsync(int requestId, int reviewerId, string rejectionReason);
		Task<UserRequest> GetUserRequestByIdAsync(int requestId);
		Task<List<UserListDto>> GetUsersListAsync();
		Task<UserListDto?> GetUserByIdAsync(int userId);
		Task<ServiceResult> UpdateUserAsync(int userId, UserUpdateDetails updateDto);
		Task<List<UserRoleDto>> GetUserRolesAsync();
		Task<List<DepartmentD>> GetListofDepartmentsAsync();

    }
}
