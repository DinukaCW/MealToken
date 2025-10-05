using MealToken.Application.Interfaces;
using MealToken.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Application.Services
{
	public class TenantService : ITenantService
	{
		private readonly IConfiguration _configuration;
		private readonly string _masterConnectionString;

		public TenantService(IConfiguration configuration)
		{
			_configuration = configuration;
			_masterConnectionString = _configuration.GetConnectionString("DefaultConnection")!;
		}

		public async Task<TenantInfo?> GetTenantBySubdomainAsync(string subdomain)
		{
			using var connection = new SqlConnection(_masterConnectionString);
			await connection.OpenAsync();

			var query = @"
                SELECT Id, Name, Subdomain, SchemaName, ConnectionString, IsActive, CreatedAt
                FROM dbo.TenantInfo 
                WHERE Subdomain = @Subdomain AND IsActive = 1";

			return await connection.QueryFirstOrDefaultAsync<TenantInfo>(query, new { Subdomain = subdomain });
			
		}

		public async Task<TenantInfo?> GetTenantByIdAsync(string tenantId)
		{
			using var connection = new SqlConnection(_masterConnectionString);
			await connection.OpenAsync();

			var query = @"
                SELECT Id, Name, Subdomain, SchemaName, ConnectionString, IsActive, CreatedAt
                FROM dbo.TenantInfo 
                WHERE TenantId = @TenantId AND IsActive = 1";

			return await connection.QueryFirstOrDefaultAsync<TenantInfo>(query, new { TenantId = tenantId });
		}

		public async Task<bool> TenantExistsAsync(string subdomain)
		{
			var tenant = await GetTenantBySubdomainAsync(subdomain);
			return tenant != null;
		}
	}
}
