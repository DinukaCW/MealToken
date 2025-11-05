using Authentication.Models.Entities;
using MealToken.Domain.Entities;
using MealToken.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Interfaces
{
	public interface IAdminRepository
	{
		Task<Person> GetPersonByIdAsync(int personId);
		Task<Person> GetPersonByNumberAsync(string personNumber);
		Task<Person> GetPersonByNICAsync(string nicNumber);
		Task<List<Person>> GetAllPersonsAsync();
		Task<List<Person>> GetPersonsByDepartmentAsync(int departmentId);
		Task CreatePersonAsync(Person person);
		Task UpdatePersonAsync(Person person);
		Task DeletePersonAsync(int personId);
		Task<List<Department>> GetDepartmentsAsync();
		Task<List<Designation>> GetDesignationsAsync();
		Task<string?> GetDepartmentByIdAsync(int departmentId);
		Task<string?> GetUserNameByIdAsync(int userId);
        Task<string?> GetDesignationByIdAsync(int designationId);
		Task<Supplier> GetSupplierByEmailAsync(string email);
		Task<Supplier> GetSupplierByIdAsync(int supplierId);
		Task CreateSupplierAsync(Supplier supplier);
		Task UpdateSupplierAsync(Supplier supplier);
		Task DeleteSupplierAsync(int supplierId);
		Task<List<Supplier>> GetAllSupplierAsync();
		Task CreateMealTypeAsync(MealType mealType);
		Task UpdateMealTypeAsync(MealType mealType);
		Task<List<MealTypeDto>> GetMealTypesAsync();
		Task<MealType> GetMealTypeByIdAsync(int mealTypeId);
		Task<MealType> GetMealTypeByNameAsync(string name);
		Task CreateMealSubTypesAsync(List<MealSubType> mealSubTypes);
		Task DeleteMealSubTypesAsync(int mealTypeId);
		Task CreateMealAddOnAsync(List<MealAddOn> mealAddOns);
		Task DeleteMealAddOnAsync(int mealTypeId);
		Task<List<MealSubType>> GetMealSubTypesAsync(int mealTypeId);
		Task<List<MealAddOn>> GetMealAddOnsAsync(int mealTypeId);
		Task<MealSubType> GetMealSubTypesByIdAsync(int mealSubTypeId);
		Task<MealCost?> GetMealCostByDetailAsync(int supplierId, int mealTypeId, int? mealSubTypeId);
		Task AddMealCostAsync(MealCost mealCost);
		Task UpdateMealCostAsync(MealCost mealCost);
		Task<MealCost> GetMealCostByIdAsync(int mealCostId);
		Task DeleteMealCostAsync(MealCost mealCost);
		Task<List<MealCost>> GetAllMealCostsAsync();
		Task<string> GetSupplierNameAsync(int supplierId);
		Task<string> GetMealTypeNameAsync(int mealTypeId);
		Task<string> GetMealSubTypeNameAsync(int mealSubTypeId);
		Task DeleteMealTypeAsync(MealType mealType);
		Task<List<MealSubType>> GetMealSubTypesListAsync();
		Task<User> GetUserByIdAsync(int userId);
		Task<List<MealCost>> GetMealCostsByIdsAsync(IEnumerable<int> mealCostIds);
		Task<List<Person>> GetPersonsByDepartmentsAsync(List<int> departmentIds);
		Task<List<DepartmentD>> GetDepartmentsByIdsAsync(List<int> departmentIds);
		Task<List<DesignationD>> GetDesignationsByIdsAsync(List<int> designationIds);
		Task<List<MealSubType>> GetMealSubTypesByMealTypeIdAsync(int mealTypeId);
		Task UpdateMealSubTypesAsync(List<MealSubType> subTypes);
		Task SoftDeleteMealSubTypesAsync(int mealTypeId);
		Task SoftDeleteMealSubTypesByIdsAsync(List<int> subTypeIds);
		Task UpdateMealAddOnsAsync(List<MealAddOn> addOns);
		Task<List<MealAddOn>> GetMealAddOnsByMealTypeIdAsync(int mealTypeId);
		Task<MealCost> GetMealCostByDetailsAsync(int mealTypeId, int? mealSubTypeId);
		Task<int> GetMealTypeIdbyNameAsync(string mealTypeName);
		Task<IEnumerable<MealType>> GetMealTypesByIdsAsync(IEnumerable<int> ids);
		Task<IEnumerable<Supplier>> GetSuppliersByIdsAsync(IEnumerable<int> ids);
		Task<IEnumerable<MealSubType>> GetMealSubTypesByIdsAsync(IEnumerable<int> ids);
		Task<IEnumerable<Person>> GetPeopleByIdsAsync(IEnumerable<int> ids);
		Task<List<ScheduleMeal>> GetScheduleMealsByMealTypeIdAsync(int mealTypeId);
		Task UpdateScheduleMeals(IEnumerable<ScheduleMeal> meals);
		Task UpdateTenantSettingsAsync(TenantInfo tenant);
		Task<TenantInfo> GetTenantInfoByIdAsync(int tenantId);
		Task<List<MealCost>> GetMealCostsByIdsAsync(List<int> mealCostIds);

	}
}
