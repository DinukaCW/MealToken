using Authentication.Extensions;
using Authentication.Interfaces;
using Authentication.Services;
using MealToken.API.Helpers;
using MealToken.API.Middlewear;
using MealToken.Application.Interfaces;
using MealToken.Application.Services;
using MealToken.Domain.Interfaces;
using MealToken.Infrastructure.Persistence;
using MealToken.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
/*
using System.Security.Cryptography;

// Specify the desired key length in bytes (e.g., 32 bytes for a 256-bit key)
const int keyLengthInBytes = 64;

// Generate a cryptographically strong random byte array
byte[] keyBytes = RandomNumberGenerator.GetBytes(keyLengthInBytes);

// Convert the byte array to a Base64 string for easy storage
string securedKey = Convert.ToBase64String(keyBytes);

// securedKey will be a long, random, non-readable string suitable for your Jwt:Key setting
Console.WriteLine(securedKey);

var (key, iv) = EncryptionKeyGenerator.GenerateAesKeyAndIv();
Console.WriteLine($"Key: {key}");
Console.WriteLine($"IV: {iv}");*/
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); 
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>());
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<TenantDbContextFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEntityCreationService, EntityCreationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IBusinessService,BusinessService>();
builder.Services.AddScoped<ScheduleDateGeneratorService>();
builder.Services.AddScoped<ITokenProcessService, TokenProcessService>();
builder.Services.AddScoped<ICompanyBusinessLogic, CompanyBusinessLogic>();
builder.Services.AddScoped<IMealReportService, MealReportService>();
builder.Services.AddScoped<IReportRepository,ReportRepository>();
builder.Services.AddScoped<IUserHistoryService,UserHistoryService>();
builder.Services.AddScoped<UserHistoryActionFilter>();


builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<PlatformDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<MealTokenDbContext>((serviceProvider, options) =>
{
	var tenantContext = serviceProvider.GetService<ITenantContext>();
	var connectionString = tenantContext?.CurrentTenant?.ConnectionString
		?? builder.Configuration.GetConnectionString("DefaultConnection");
	options.UseSqlServer(connectionString);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new() { Title = "MealToken API", Version = "v1" });
});

// 🔹 Register your class library services
builder.Services.AddAuthenticationLibrary(builder.Configuration);

var jwtKey = builder.Configuration["JwtSettings:Key"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtIssuer,
		ValidAudience = jwtAudience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
	};
});

// 🔹 Add role-based policies (optional)
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("DepartmentHeadOnly", policy => policy.RequireRole("DepartmentHead"));
	options.AddPolicy("SchedulerOnly", policy => policy.RequireRole("Sheduler"));
	options.AddPolicy("RequesterOnly", policy => policy.RequireRole("Requester"));
});

builder.Services.AddRateLimiter(options =>
{
	options.AddPolicy("LoginLimit", httpContext =>
		RateLimitPartition.GetTokenBucketLimiter(
			partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
			_ => new TokenBucketRateLimiterOptions
			{
				TokenLimit = 5, // Max 5 attempts
				TokensPerPeriod = 5, // Reset to 5 tokens each period
				ReplenishmentPeriod = TimeSpan.FromMinutes(1), // 1-minute reset
				AutoReplenishment = true,
				QueueLimit = 0
			}));
});

// Add CORS policy
/*builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("http://10.30.1.1:8082", "http://localhost:8082") // Your frontend URL
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials(); // If you need cookies/auth
	});
});
*/
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "MealToken API v1");
		c.RoutePrefix = string.Empty; 
	});
}

app.UseHttpsRedirection();
//app.UseCors("AllowFrontend");
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<UserContextMiddleware>();

app.MapControllers();

app.Run();
