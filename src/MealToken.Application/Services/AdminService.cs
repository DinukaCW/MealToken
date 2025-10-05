using Authentication.Interfaces;
using Authentication.Models.DTOs;
using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using MealToken.Domain.Enums;
using MealToken.Domain.Interfaces;
using MealToken.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static MealToken.Domain.Models.MealTypeUpdateDto;

namespace MealToken.Application.Services
{
	public class AdminService : IAdminService
	{
		private readonly IEncryptionService _encryption;
		private readonly IAdminRepository _adminData;
		private readonly ILogger<AdminService> _logger;
		private readonly ITenantContext _tenantContext;
		public AdminService(IEncryptionService encryptionService, IAdminRepository adminData, ILogger<AdminService> logger, ITenantContext tenantContext)
		{
			_encryption = encryptionService;
			_adminData = adminData;
			_logger = logger;
			_tenantContext = tenantContext;
		}
		public async Task<ServiceResult> AddPersonAsync(PersonCreateDto personDto)
		{
			try
			{
				_logger.LogInformation("Creating new person: {PersonNumber}", personDto.PersonNumber);

				// Check if person number already exists
				var existingPerson = await _adminData.GetPersonByNumberAsync(personDto.PersonNumber);
				if (existingPerson != null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Person number already exists."
					};
				}

				// Check if NIC already exists (only if NIC is provided)
				if (!string.IsNullOrWhiteSpace(personDto.NICNumber))
				{
					var existingNIC = await _adminData.GetPersonByNICAsync(_encryption.EncryptData(personDto.NICNumber));
					if (existingNIC != null)
					{
						return new ServiceResult
						{
							Success = false,
							Message = "NIC number already exists."
						};
					}
				}

				var person = new Person
				{
					PersonType = personDto.PersonType,
					PersonNumber = personDto.PersonNumber, // Updated from EmployeeNumber
					TenantId = _tenantContext.TenantId.Value,
					Name = personDto.Name, // Updated from FullName
					NICNumber = !string.IsNullOrWhiteSpace(personDto.NICNumber) ?
					   _encryption.EncryptData(personDto.NICNumber) : null,
					JoinedDate = personDto.JoinedDate,
					DepartmentId = personDto.DepartmentId,
					DesignationId = personDto.DesignationId ?? 11,
					EmployeeGrade = personDto.EmployeeGrade,
					PersonSubType = personDto.PersonSubType, // Updated from EmployeeType
					Gender = personDto.Gender,
					MealGroup = personDto.MealGroup,
					MealEligibility = personDto.MealEligibility,
					IsActive = personDto.IsActive, // Updated from ActiveEmployee
					SpecialNote = personDto.SpecialNote,
					CreatedAt = DateTime.UtcNow,
				};

				await _adminData.CreatePersonAsync(person);

				_logger.LogInformation("Person Added successfully: {PersonId}",person.PersonId);

				return new ServiceResult
				{
					Success = true,
					Message = $"{personDto.PersonType} created successfully.",
					ObjectId = person.PersonId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating person: {PersonNumber}", personDto.PersonNumber);
				return new ServiceResult
				{
					Success = false,
					Message = "Person creation error. Please try again."
				};
			}
		}

		public async Task<ServiceResult> UpdatePersonAsync(int personId, PersonCreateDto personDto)
		{
			try
			{
				_logger.LogInformation("Updating person with ID: {PersonId}", personId);

				// Get existing person
				var existingPerson = await _adminData.GetPersonByIdAsync(personId);
				if (existingPerson == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Person not found."
					};
				}

				// Check if person number is being changed and if new number already exists
				if (!string.Equals(existingPerson.PersonNumber, personDto.PersonNumber, StringComparison.OrdinalIgnoreCase))
				{
					var personWithNumber = await _adminData.GetPersonByNumberAsync(personDto.PersonNumber);
					if (personWithNumber != null && personWithNumber.PersonId != personId)
					{
						return new ServiceResult
						{
							Success = false,
							Message = "Person number is already in use by another person."
						};
					}
				}

				// Check if NIC is being changed and if new NIC already exists (only if NIC is provided)
				if (!string.IsNullOrWhiteSpace(personDto.NICNumber))
				{
					var encryptedNewNIC = _encryption.EncryptData(personDto.NICNumber);
					if (!string.Equals(existingPerson.NICNumber, encryptedNewNIC, StringComparison.OrdinalIgnoreCase))
					{
						var personWithNIC = await _adminData.GetPersonByNICAsync(encryptedNewNIC);
						if (personWithNIC != null && personWithNIC.PersonId != personId)
						{
							return new ServiceResult
							{
								Success = false,
								Message = "NIC number is already in use by another person."
							};
						}
					}
				}

				// Model validation will be handled by the framework via IValidatableObject

				// Update fields
				existingPerson.PersonType = personDto.PersonType;
				existingPerson.PersonNumber = personDto.PersonNumber;
				existingPerson.Name = personDto.Name;
				existingPerson.NICNumber = !string.IsNullOrWhiteSpace(personDto.NICNumber) ?
										  _encryption.EncryptData(personDto.NICNumber) : null;
				existingPerson.JoinedDate = personDto.JoinedDate;
				existingPerson.DepartmentId = personDto.DepartmentId;
				existingPerson.DesignationId = personDto.DesignationId;
				existingPerson.EmployeeGrade = personDto.EmployeeGrade;
				existingPerson.PersonSubType = personDto.PersonSubType;
				existingPerson.Gender = personDto.Gender;
				existingPerson.MealGroup = personDto.MealGroup;
				existingPerson.MealEligibility = personDto.MealEligibility;
				existingPerson.IsActive = personDto.IsActive;
				existingPerson.SpecialNote = personDto.SpecialNote;

				await _adminData.UpdatePersonAsync(existingPerson);

				_logger.LogInformation("Person updated successfully: {PersonId}", personId);

				return new ServiceResult
				{
					Success = true,
					Message = $"{personDto.PersonType} updated successfully.",
					ObjectId = personId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating person with ID: {PersonId}", personId);
				return new ServiceResult
				{
					Success = false,
					Message = "Person update error. Please try again."
				};
			}
		}


		public async Task<ServiceResult> DeletePersonAsync(int personId)
		{
			try
			{
				_logger.LogInformation("Deleting person with ID: {PersonId}", personId);

				// Check if person exists
				var existingPerson = await _adminData.GetPersonByIdAsync(personId);
				if (existingPerson == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Person not found."
					};
				}

				// Check if person is already inactive (soft delete check)
				if (!existingPerson.IsActive)
				{
					return new ServiceResult
					{
						Success = false,
						Message = $"{existingPerson.PersonType} is already inactive."
					};
				}

				// Perform soft delete by setting IsActive to false
				existingPerson.IsActive = false;

				await _adminData.UpdatePersonAsync(existingPerson);

				_logger.LogInformation("Person deleted successfully: {PersonId}", personId);

				return new ServiceResult
				{
					Success = true,
					Message = $"{existingPerson.PersonType} deleted successfully.",
					ObjectId = personId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting person with ID: {PersonId}", personId);
				return new ServiceResult
				{
					Success = false,
					Message = "Person deletion error. Please try again."
				};
			}
		}

		public async Task<List<PersonListDto>> GetPersonsListAsync()
		{
			try
			{
				_logger.LogInformation("Retrieving persons list");
				var persons = await _adminData.GetAllPersonsAsync();

				if (persons == null || !persons.Any())
				{
					_logger.LogInformation("No persons found in the database");
					return new List<PersonListDto>();
				}

				var personDtos = new List<PersonListDto>();

				foreach (var person in persons)
				{
					var dto = new PersonListDto
					{
						PersonId = person.PersonId,
						PersonType = person.PersonType,
						PersonNumber = person.PersonNumber,
						Name = person.Name,
						NICNumber = !string.IsNullOrWhiteSpace(person.NICNumber) ?
								   _encryption.DecryptData(person.NICNumber) : null,
						JoinedDate = person.JoinedDate,
						DepartmentId = person.DepartmentId,
						DepartmentName = await _adminData.GetDepartmentByIdAsync(person.DepartmentId),
						DesignationId = person.DesignationId,
						DesignationName = person.DesignationId.HasValue && person.DesignationId > 0
							? (await _adminData.GetDesignationByIdAsync(person.DesignationId.Value))
							: null,
						EmployeeGrade = person.EmployeeGrade,
						PersonSubType = person.PersonSubType,
						Gender = person.Gender,
						MealGroup = person.MealGroup,
						MealEligibility = person.MealEligibility,
						IsActive = person.IsActive,
						SpecialNote = person.SpecialNote
					};
					personDtos.Add(dto);
				}

				_logger.LogInformation("Retrieved {PersonCount} persons successfully", personDtos.Count);
				return personDtos;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving persons list");
				return new List<PersonListDto>();
			}
		}

		public async Task<object> GetPersonByIdAsync(int personId)
		{
			try
			{
				_logger.LogInformation("Retrieving person by Id: {PersonId}", personId);
				var person = await _adminData.GetPersonByIdAsync(personId);

				if (person == null)
				{
					_logger.LogInformation("No person found with Id: {PersonId}", personId);
					return null;
				}

				if (person.PersonType == PersonType.Employer)
				{
					var employeeDto = new EmployeeListDto
					{
						EmployeeId = person.PersonId,
						EmployeeNumber = person.PersonNumber,
						FullName = person.Name,
						NICNumber = !string.IsNullOrWhiteSpace(person.NICNumber) ?
								   _encryption.DecryptData(person.NICNumber) : string.Empty,
						JoinedDate = person.JoinedDate ?? DateTime.MinValue,
						DepartmentId = person.DepartmentId ,
						DepartmentName = await _adminData.GetDepartmentByIdAsync(person.DepartmentId),
						DesignationId = person.DesignationId ?? 0,
						DesignationName = person.DesignationId.HasValue && person.DesignationId > 0
							? (await _adminData.GetDesignationByIdAsync(person.DesignationId.Value)) ?? string.Empty
							: string.Empty,
						EmployeeGrade = person.EmployeeGrade,
						EmployeeType = person.PersonSubType,
						Gender = person.Gender ?? string.Empty,
						MealGroup = person.MealGroup,
						MealEligibility = person.MealEligibility,
						ActiveEmployee = person.IsActive,
					};
					return employeeDto;
				}
				else if (person.PersonType == PersonType.Visitor)
				{
					var visitorDto = new VisitorListDto
					{
						VisitorId = person.PersonId,
						CardNumber = person.PersonNumber,
						CardName = person.Name,
						VisitorType = person.PersonSubType,
						DepartmentId = person.DepartmentId,
						DepartmentName = await _adminData.GetDepartmentByIdAsync(person.DepartmentId),
						SpecialNote = person.SpecialNote,
						MealEligibility = person.MealEligibility,
						ActiveVisitor = person.IsActive
					};
					return visitorDto;
				}

				_logger.LogInformation("Retrieved person successfully: {PersonId}", personId);
				return null;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving person with Id: {PersonId}", personId);
				return null;
			}
		}

		public async Task<EmployeeCreationDetails> GetEmployeeCreationDetailsAsync()
		{
			try
			{
				_logger.LogInformation("Retrieving employee creation details");

				var departments= await _adminData.GetDepartmentsAsync();
				var designations = await _adminData.GetDesignationsAsync();

				var result = new EmployeeCreationDetails();

				// Map departments
				if (departments != null && departments.Any())
				{
					result.Departments = departments.Select(d => new DepartmentDto
					{
						DepartmentId = d.DepartmnetId,
						DepartmentName = d.Name
					}).ToList();
				}

				// Map designations
				if (designations != null && designations.Any())
				{
					result.Designations = designations.Select(d => new DesignationDto
					{
						DesignationId = d.DesignationId,
						DesignationName = d.Title
					}).ToList();
				}

				_logger.LogInformation("Retrieved {DepartmentCount} departments and {DesignationCount} designations",
									  result.Departments.Count, result.Designations.Count);

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving employee creation details");
				return new EmployeeCreationDetails(); // Return empty object on error
			}
		}

		public async Task<ServiceResult> CreateSupplierAsync(SupplierCreateRequestDto supplierDto)
		{
			try
			{
				_logger.LogInformation("Creating new supplier: {SupplierName}", supplierDto.SupplierName);

				// Check if email already exists
				var existingSupplier = await _adminData.GetSupplierByEmailAsync(_encryption.EncryptData(supplierDto.Email));
				if (existingSupplier != null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "A supplier with this email already exists."
					};
				}

				var supplier = new Supplier
				{
					TenantId = _tenantContext.TenantId.Value,
					SupplierName = supplierDto.SupplierName,
					ContactNumber = _encryption.EncryptData(supplierDto.ContactNumber),
					Email = _encryption.EncryptData(supplierDto.Email),
					Address = _encryption.EncryptData(supplierDto.Address),
					SupplierRating = supplierDto.SupplierRating,
					IsActive = supplierDto.IsActive,
					CreatedAt = DateTime.UtcNow
				};

				await _adminData.CreateSupplierAsync(supplier);

				_logger.LogInformation("Supplier created successfully: {SupplierId}", supplier.SupplierId);

				return new ServiceResult
				{
					Success = true,
					Message = "Supplier created successfully.",
					ObjectId = supplier.SupplierId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating supplier: {SupplierName}", supplierDto.SupplierName);
				return new ServiceResult
				{
					Success = false,
					Message = "Supplier creation error. Please try again."
				};
			}
		}

		// Update Supplier
		public async Task<ServiceResult> UpdateSupplierAsync(int supplierId, SupplierCreateRequestDto supplierDto)
		{
			try
			{
				_logger.LogInformation("Updating supplier with ID: {SupplierId}", supplierId);

				var existingSupplier = await _adminData.GetSupplierByIdAsync(supplierId);
				if (existingSupplier == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Supplier not found."
					};
				}

				// Check if email is being changed and if new email already exists
				if (!string.Equals(existingSupplier.Email, _encryption.EncryptData(supplierDto.Email), StringComparison.OrdinalIgnoreCase))
				{
					var supplierWithEmail = await _adminData.GetSupplierByEmailAsync(_encryption.EncryptData(supplierDto.Email));
					if (supplierWithEmail != null && supplierWithEmail.SupplierId != supplierId)
					{
						return new ServiceResult
						{
							Success = false,
							Message = "Email is already in use by another supplier."
						};
					}
				}

				// Update fields
				existingSupplier.SupplierName = supplierDto.SupplierName;
				existingSupplier.ContactNumber = _encryption.EncryptData(supplierDto.ContactNumber);
				existingSupplier.Email = _encryption.EncryptData(supplierDto.Email);
				existingSupplier.Address = _encryption.EncryptData(supplierDto.Address);
				existingSupplier.SupplierRating = supplierDto.SupplierRating;
				existingSupplier.IsActive = supplierDto.IsActive;

				await _adminData.UpdateSupplierAsync(existingSupplier);

				_logger.LogInformation("Supplier updated successfully: {SupplierId}", supplierId);

				return new ServiceResult
				{
					Success = true,
					Message = "Supplier updated successfully.",
					ObjectId = supplierId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating supplier with ID: {SupplierId}", supplierId);
				return new ServiceResult
				{
					Success = false,
					Message = "Supplier update error. Please try again."
				};
			}
		}

		// Delete Supplier (Soft Delete)
		public async Task<ServiceResult> DeleteSupplierAsync(int supplierId)
		{
			try
			{
				_logger.LogInformation("Deleting supplier with ID: {SupplierId}", supplierId);

				var existingSupplier = await _adminData.GetSupplierByIdAsync(supplierId);
				if (existingSupplier == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Supplier not found."
					};
				}

				if (!existingSupplier.IsActive)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Supplier is already inactive."
					};
				}
				// Perform soft delete
				existingSupplier.IsActive = false;

				await _adminData.UpdateSupplierAsync(existingSupplier);

				_logger.LogInformation("Supplier deleted successfully: {SupplierId}", supplierId);

				return new ServiceResult
				{
					Success = true,
					Message = "Supplier deleted successfully.",
					ObjectId = supplierId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting supplier with ID: {SupplierId}", supplierId);
				return new ServiceResult
				{
					Success = false,
					Message = "Supplier deletion error. Please try again."
				};
			}
		}

		// Get Suppliers List
		public async Task<List<SupplierDto>> GetSuppliersListAsync()
		{
			try
			{
				_logger.LogInformation("Retrieving suppliers list");
				var suppliers = await _adminData.GetAllSupplierAsync();

				if (suppliers == null || !suppliers.Any())
				{
					_logger.LogInformation("No suppliers found in the database");
					return new List<SupplierDto>();
				}

				var supplierDtos = suppliers.Select(s => new SupplierDto
				{
					SupplierId = s.SupplierId,
					SupplierName = s.SupplierName,
					ContactNumber = _encryption.DecryptData(s.ContactNumber),
					Email = _encryption.DecryptData(s.Email),
					Address = _encryption.DecryptData(s.Address),
					SupplierRating = s.SupplierRating,
					IsActive = s.IsActive,
				}).ToList();

				_logger.LogInformation("Retrieved {SupplierCount} suppliers successfully", supplierDtos.Count);
				return supplierDtos;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving suppliers list");
				return new List<SupplierDto>();
			}
		}

		// Get Supplier by ID
		public async Task<ServiceResult> GetSupplierByIdAsync(int supplierId)
		{
			try
			{
				_logger.LogInformation("Retrieving supplier by ID: {SupplierId}", supplierId);
				var supplier = await _adminData.GetSupplierByIdAsync(supplierId);

				if (supplier == null)
				{
					_logger.LogInformation("No supplier found with ID: {SupplierId}", supplierId);
					return new ServiceResult
					{
						Success = false,
						Message = "Supplier not found.",
						Data = null
					};
				}

				var supplierDto = new SupplierDto
				{
					SupplierId = supplier.SupplierId,
					SupplierName = supplier.SupplierName,
					ContactNumber = _encryption.DecryptData(supplier.ContactNumber),
					Email = _encryption.DecryptData(supplier.Email),
					Address = _encryption.DecryptData(supplier.Address),
					SupplierRating = supplier.SupplierRating,
					IsActive = supplier.IsActive,
				};

				_logger.LogInformation("Retrieved supplier successfully: {SupplierId}", supplierId);
				return new ServiceResult
				{
					Success = true,
					Message = "Supplier retrieved successfully.",
					Data = supplierDto
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving supplier with ID: {SupplierId}", supplierId);
				return new ServiceResult
				{
					Success = false,
					Message = "Error retrieving supplier. Please try again.",
					Data = null
				};
			}
		}

		public async Task<ServiceResult> CreateMealTypeAsync(string mealTypeName, string? description)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(mealTypeName))
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Meal type name is required."
					};
				}

				var mealType = new MealType
				{
					TenantId = _tenantContext.TenantId.Value,
					TypeName = mealTypeName,
					Description = description ?? null,
					CreatedAt = DateTime.UtcNow
				};			

				await _adminData.CreateMealTypeAsync(mealType);
				_logger.LogInformation("Meal type created successfully: {MealTypeId}", mealType.TypeName);
				return new ServiceResult
				{
					Success = true,
					Message = "Meal type created successfully.",
					ObjectId = mealType.MealTypeId // Return the ID for later use
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating meal type with name: {MealTypeName}", mealTypeName);

				return new ServiceResult
				{
					Success = false,
					Message = "Error creating meal type. Please try again."
				};
			}
		}

		public async Task<List<MealTypeDto>> GetMealTypeListAsync()
		{
			try
			{
				_logger.LogInformation("Retrieving meal types list");
				var mealTypes = await _adminData.GetMealTypesAsync();

				if (mealTypes == null || !mealTypes.Any())
				{
					_logger.LogInformation("No meal types found in the database");
					return new List<MealTypeDto>();
				}

				_logger.LogInformation("Retrieved {MealTypeCount} meal types successfully", mealTypes.Count);
				return mealTypes;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving meal types list");
				return new List<MealTypeDto>();
			}
		}

		public async Task<ServiceResult> UpdateMealTypeAsync(int mealTypeId, MealTypeUpdateDto mealTypeDto)
		{
			try
			{
				_logger.LogInformation("Updating meal type with ID: {MealTypeId}", mealTypeId);

				var existingMealType = await _adminData.GetMealTypeByIdAsync(mealTypeId);
				if (existingMealType == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Meal type not found."
					};
				}

				// Check if meal type name is being changed and if new name already exists
				if (!string.Equals(existingMealType.TypeName, mealTypeDto.TypeName, StringComparison.OrdinalIgnoreCase))
				{
					var mealTypeWithName = await _adminData.GetMealTypeByNameAsync(mealTypeDto.TypeName);
					if (mealTypeWithName != null && mealTypeWithName.MealTypeId != mealTypeId)
					{
						return new ServiceResult
						{
							Success = false,
							Message = "Meal type name is already in use."
						};
					}
				}
				existingMealType.TokenIssueStartDate = mealTypeDto.TokenIssueStartDate;
				existingMealType.TokenIssueEndDate = mealTypeDto.TokenIssueEndDate;
				existingMealType.TokenIssueStartTime = mealTypeDto.TokenIssueStartTime;
				existingMealType.TokenIssueEndTime = mealTypeDto.TokenIssueEndTime;
				existingMealType.MealTimeStartDate = mealTypeDto.MealTimeStartDate;
				existingMealType.MealTimeEndDate = mealTypeDto.MealTimeEndDate;
				existingMealType.MealTimeStartTime = mealTypeDto.MealTimeStartTime;
				existingMealType.MealTimeEndTime = mealTypeDto.MealTimeEndTime;
				existingMealType.IsFunctionKeysEnable = mealTypeDto.IsFunctionKeysEnable;
				existingMealType.IsAddOnsEnable = mealTypeDto.IsAddOnsEnable;
				existingMealType.UpdatedAt = DateTime.UtcNow;

				await _adminData.UpdateMealTypeAsync(existingMealType);

				// Handle SubTypes if function keys are enabled
				if (mealTypeDto.IsFunctionKeysEnable)
				{
					await _adminData.DeleteMealSubTypesAsync(mealTypeId);

					// Add new sub types if any provided
					if (mealTypeDto.SubTypes.Any())
					{
						var mealSubTypes = new List<MealSubType>();

						foreach (var subType in mealTypeDto.SubTypes)
						{
							mealSubTypes.Add(new MealSubType
							{
								TenantId = _tenantContext.TenantId.Value,
								MealTypeId = mealTypeId,
								SubTypeName = subType.SubTypeName,
								Description = subType.Description,
								Functionkey = subType.Functionkey.ToUpper(),
								CreatedAt = DateTime.UtcNow,
							});
						}

						await _adminData.CreateMealSubTypesAsync(mealSubTypes);
						_logger.LogInformation("Created {SubTypeCount} meal sub types for meal type {MealTypeId}",
											 mealSubTypes.Count, mealTypeId);
					}
				}
				else
				{
					await _adminData.DeleteMealSubTypesAsync(mealTypeId);
					_logger.LogInformation("Removed all sub types for meal type {MealTypeId} as function keys are disabled", mealTypeId);
				}

				// Handle AddOns if enabled
				if (mealTypeDto.IsAddOnsEnable)
				{
					await _adminData.DeleteMealAddOnAsync(mealTypeId);

					// Add new add-ons if any provided
					if (mealTypeDto.AddOns.Any())
					{
						var mealAddOns = new List<MealAddOn>();

						foreach (var addOn in mealTypeDto.AddOns)
						{
							mealAddOns.Add(new MealAddOn
							{
								TenantId = _tenantContext.TenantId.Value,
								MealTypeId = mealTypeId,
								AddOnName = addOn.AddonName,
								AddOnType =addOn.AddonType,
								Description = addOn.Description
							});
						}

						await _adminData.CreateMealAddOnAsync(mealAddOns);
						_logger.LogInformation("Created {AddOnCount} meal add-ons for meal type {MealTypeId}",
											 mealAddOns.Count, mealTypeId);
					}
				}
				else
				{
					await _adminData.DeleteMealAddOnAsync(mealTypeId);
					_logger.LogInformation("Removed all add-ons for meal type {MealTypeId} as add-ons are disabled", mealTypeId);
				}

				_logger.LogInformation("Meal type updated successfully: {MealTypeId}", mealTypeId);

				return new ServiceResult
				{
					Success = true,
					Message = "Meal type updated successfully.",
					ObjectId = mealTypeId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating meal type with ID: {MealTypeId}", mealTypeId);
				return new ServiceResult
				{
					Success = false,
					Message = "Error updating meal type. Please try again."
				};
			}
		}
		public async Task<ServiceResult> GetMealTypeByIdAsync(int mealTypeId)
		{
			try
			{
				_logger.LogInformation("Retrieving meal type with ID: {MealTypeId}", mealTypeId);

				var mealType = await _adminData.GetMealTypeByIdAsync(mealTypeId);

				if (mealType == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Meal type not found."
					};
				}

				// Get SubTypes
				var subTypes = await _adminData.GetMealSubTypesAsync(mealTypeId);

				// Get AddOns
				var addOns = await _adminData.GetMealAddOnsAsync(mealTypeId);

				var dto = new MealTypeDetails
				{
					MealTypeId = mealType.MealTypeId,
					TypeName = mealType.TypeName,
					Description = mealType.Description,

					TokenIssueStartDate = mealType.TokenIssueStartDate, // if you stored as string like "today"
					TokenIssueEndDate = mealType.TokenIssueEndDate,
					TokenIssueStartTime = mealType.TokenIssueStartTime,
					TokenIssueEndTime = mealType.TokenIssueEndTime,

					MealTimeStartDate = mealType.MealTimeStartDate,
					MealTimeEndDate = mealType.MealTimeEndDate,
					MealTimeStartTime = mealType.MealTimeStartTime,
					MealTimeEndTime = mealType.MealTimeEndTime,

					IsFunctionKeysEnable = mealType.IsFunctionKeysEnable ?? false,
					IsAddOnsEnable = mealType.IsAddOnsEnable ?? false,

					SubTypes = subTypes.Select(st => new MealSubTypeDto
					{
						SubTypeName = st.SubTypeName,
						Description = st.Description,
						Functionkey = st.Functionkey
					}).ToList(),

					AddOns = addOns.Select(ao => new MealAddOns
					{
						AddonName = ao.AddOnName,
						Description = ao.Description
					}).ToList()
				};

				return new ServiceResult
				{
					Success = true,
					Message = "Meal type retrieved successfully.",
					ObjectId = mealTypeId,
					Data = dto
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving meal type with ID: {MealTypeId}", mealTypeId);
				return new ServiceResult
				{
					Success = false,
					Message = "Error retrieving meal type. Please try again."
				};
			}
		}

		public async Task<ServiceResult> DeleteMealTypeAsync(int mealTypeId)
		{
			try
			{
				_logger.LogInformation("Deleting MealType with ID: {MealTypeId}", mealTypeId);

				var existingMealType = await _adminData.GetMealTypeByIdAsync(mealTypeId);
				if (existingMealType == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "MealType not found."
					};
				}

				// Check dependencies before deleting (since OnDelete is Restrict)
				//bool hasSchedules = await _adminData.HasMealSchedulesAsync(mealTypeId);

				/*if (hasSchedules)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "MealType cannot be deleted because it is referenced in schedules"
					};
				}*/

				await _adminData.DeleteMealTypeAsync(existingMealType);

				_logger.LogInformation("MealType deleted successfully: {MealTypeId}", mealTypeId);

				return new ServiceResult
				{
					Success = true,
					Message = "MealType deleted successfully.",
					ObjectId = mealTypeId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting MealType with ID: {MealTypeId}", mealTypeId);
				return new ServiceResult
				{
					Success = false,
					Message = "Error deleting MealType. Please try again."
				};
			}
		}

		public async Task<AddOnDto> GetAddOnsAsync()
		{
			try
			{
				// Get meal types
				var mealTypes = await _adminData.GetMealTypesAsync();

				// Snacks
				int snackTypeId = mealTypes.FirstOrDefault(mt => mt.MealTypeName == "Snacks")?.MealTypeId ?? 0;
				var snackSubTypes = await _adminData.GetMealSubTypesAsync(snackTypeId);
				var snacks = snackSubTypes.Select(m => new Snacks
				{
					SnackId = m.MealSubTypeId,
					SnackName = m.SubTypeName,
					AddOnType = AddOnType.Snacks
				}).ToList();

				// Beverages
				int beverageTypeId = mealTypes.FirstOrDefault(mt => mt.MealTypeName == "Beverages")?.MealTypeId ?? 0;
				var beverageSubTypes = await _adminData.GetMealSubTypesAsync(beverageTypeId);
				var beverages = beverageSubTypes.Select(m => new Beverages
				{
					BeverageId = m.MealSubTypeId,
					BeverageName = m.SubTypeName,
					AddOnType = AddOnType.Beverages
				}).ToList();

				return new AddOnDto
				{
					Snacks = snacks,
					Beverages = beverages
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while fetching AddOns.");
				throw; 
			}
		}


		public async Task<ServiceResult> CreateMealCostAsync(MealCostDto mealCostDto)
		{
			try
			{
				_logger.LogInformation("Creating new meal cost for Supplier: {SupplierId}, MealType: {MealTypeId}",
									 mealCostDto.SupplierId, mealCostDto.MealTypeId);

				// Validate supplier exists and is active
				var supplier = await _adminData.GetSupplierByIdAsync(mealCostDto.SupplierId);
				if (supplier == null || !supplier.IsActive)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Selected supplier is not valid or inactive."
					};
				}

				// Validate meal type exists and is active
				var mealType = await _adminData.GetMealTypeByIdAsync(mealCostDto.MealTypeId);
				if (mealType == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Selected meal type is not valid."
					};
				}

				// Validate meal sub type if provided
				if (mealCostDto.MealSubTypeId.HasValue)
				{
					var mealSubType = await _adminData.GetMealSubTypesByIdAsync(mealCostDto.MealSubTypeId.Value);
					if (mealSubType == null || mealSubType.MealTypeId != mealCostDto.MealTypeId)
					{
						return new ServiceResult
						{
							Success = false,
							Message = "Selected meal sub type is not valid or doesn't belong to the selected meal type."
						};
					}
				}

				// Check for duplicate meal cost entry
				var existingMealCost = await _adminData.GetMealCostByDetailAsync(
					mealCostDto.SupplierId, mealCostDto.MealTypeId, mealCostDto.MealSubTypeId);
				if (existingMealCost != null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "A meal cost entry already exists for this combination."
					};
				}

				var mealCost = new MealCost
				{
					TenantId = _tenantContext.TenantId.Value,
					SupplierId = mealCostDto.SupplierId,
					MealTypeId = mealCostDto.MealTypeId,
					MealSubTypeId = mealCostDto.MealSubTypeId,
					SupplierCost = mealCostDto.SupplierCost,
					SellingPrice = mealCostDto.SellingPrice,
					CompanyCost = mealCostDto.CompanyCost,
					EmployeeCost = mealCostDto.EmployeeCost,
					Description = mealCostDto.Description,
				};

				await _adminData.AddMealCostAsync(mealCost);

				_logger.LogInformation("Meal cost added successfully: {MealCostId}", mealCost.MealCostId);

				return new ServiceResult
				{
					Success = true,
					Message = "Meal cost created successfully.",
					ObjectId = mealCost.MealCostId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating meal cost");
				return new ServiceResult
				{
					Success = false,
					Message = "Error creating meal cost. Please try again."
				};
			}
		}

		public async Task<ServiceResult> UpdateMealCostAsync(int mealCostId, MealCostDto mealCostDto)
		{
			try
			{
				_logger.LogInformation("Updating meal cost with ID: {MealCostId}", mealCostId);

				var existingMealCost = await _adminData.GetMealCostByIdAsync(mealCostId);
				if (existingMealCost == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Meal cost not found."
					};
				}

				// Validate supplier, meal type, and sub type (same validations as create)
				var supplier = await _adminData.GetSupplierByIdAsync(mealCostDto.SupplierId);
				if (supplier == null || !supplier.IsActive)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Selected supplier is not valid or inactive."
					};
				}

				var mealType = await _adminData.GetMealTypeByIdAsync(mealCostDto.MealTypeId);
				if (mealType == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Selected meal type is not valid or inactive."
					};
				}

				if (mealCostDto.MealSubTypeId.HasValue)
				{
					var mealSubType = await _adminData.GetMealSubTypesByIdAsync(mealCostDto.MealSubTypeId.Value);
					if (mealSubType == null || mealSubType.MealTypeId != mealCostDto.MealTypeId)
					{
						return new ServiceResult
						{
							Success = false,
							Message = "Selected meal sub type is not valid or doesn't belong to the selected meal type."
						};
					}
				}

				// Check for duplicate if key fields are being changed
				if (existingMealCost.SupplierId != mealCostDto.SupplierId ||
					existingMealCost.MealTypeId != mealCostDto.MealTypeId ||
					existingMealCost.MealSubTypeId != mealCostDto.MealSubTypeId)
				{
					var duplicateMealCost = await _adminData.GetMealCostByDetailAsync(
						mealCostDto.SupplierId, mealCostDto.MealTypeId, mealCostDto.MealSubTypeId);
					if (duplicateMealCost != null && duplicateMealCost.MealCostId != mealCostId)
					{
						return new ServiceResult
						{
							Success = false,
							Message = "A meal cost entry already exists for this combination."
						};
					}
				}

				// Update fields
				existingMealCost.SupplierId = mealCostDto.SupplierId;
				existingMealCost.MealTypeId = mealCostDto.MealTypeId;
				existingMealCost.MealSubTypeId = mealCostDto.MealSubTypeId;
				existingMealCost.SupplierCost = mealCostDto.SupplierCost;
				existingMealCost.SellingPrice = mealCostDto.SellingPrice;
				existingMealCost.CompanyCost = mealCostDto.CompanyCost;
				existingMealCost.EmployeeCost = mealCostDto.EmployeeCost;
				existingMealCost.Description = mealCostDto.Description;

				await _adminData.UpdateMealCostAsync(existingMealCost);

				_logger.LogInformation("Meal cost updated successfully: {MealCostId}", mealCostId);

				return new ServiceResult
				{
					Success = true,
					Message = "Meal cost updated successfully.",
					ObjectId = mealCostId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating meal cost with ID: {MealCostId}", mealCostId);
				return new ServiceResult
				{
					Success = false,
					Message = "Error updating meal cost. Please try again."
				};
			}
		}
		public async Task<ServiceResult> DeleteMealCostAsync(int mealCostId)
		{
			try
			{
				_logger.LogInformation("Deleting meal cost with ID: {MealCostId}", mealCostId);

				var existingMealCost = await _adminData.GetMealCostByIdAsync(mealCostId);
				if (existingMealCost == null)
				{
					return new ServiceResult
					{
						Success = false,
						Message = "Meal cost not found."
					};
				}

				await _adminData.DeleteMealCostAsync(existingMealCost);

				_logger.LogInformation("Meal cost deleted successfully: {MealCostId}", mealCostId);

				return new ServiceResult
				{
					Success = true,
					Message = "Meal cost deleted successfully.",
					ObjectId = mealCostId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting meal cost with ID: {MealCostId}", mealCostId);
				return new ServiceResult
				{
					Success = false,
					Message = "Error deleting meal cost. Please try again."
				};
			}
		}
		public async Task<MealCostCreationDetails> GetMealCostCreationDetails()
		{
			try
			{
				// Get suppliers
				var suppliers = await _adminData.GetAllSupplierAsync();
				var supplierDtos = suppliers.Select(s => new SupplierD
				{
					SupplierId = s.SupplierId,
					SupplierName = s.SupplierName
				}).ToList();

				// Get meal types with sub types
				var mealTypes = await _adminData.GetMealTypesAsync();
				var mealTypeList = new List<MealTypeReturn>();

				foreach (var mealType in mealTypes)
				{
					var subTypes = await _adminData.GetMealSubTypesAsync(mealType.MealTypeId);

					mealTypeList.Add(new MealTypeReturn
					{
						MealTypeId = mealType.MealTypeId,
						MealTypeName = mealType.MealTypeName,
						SubTypes = subTypes.Select(st => new SubMealTypeReturn
						{
							MealSubTypeId = st.MealSubTypeId,
							MealSubTypeName = st.SubTypeName
						}).ToList()
					});
				}

				return new MealCostCreationDetails
				{
					Suppliers = supplierDtos,
					MealTypes = mealTypeList
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while fetching meal cost creation details.");
				throw;
			}
		}


		// Fixed GetMealCostsListAsync method
		public async Task<List<MealCostDetails>> GetMealCostsListAsync()
		{
			try
			{
				_logger.LogInformation("Retrieving meal costs list");
				var mealCosts = await _adminData.GetAllMealCostsAsync();

				if (mealCosts == null || !mealCosts.Any())
				{
					_logger.LogInformation("No meal costs found in the database");
					return new List<MealCostDetails>();
				}

				var mealCostDtos = new List<MealCostDetails>();

				foreach (var mc in mealCosts)
				{
					var dto = new MealCostDetails
					{
						MealCostId = mc.MealCostId,
						SupplierId = mc.SupplierId,
						SupplierName = await _adminData.GetSupplierNameAsync(mc.SupplierId) ?? "Unknown",
						MealTypeId = mc.MealTypeId,
						MealTypeName = await _adminData.GetMealTypeNameAsync(mc.MealTypeId) ?? "Unknown",
						MealSubTypeId = mc.MealSubTypeId,
						MealSubTypeName = mc.MealSubTypeId.HasValue
							? await _adminData.GetMealSubTypeNameAsync(mc.MealSubTypeId.Value)
							: null,
						SupplierCost = mc.SupplierCost,
						SellingPrice = mc.SellingPrice,
						CompanyCost = mc.CompanyCost,
						EmployeeCost = mc.EmployeeCost,
						Description = mc.Description,
					};
					mealCostDtos.Add(dto);
				}

				_logger.LogInformation("Retrieved {MealCostCount} meal costs successfully", mealCostDtos.Count);
				return mealCostDtos;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving meal costs list");
				return new List<MealCostDetails>();
			}
		}

		public async Task<ServiceResult> GetMealCostByIdAsync(int mealCostId)
		{
			try
			{
				_logger.LogInformation("Retrieving meal cost by ID: {MealCostId}", mealCostId);
				var mealCost = await _adminData.GetMealCostByIdAsync(mealCostId);

				if (mealCost == null)
				{
					_logger.LogInformation("No meal cost found with ID: {MealCostId}", mealCostId);
					return new ServiceResult
					{
						Success = false,
						Message = "Meal cost not found.",
						Data = null
					};
				}

				var mealCostDto = new MealCostDetails
				{
					MealCostId = mealCost.MealCostId,
					SupplierId = mealCost.SupplierId,
					SupplierName = await _adminData.GetSupplierNameAsync(mealCost.SupplierId) ?? "Unknown",
					MealTypeId = mealCost.MealTypeId,
					MealTypeName = await _adminData.GetMealTypeNameAsync(mealCost.MealTypeId) ?? "Unknown",
					MealSubTypeId = mealCost.MealSubTypeId,
					MealSubTypeName = mealCost.MealSubTypeId.HasValue
						? await _adminData.GetMealSubTypeNameAsync(mealCost.MealSubTypeId.Value)
						: null,
					SupplierCost = mealCost.SupplierCost,
					SellingPrice = mealCost.SellingPrice,
					CompanyCost = mealCost.CompanyCost,
					EmployeeCost = mealCost.EmployeeCost,
					Description = mealCost.Description,
				};

				_logger.LogInformation("Retrieved meal cost successfully: {MealCostId}", mealCostId);

				return new ServiceResult
				{
					Success = true,
					Message = "Meal cost retrieved successfully.",
					Data = mealCostDto
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving meal cost with ID: {MealCostId}", mealCostId);
				return new ServiceResult
				{
					Success = false,
					Message = "Error retrieving meal cost. Please try again.",
					Data = null
				};
			}
		}
	}
}
