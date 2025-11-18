using Authentication.Models.DTOs;
using MealToken.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Interfaces
{
   public interface ICompanyBusinessLogic
    {
        Task<ServiceResult> HemasCompanyLogic(MealDeviceRequest mealDeviceRequest);
		Task<ServiceResult> ManualPrintTokenLostAsync(int personId, DateTime reqDateTime);
		Task<ServiceResult> ManualPrintTokenOtherAsync(ManualPrintRequest printRequest);

	}
}
