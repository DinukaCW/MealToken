using Authentication.Models.DTOs;
using MealToken.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Interfaces
{
	public interface IAdminService
	{
		Task<ServiceResult> AddPersonAsync(PersonCreateDto personDto);
		Task<ServiceResult> UpdatePersonAsync(int personId, PersonCreateDto personDto);
		Task<ServiceResult> DeletePersonAsync(int personId);
		Task<List<PersonListDto>> GetPersonsListAsync();
		Task<object> GetPersonByIdAsync(int personId);
		Task<EmployeeCreationDetails> GetEmployeeCreationDetailsAsync();
		Task<ServiceResult> CreateSupplierAsync(SupplierCreateRequestDto supplierDto);
		Task<ServiceResult> UpdateSupplierAsync(int supplierId, SupplierCreateRequestDto supplierDto);
		Task<ServiceResult> DeleteSupplierAsync(int supplierId);
		Task<List<SupplierDto>> GetSuppliersListAsync();
		Task<ServiceResult> GetSupplierByIdAsync(int supplierId);
		Task<ServiceResult> CreateMealTypeAsync(string mealTypeName, string description);
		Task<List<MealTypeDto>> GetMealTypeListAsync();
		Task<ServiceResult> UpdateMealTypeAsync(int mealTypeId, MealTypeUpdateDto mealTypeDto);
		Task<ServiceResult> GetMealTypeByIdAsync(int mealTypeId);
		Task<ServiceResult> DeleteMealTypeAsync(int mealTypeId);
		Task<AddOnDto> GetAddOnsAsync();
		Task<ServiceResult> CreateMealCostAsync(MealCostDto mealCostDto);
		Task<ServiceResult> UpdateMealCostAsync(int mealCostId, MealCostDto mealCostDto);
		Task<ServiceResult> DeleteMealCostAsync(int mealCostId);
		Task<MealCostCreationDetails> GetMealCostCreationDetails();
		Task<List<MealCostDetails>> GetMealCostsListAsync();
		Task<ServiceResult> GetMealCostByIdAsync(int mealCostId);
			Task<ServiceResult> UpdateSettingsAsync(ApplicationSettings updatedSettings);
		Task<ServiceResult> GetApplicationSettingsAsync();
	}
}
