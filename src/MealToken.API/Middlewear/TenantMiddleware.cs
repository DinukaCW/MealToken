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

			var subdomain = ExtractSubdomain(context.Request.Host.Value);

			if (!string.IsNullOrEmpty(subdomain))
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
			else
			{
				// Instead of rejecting, use default tenant for localhost
				_logger.LogInformation("No subdomain found. Using default tenant (localhost).");

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

			await _next(context);
		}

		private string? ExtractSubdomain(string host)
		{
			var hostWithoutPort = host.Split(':')[0];

			// Handle localhost explicitly
			if (hostWithoutPort.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
				hostWithoutPort.Equals("127.0.0.1") ||
				hostWithoutPort.Contains("azurewebsites.net", StringComparison.OrdinalIgnoreCase))
			{
				return null; // Force middleware to fallback to default tenant
			}

			var parts = hostWithoutPort.Split('.');

			return parts.Length > 2 ? parts[0] : null;
		}
	}
}
