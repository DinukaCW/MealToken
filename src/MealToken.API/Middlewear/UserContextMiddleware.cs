using MealToken.Application.Interfaces;
using MealToken.Application.Services;
using MealToken.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MealToken.API.Middlewear
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IUserContext userContext, MealTokenDbContext dbContext)
        {
            var user = httpContext.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("UserID");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    var departmentIds = await dbContext.UserDepartment
                        .Where(ud => ud.UserId == userId)
                        .Select(ud => ud.DepartmentId)
                        .ToListAsync();

                    if (userContext is UserContext concreteUserContext)
                    {
                        concreteUserContext.SetUserContext(userId, departmentIds);
                    }
                }
            }
            await _next(httpContext);
        }
    }
}
