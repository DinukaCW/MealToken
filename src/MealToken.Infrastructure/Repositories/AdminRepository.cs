using Authentication.Models.Entities;
using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Interfaces;
using MealToken.Domain.Models;
using MealToken.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Infrastructure.Repositories
{
	public class AdminRepository : IAdminRepository
	{
		private readonly MealTokenDbContext _tenantContext;
		private readonly PlatformDbContext _platformContext;
		private readonly ITenantContext _currentTenant;
		private readonly ILogger<AdminRepository> _logger;

		public AdminRepository(
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
		// Employee CRUD operations
		public async Task<Person> GetPersonByIdAsync(int personId)
		{
			return await _tenantContext.Person.FindAsync(personId);
		}

		public async Task<Person> GetPersonByNumberAsync(string personNumber)
		{
			return await _tenantContext.Person
				.Where(e => e.PersonNumber == personNumber && e.IsActive)
				.FirstOrDefaultAsync();
        }

		public async Task<Person> GetPersonByNICAsync(string nicNumber)
		{
			return await _tenantContext.Person
				.Where(e => e.NICNumber == nicNumber && e.IsActive)
                .FirstOrDefaultAsync();
		}

		public async Task<List<Person>> GetAllPersonsAsync()
		{
			return await _tenantContext.Person
				.Where(e => e.IsActive)
				.ToListAsync();
		}

		public async Task<List<Person>> GetPersonsByDepartmentAsync(int departmentId)
		{
			return await _tenantContext.Person
				.Where(e => e.DepartmentId == departmentId && e.IsActive)
				.OrderBy(e => e.Name)
				.ToListAsync();
		}

		public async Task CreatePersonAsync(Person person)
		{
			await _tenantContext.Person.AddAsync(person);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task UpdatePersonAsync(Person person)
		{
			_tenantContext.Person.Update(person);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task DeletePersonAsync(int personId)
		{
			var employee = await _tenantContext.Person.FindAsync(personId);
			if (employee != null)
			{
				_tenantContext.Person.Remove(employee);
				await _tenantContext.SaveChangesAsync();
			}
		}


		// Lookup table operations (if they're in tenant context)
		public async Task<List<Department>> GetDepartmentsAsync()
		{
			return await _platformContext.Department
				.Where(d => d.IsActive)
				.OrderBy(d => d.Name)
				.ToListAsync();
		}

		public async Task<List<Designation>> GetDesignationsAsync()
		{
			return await _platformContext.Designation
				.Where(d => d.IsActive)
				.OrderBy(d => d.Title)
				.ToListAsync();
		}
		public async Task<string?> GetDepartmentByIdAsync(int departmentId)
		{
			return await _platformContext.Department
				.Where(d => d.DepartmnetId == departmentId)
				.Select(d => d.Name)
				.FirstOrDefaultAsync();
		}
        public async Task<string?> GetUserNameByIdAsync(int userId)
        {
            return await _tenantContext.Users
                .Where(d => d.UserID == userId)
                .Select(d => d.FullName)
                .FirstOrDefaultAsync();
        }
        public async Task<string?> GetDesignationByIdAsync(int designationId)
		{
			return await _platformContext.Designation
				.Where(d => d.DesignationId == designationId)
				.Select(d => d.Title)
				.FirstOrDefaultAsync();
		}
		public async Task<Supplier> GetSupplierByEmailAsync(string email)
		{
			return await _tenantContext.Supplier
				.Where(e => e.IsActive)
                .FirstOrDefaultAsync(e => e.Email == email );
		}
		public async Task<Supplier> GetSupplierByIdAsync(int supplierId)
		{
			return await _tenantContext.Supplier.FindAsync(supplierId);
		}
		public async Task CreateSupplierAsync(Supplier supplier)
		{
			await _tenantContext.Supplier.AddAsync(supplier);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task UpdateSupplierAsync(Supplier supplier)
		{
			_tenantContext.Supplier.Update(supplier);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task DeleteSupplierAsync(int supplierId)
		{
			var supplier = await _tenantContext.Supplier.FindAsync(supplierId);
			if (supplier != null)
			{
				_tenantContext.Supplier.Remove(supplier);
				await _tenantContext.SaveChangesAsync();
			}
		}
		public async Task<List<Supplier>> GetAllSupplierAsync()
		{
			return await _tenantContext.Supplier
				.Where(e => e.IsActive)
				.OrderBy(e => e.SupplierName)
				.ToListAsync();
		}

		public async Task CreateMealTypeAsync(MealType mealType)
		{
			await _tenantContext.MealType.AddAsync(mealType);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task UpdateMealTypeAsync(MealType mealType)
		{
			_tenantContext.MealType.Update(mealType);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task<List<MealTypeDto>> GetMealTypesAsync()
		{
			return await _tenantContext.MealType
                .Where(d => d.IsActive)
                .Select(d => new MealTypeDto
				{
					MealTypeId = d.MealTypeId,
					MealTypeName = d.TypeName,
					Description = d.Description
				}) 
                .ToListAsync();
		}
		public async Task<MealType> GetMealTypeByIdAsync(int mealTypeId)
		{
			return await _tenantContext.MealType
				.Where(e => e.IsActive && e.MealTypeId == mealTypeId)
                .FirstOrDefaultAsync();
		}
		public async Task<MealType> GetMealTypeByNameAsync(string name)
		{
			return await _tenantContext.MealType
                .Where(e => e.IsActive)
                .FirstOrDefaultAsync(e => e.TypeName == name);
		}
		public async Task CreateMealSubTypesAsync(List<MealSubType> mealSubTypes)
		{
			if (mealSubTypes == null || !mealSubTypes.Any())
				throw new ArgumentException("Meal subtypes list cannot be empty.");

			await _tenantContext.MealSubType.AddRangeAsync(mealSubTypes);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task DeleteMealSubTypesAsync(int mealTypeId)
		{
			// Find all subtypes for the given MealTypeId
			var subTypes = await _tenantContext.MealSubType
				.Where(st => st.MealTypeId == mealTypeId)
				.ToListAsync();

			if (subTypes.Any())
			{
				_tenantContext.MealSubType.RemoveRange(subTypes);
				await _tenantContext.SaveChangesAsync();
			}
		}
		public async Task CreateMealAddOnAsync(List<MealAddOn> mealAddOns)
		{
			if (mealAddOns == null || !mealAddOns.Any())
				throw new ArgumentException("Meal Addon list cannot be empty.");

			await _tenantContext.MealAddOn.AddRangeAsync(mealAddOns);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task DeleteMealAddOnAsync(int mealTypeId)
		{
			var addOns = await _tenantContext.MealAddOn
				.Where(st => st.MealTypeId == mealTypeId)
				.ToListAsync();

			if (addOns.Any())
			{
				_tenantContext.MealAddOn.RemoveRange(addOns);
				await _tenantContext.SaveChangesAsync();
			}
		}
		public async Task<List<MealSubType>> GetMealSubTypesAsync(int mealTypeId)
		{
			return await _tenantContext.MealSubType
				.Where(e => e.MealTypeId == mealTypeId && e.IsActive)
				.ToListAsync();
		}
		public async Task<List<MealAddOn>> GetMealAddOnsAsync(int mealTypeId)
		{
			return await _tenantContext.MealAddOn
				.Where(e => e.MealTypeId == mealTypeId)
				.ToListAsync();
		}
		public async Task<MealSubType> GetMealSubTypesByIdAsync(int mealSubTypeId)
		{
			return await _tenantContext.MealSubType
				.Where(s => s.MealSubTypeId == mealSubTypeId && s.IsActive)
				.FirstOrDefaultAsync();
		}
		public async Task<MealCost?> GetMealCostByDetailAsync(int supplierId, int mealTypeId, int? mealSubTypeId)
		{
			return await _tenantContext.MealCost
				.FirstOrDefaultAsync(e =>
					e.SupplierId == supplierId
					&& e.MealTypeId == mealTypeId
					&& e.MealSubTypeId == mealSubTypeId
					&& e.IsActive);
		}

		public async Task AddMealCostAsync(MealCost mealCost)
		{
			await _tenantContext.MealCost.AddAsync(mealCost);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task UpdateMealCostAsync(MealCost mealCost)
		{
			_tenantContext.MealCost.Update(mealCost);
			await _tenantContext.SaveChangesAsync();
		}
		public async Task<MealCost> GetMealCostByIdAsync(int mealCostId)
		{
			return await _tenantContext.MealCost
				.Where(mc => mc.MealCostId == mealCostId && mc.IsActive)
				.FirstOrDefaultAsync();
		}
		public async Task DeleteMealCostAsync(MealCost mealCost)
		{
			_tenantContext.MealCost.Remove(mealCost);
			await _tenantContext.SaveChangesAsync();

		}
		public async Task<List<MealCost>> GetAllMealCostsAsync()
		{
			return await _tenantContext.MealCost
				.Where(mc => mc.IsActive)
                .ToListAsync();
		}
		public async Task<string> GetSupplierNameAsync(int supplierId)
		{
			return await _tenantContext.Supplier
				.Where(d => d.SupplierId == supplierId)
				.Select(d => d.SupplierName)
				.FirstOrDefaultAsync();
		}
		public async Task<string> GetMealTypeNameAsync(int mealTypeId)
		{
			return await _tenantContext.MealType
				.Where(d => d.MealTypeId == mealTypeId)
				.Select(d => d.TypeName)
				.FirstOrDefaultAsync();

		}
		public async Task<string> GetMealSubTypeNameAsync(int mealSubTypeId)
		{
			return await _tenantContext.MealSubType
				.Where(d => d.MealSubTypeId == mealSubTypeId)
				.Select(d => d.SubTypeName)
				.FirstOrDefaultAsync();
		}

		public async Task DeleteMealTypeAsync(MealType mealType)
		{
			_tenantContext.MealType.Remove(mealType);
			await _tenantContext.SaveChangesAsync();		
		}
		public async Task<List<MealSubType>> GetMealSubTypesListAsync()
		{
			return await _tenantContext.MealSubType
				.ToListAsync();
		}
		public async Task<User> GetUserByIdAsync(int userId)
		{
			return await _tenantContext.Users.FindAsync(userId);
		}
        public async Task<List<MealCost>> GetMealCostsByIdsAsync(IEnumerable<int> mealCostIds)
        {          
            return await _tenantContext.MealCost
                .Where(mc => mealCostIds.Contains(mc.MealCostId)&& mc.IsActive)
                .ToListAsync();
        }
        public async Task<List<Person>> GetPersonsByDepartmentsAsync(List<int> departmentIds)
        {
            return await _tenantContext.Person
                .Where(p => departmentIds.Contains(p.DepartmentId))
                .ToListAsync();
        }

        public async Task<List<DepartmentD>> GetDepartmentsByIdsAsync(List<int> departmentIds)
        {
            return await _platformContext.Department
                .Where(d => departmentIds.Contains(d.DepartmnetId))
                .Select(d => new DepartmentD
				{
					DepartmentId = d.DepartmnetId,
					Name = d.Name
                })
                .ToListAsync();
        }

        public async Task<List<DesignationD>> GetDesignationsByIdsAsync(List<int> designationIds)
        {
            return await _platformContext.Designation
                .Where(d => designationIds.Contains(d.DesignationId))
                .Select(d => new DesignationD
				{
					DesignationId = d.DesignationId,
					Name = d.Title
                })
                .ToListAsync();
        }
        public async Task<List<MealSubType>> GetMealSubTypesByMealTypeIdAsync(int mealTypeId)
        {
            return await _tenantContext.MealSubType
                .Where(s => s.MealTypeId == mealTypeId && s.IsActive)
                .ToListAsync();
        }

        public async Task UpdateMealSubTypesAsync(List<MealSubType> subTypes)
        {
            _tenantContext.MealSubType.UpdateRange(subTypes);
            await _tenantContext.SaveChangesAsync();
        }

        public async Task SoftDeleteMealSubTypesAsync(int mealTypeId)
        {
            var subTypes = await _tenantContext.MealSubType
                .Where(s => s.MealTypeId == mealTypeId && s.IsActive)
                .ToListAsync();

            foreach (var subType in subTypes)
            {
                subType.IsActive = false;
            }

            await _tenantContext.SaveChangesAsync();
        }

        public async Task SoftDeleteMealSubTypesByIdsAsync(List<int> subTypeIds)
        {
            var subTypes = await _tenantContext.MealSubType
                .Where(s => subTypeIds.Contains(s.MealSubTypeId))
                .ToListAsync();

            foreach (var subType in subTypes)
            {
                subType.IsActive = false;
            }

            await _tenantContext.SaveChangesAsync();
        }

        // MealAddOn Methods
        public async Task<List<MealAddOn>> GetMealAddOnsByMealTypeIdAsync(int mealTypeId)
        {
            return await _tenantContext.MealAddOn
                .Where(a => a.MealTypeId == mealTypeId)
                .ToListAsync();
        }

        public async Task UpdateMealAddOnsAsync(List<MealAddOn> addOns)
        {
           _tenantContext.MealAddOn.UpdateRange(addOns);
            await _tenantContext.SaveChangesAsync();
        }
        public async Task<MealCost> GetMealCostByDetailsAsync(int mealTypeId, int? mealSubTypeId)
        {
            return await _tenantContext.MealCost
                .Where(a =>
                    a.MealTypeId == mealTypeId &&
                    ((mealSubTypeId == null && a.MealSubTypeId == null) || a.MealSubTypeId == mealSubTypeId)
                )
                .FirstOrDefaultAsync();
        }
        public async Task<int> GetMealTypeIdbyNameAsync(string mealTypeName)
        {
            return await _tenantContext.MealType
                .Where(m => m.TypeName == mealTypeName  )
				.Select(m => m.MealTypeId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MealType>> GetMealTypesByIdsAsync(IEnumerable<int> ids)
        {
           
              if (ids == null || !ids.Any())
                   return new List<MealType>();

                var idList = ids.Distinct().ToList();
                return await _tenantContext.MealType
                    .Where(mt => idList.Contains(mt.MealTypeId))
                    .AsNoTracking()
                    .ToListAsync();
    
        }

        public async Task<IEnumerable<Supplier>> GetSuppliersByIdsAsync(IEnumerable<int> ids)
        {
                if (ids == null || !ids.Any())
                    return new List<Supplier>();

                var idList = ids.Distinct().ToList();
                return await _tenantContext.Supplier
                    .Where(s => idList.Contains(s.SupplierId))
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task<IEnumerable<MealSubType>> GetMealSubTypesByIdsAsync(IEnumerable<int> ids)
        {
                if (ids == null || !ids.Any())
                    return new List<MealSubType>();

                var idList = ids.Distinct().ToList();
                return await _tenantContext.MealSubType
                    .Where(mst => idList.Contains(mst.MealSubTypeId))
                    .AsNoTracking()
                    .ToListAsync();

        }
        public async Task<IEnumerable<Person>> GetPeopleByIdsAsync(IEnumerable<int> ids)
        {
                if (ids == null || !ids.Any())
                    return new List<Person>();

                var idList = ids.Distinct().ToList();
                return await _tenantContext.Person
                    .Where(p => idList.Contains(p.PersonId))
                    .AsNoTracking()
                    .ToListAsync();
        }
		public async Task<List<ScheduleMeal>> GetScheduleMealsByMealTypeIdAsync(int mealTypeId)
		{
			return await _tenantContext.ScheduleMeal
				.Where(a => a.MealTypeId == mealTypeId)
				.ToListAsync();
		}

		public async Task UpdateScheduleMeals(IEnumerable<ScheduleMeal> meals)
		{
			_tenantContext.ScheduleMeal.UpdateRange(meals);
			await _tenantContext.SaveChangesAsync();
		}

		public async Task UpdateTenantSettingsAsync(TenantInfo tenant)
		{
			_platformContext.TenantInfo.Update(tenant);
			await _platformContext.SaveChangesAsync();
		}
		public async Task<TenantInfo> GetTenantInfoByIdAsync(int tenantId)
		{
			return await _platformContext.TenantInfo
				.Where(m => m.Id == tenantId)
				.FirstOrDefaultAsync();
		}
	}
}
