using Authentication.Models.Entities;
using MealToken.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Services
{
    public class UserContext : IUserContext
    {
        public int? UserId { get; private set; }
        public List<int>? DepartmentIds { get; private set; }

        public void SetUserContext(int userId, List<int> departmentIds)
        {
            UserId = userId;
            DepartmentIds = departmentIds;
        }
    }
}
