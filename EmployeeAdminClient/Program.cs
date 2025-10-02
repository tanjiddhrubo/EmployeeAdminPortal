using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(apm =>
    {
        // Remove any ApplicationPart that comes from the API project assembly to avoid registering API controllers in the client app
        var partsToRemove = apm.ApplicationParts
            .Where(p => p.Name != null && p.Name.Contains("EmployeeAdminPortal", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var p in partsToRemove)
        {
            apm.ApplicationParts.Remove(p);
        }
    });

// Add distributed cache required for session
builder.Services.AddDistributedMemoryCache();

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Explicit root redirect to the client login page to avoid accidental API routing
app.MapGet("/", context =>
{
    context.Response.Redirect("/Auth/Login");
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();