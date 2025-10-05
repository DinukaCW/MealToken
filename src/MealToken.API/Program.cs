using Authentication.Extensions;
using Authentication.Interfaces;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); 
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>());
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
var tempProvider = builder.Services.BuildServiceProvider();
var encryptionService = tempProvider.GetRequiredService<IEncryptionService>();

var encryptedJwtKey = builder.Configuration["JwtSettings:Key"];
string jwtKey = encryptionService.DecryptData(encryptedJwtKey);
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
	options.AddPolicy("ShedulerOnly", policy => policy.RequireRole("Sheduler"));
	options.AddPolicy("RequesterOnly", policy => policy.RequireRole("Requester"));
});
// Add CORS policy
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("http://207.180.217.101:85" , "https://localhost:3000") // Your frontend URL
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials(); // If you need cookies/auth
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
app.UseCors("AllowFrontend");
app.UseMiddleware<TenantMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
