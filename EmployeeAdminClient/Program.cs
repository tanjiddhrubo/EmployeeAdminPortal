using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ?? ADDED: Configure session services ??
builder.Services.AddSession(options =>
{
    // Set a timeout for the session token
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    // Prevents client-side script access to the session cookie
    options.Cookie.HttpOnly = true;
    // Makes the session cookie essential for the app to function
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ?? ADDED: Use session middleware ??
app.UseSession();

app.UseRouting();

// NOTE: We don't need app.UseAuthentication() here, as this is a client app,
// but we keep app.UseAuthorization() for MVC controller-level authorization attributes.

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    // ?? CHANGE THE DEFAULT STARTING ROUTE ??
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();