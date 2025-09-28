// File: RoleInitializer.cs

using Microsoft.AspNetCore.Identity;

namespace EmployeeAdminPortal
{
    public static class RoleInitializer
    {
        private static readonly List<string> Roles = new List<string> { "Admin", "User" };

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            // Get the RoleManager service
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in Roles)
            {
                // Check if the role already exists
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // Create the role if it doesn't exist
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}