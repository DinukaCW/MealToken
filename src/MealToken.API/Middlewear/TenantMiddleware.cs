using MealToken.Application.Interfaces;
using MealToken.Application.Services;

namespace MealToken.API.Middlewear
{
	public class TenantMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<TenantMiddleware> _logger;
		private readonly IConfiguration _configuration;

		public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger, IConfiguration configuration)
		{
			_next = next;
			_logger = logger;
			_configuration = configuration;
		}

        public async Task InvokeAsync(HttpContext context)
        {
            var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
            var tenantContext = context.RequestServices.GetRequiredService<TenantContext>();

            var host = context.Request.Host.Value;
            var subdomain = ExtractSubdomain(host);

            // Handle known hosts (localhost or Azure App Service URL)
            if (string.IsNullOrEmpty(subdomain) ||
                host.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                host.Contains("azurewebsites.net", StringComparison.OrdinalIgnoreCase)||
				host.Contains("10.30.1.1", StringComparison.OrdinalIgnoreCase))
			{
                _logger.LogInformation($"No subdomain found or running on a known host ({host}). Using default tenant.");

                var defaultTenantKey = _configuration["DefaultTenant"] ?? "default";
                var tenant = await tenantService.GetTenantBySubdomainAsync(defaultTenantKey);

                if (tenant != null)
                {
                    tenantContext.SetTenant(tenant);
                }
                else
                {
                    _logger.LogWarning("Default tenant not found in database");
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid tenant");
                    return;
                }
            }
            else
            {
                var tenant = await tenantService.GetTenantBySubdomainAsync(subdomain);

                if (tenant == null)
                {
                    _logger.LogWarning($"Tenant not found for subdomain: {subdomain}");
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Tenant not found");
                    return;
                }

                tenantContext.SetTenant(tenant);
            }

            await _next(context);
        }

        private string? ExtractSubdomain(string host)
        {
            var hostWithoutPort = host.Split(':')[0];
            var parts = hostWithoutPort.Split('.');
            return parts.Length > 2 ? parts[0] : null;
        }
    }
}
