using MealToken.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Interfaces
{
    public interface ITokenProcessService
    {
        Task<MealTokenResponse> GetMealDistributionAsync(MealTokenRequest request);
    }
}
