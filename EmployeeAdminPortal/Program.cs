using EmployeeAdminPortal;
using EmployeeAdminPortal.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Repositories.Implementations;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS first
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7277")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add HttpClient factory for external integrations
builder.Services.AddHttpClient();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Admin Portal API",
        Version = "v1",
        Description = "API for Employee Administration Portal"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if they exist
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add DbContext: prefer using DefaultConnection (LocalDB) in Development to match migrations and design-time factory
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Environment.IsDevelopment())
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        // Use SQL Server LocalDB in development if configured to match design-time factory and existing migrations
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
    }
    else
    {
        // Fallback to SQLite if no DefaultConnection is provided
        var dbPath = Path.Combine(builder.Environment.ContentRootPath, "employeeadmin-dev.db");
        var sqliteConn = $"Data Source={dbPath}";
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(sqliteConn));
        builder.Logging.AddConsole();
    }
}
else
{
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("DefaultConnection not set.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));
}

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
    jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration.")));

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = key
    };
});

// Add services
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var app = builder.Build();

// Enable Swagger UI for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Admin API V1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Employee Admin API Documentation";
    c.DefaultModelsExpandDepth(2);
    c.DefaultModelExpandDepth(2);
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.EnableDeepLinking();
    c.DisplayRequestDuration();
});

// Seed data and apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        try
        {
            // Apply any pending migrations
            await context.Database.MigrateAsync();
            logger.LogInformation("Applied database migrations.");
        }
        catch (Exception mex)
        {
            // If migrations fail, try EnsureCreated as a fallback
            logger.LogWarning(mex, "Could not apply migrations automatically. Attempting EnsureCreated fallback.");
            try
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Ensured database was created (EnsureCreated).");
            }
            catch (Exception ex2)
            {
                logger.LogError(ex2, "Failed to ensure database created. This may cause runtime errors for Identity.");
            }
        }

        // After migrations, verify Identity tables exist; if not, attempt EnsureCreated
        try
        {
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='AspNetUsers';";
            var res = await cmd.ExecuteScalarAsync();
            if (res == null)
            {
                logger.LogWarning("AspNetUsers table not found after migrations. Calling EnsureCreatedAsync as fallback.");
                try
                {
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("Database created via EnsureCreatedAsync fallback.");
                }
                catch (Exception ex3)
                {
                    logger.LogError(ex3, "EnsureCreated fallback failed.");
                }
            }
            await conn.CloseAsync();
        }
        catch (Exception checkEx)
        {
            logger.LogWarning(checkEx, "Could not verify database tables; continuing.");
        }

        try
        {
            await RoleInitializer.SeedRolesAsync(services);
        }
        catch (Exception rex)
        {
            logger.LogWarning(rex, "Could not seed roles; continuing.");
        }

        try
        {
            await DataSeeder.SeedLookupData(context);
        }
        catch (Exception sex)
        {
            logger.LogWarning(sex, "Could not seed lookup data; continuing.");
        }

        // Create stored procedure(s), audit table and trigger if using SQL Server
        try
        {
            if (context.Database.IsSqlServer())
            {
                var procSql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Proc_GetAllEmployees')
BEGIN
    EXEC('CREATE PROCEDURE Proc_GetAllEmployees AS BEGIN SELECT E.Id, E.Name, E.Email, E.Phone, E.Salary, E.DepartmentId, E.DesignationId, D.Name AS DepartmentName, G.Name AS DesignationName FROM Employees E INNER JOIN Departments D ON E.DepartmentId = D.Id INNER JOIN Designations G ON E.DesignationId = G.Id END')
END";
                await context.Database.ExecuteSqlRawAsync(procSql);

                var auditSql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmployeeAudit')
BEGIN
    CREATE TABLE EmployeeAudit (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        ActionDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END";
                await context.Database.ExecuteSqlRawAsync(auditSql);

                var triggerSql = @"
IF OBJECT_ID('trg_Employee_Insert', 'TR') IS NULL
BEGIN
    EXEC('CREATE TRIGGER trg_Employee_Insert ON Employees AFTER INSERT AS BEGIN SET NOCOUNT ON; INSERT INTO EmployeeAudit (Id, EmployeeId, Action) SELECT NEWID(), Id, ''INSERT'' FROM inserted; END')
END";
                await context.Database.ExecuteSqlRawAsync(triggerSql);

                var funcSql = @"
IF OBJECT_ID('ufn_GetEmployeeSummary', 'FN') IS NULL
BEGIN
    EXEC('CREATE FUNCTION ufn_GetEmployeeSummary() RETURNS TABLE AS RETURN (SELECT DepartmentId, COUNT(*) AS EmployeeCount, SUM(Salary) AS TotalSalary FROM Employees GROUP BY DepartmentId)')
END";
                await context.Database.ExecuteSqlRawAsync(funcSql);

                logger.LogInformation("Ensured stored procedures, audit table, trigger and function exist.");
            }
        }
        catch (Exception scEx)
        {
            logger.LogWarning(scEx, "Failed to create stored procs/trigger/function; continuing.");
        }

        // Seed an admin user if one does not exist
        try
        {
            var config = services.GetRequiredService<IConfiguration>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var adminEmail = config["Seed:AdminEmail"] ?? "admin@test.com";
            var adminPassword = config["Seed:AdminPassword"] ?? "P@ssword123!";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail) ?? await userManager.FindByNameAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Seeded default admin user: {Email}", adminEmail);
                }
                else
                {
                    logger.LogWarning("Failed to create admin user: {Errors}", string.Join(';', createResult.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception seedEx)
        {
            logger.LogWarning(seedEx, "Failed to seed admin user; continuing.");
        }

    }
    catch (Exception ex)
    {
        var logger2 = services.GetRequiredService<ILogger<Program>>();
        logger2.LogError(ex, "An error occurred while preparing the database.");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Critical middleware ordering
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers()
   .RequireCors(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.Run();