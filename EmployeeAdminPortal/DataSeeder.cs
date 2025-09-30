using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.API.Models.Entities; // ⭐️ Correct namespace for your Entity Models ⭐️

namespace EmployeeAdminPortal
{
    public static class DataSeeder
    {
        public static async Task SeedLookupData(ApplicationDbContext context)
        {
            // Seed Departments
            if (!context.Departments.Any())
            {
                var departments = new List<Department>
                {
                    new Department { Name = "Engineering" },
                    new Department { Name = "Human Resources (HR)" },
                    new Department { Name = "Marketing" },
                    new Department { Name = "Sales" },
                    new Department { Name = "Finance" }
                };
                // Assuming 'context.Departments' is the correct DbSet<Department> property
                context.Departments.AddRange(departments);
            }

            // Seed Designations
            if (!context.Designations.Any())
            {
                var designations = new List<Designation>
                {
                    new Designation { Name = "Software Engineer" },
                    new Designation { Name = "Project Manager" },
                    new Designation { Name = "HR Specialist" },
                    new Designation { Name = "Sales Representative" },
                    new Designation { Name = "Financial Analyst" }
                };
                // Assuming 'context.Designations' is the correct DbSet<Designation> property
                context.Designations.AddRange(designations);
            }

            // Save all changes to the database
            await context.SaveChangesAsync();
        }
    }
}