using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EmployeeAdminPortal.Data; // Ensure this matches your namespace/path

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // **CRITICAL: Provide the connection string directly here.**
        // This is the string the EF Core tools will use.
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Employeeinfo;Trusted_connection=true;TrustServerCertificate=True;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}