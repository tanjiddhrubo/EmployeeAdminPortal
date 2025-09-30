using EmployeeAdminPortal.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Repositories.Implementations;
using System.Text;
using AutoMapper;
using EmployeeAdminPortal; // Needed for RoleInitializer and DataSeeder

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------
// ⭐️ START CORS CONFIGURATION (Services) ⭐️
// -----------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // CRUCIAL: Allow the Client MVC App's origin (port 7277) to connect.
        policy.WithOrigins("https://localhost:7277")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// -----------------------------------------------------------
// ⭐️ END CORS CONFIGURATION (Services) ⭐️
// -----------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration.");
var jwtIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration.");
var jwtAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not found in configuration.");


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

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


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
});


builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var app = builder.Build();

// -----------------------------------------------------------
// ⭐️ START SEEDING LOGIC: Roles, Departments, and Designations
// -----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        // 1. Seed Roles (existing code)
        RoleInitializer.SeedRolesAsync(serviceProvider).Wait();

        // 2. ⭐️ NEW: Seed Department and Designation Data ⭐️
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        DataSeeder.SeedLookupData(context).Wait();
    }
    catch (Exception ex)
    {
        // Optional: Log the error if seeding fails
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles or lookup data.");
    }
}
// -----------------------------------------------------------
// ⭐️ END SEEDING LOGIC
// -----------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseHttpsRedirection();

// ⭐️ CRITICAL: Add UseCors() here, before UseAuthentication() ⭐️
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();