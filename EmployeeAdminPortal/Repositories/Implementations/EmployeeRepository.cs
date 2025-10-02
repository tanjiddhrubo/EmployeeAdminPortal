using Dapper;
using EmployeeAdminPortal.API.Models.Entities;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic; // Added for IEnumerable
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeAdminPortal.Repositories.Implementations
{
    // The previous compilation errors (Dapper, SqlConnection not found) were resolved by fixing file encoding and rebuilding.
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public EmployeeRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                 ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // =========================================================================
        // Use stored procedure on SQL Server; fallback to raw SQL for SQLite.
        // =========================================================================
        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            if (_context.Database.IsSqlServer())
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Stored procedure returns columns including DepartmentId and DesignationId
                    var employees = await connection.QueryAsync<Employee, Department, Designation, Employee>(
                        "Proc_GetAllEmployees",
                        (employee, department, designation) =>
                        {
                            employee.Department = department;
                            employee.Designation = designation;
                            return employee;
                        },
                        commandType: CommandType.StoredProcedure,
                        // Use the department/designation id columns as split markers (these exist in the proc result)
                        splitOn: "DepartmentId,DesignationId"
                    );

                    return employees.ToList();
                }
            }

            // SQLite or other providers: use inline SQL
            var sql = @"
                SELECT E.*, D.Id, D.Name, G.Id, G.Name
                FROM Employees E
                INNER JOIN Departments D ON E.DepartmentId = D.Id
                INNER JOIN Designations G ON E.DesignationId = G.Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var employees = await connection.QueryAsync<Employee, Department, Designation, Employee>(
                    sql,
                    (employee, department, designation) =>
                    {
                        employee.Department = department;
                        employee.Designation = designation;
                        return employee;
                    },
                    splitOn: "Id, Id"
                );

                return employees.ToList();
            }
        }

        // =========================================================================
        // Get by id: try stored procedure Proc_GetEmployeeById when using SQL Server
        // =========================================================================
        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            if (_context.Database.IsSqlServer())
            {
                var proc = "Proc_GetEmployeeById";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var employee = await connection.QueryAsync<Employee, Department, Designation, Employee>(
                        proc,
                        (employee, department, designation) =>
                        {
                            employee.Department = department;
                            employee.Designation = designation;
                            return employee;
                        },
                        param: new { EmployeeId = id },
                        commandType: CommandType.StoredProcedure,
                        // Use DepartmentId/DesignationId as split markers
                        splitOn: "DepartmentId,DesignationId"
                    );

                    return employee.FirstOrDefault();
                }
            }

            // Fallback raw SQL
            var sql = @"
                     SELECT 
                         E.*, 
                         D.Id, D.Name, 
                         G.Id, G.Name
                     FROM dbo.Employees E
                     INNER JOIN dbo.Departments D ON E.DepartmentId = D.Id
                     INNER JOIN dbo.Designations G ON E.DesignationId = G.Id
                     WHERE E.Id = @EmployeeId";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var employee = await connection.QueryAsync<Employee, Department, Designation, Employee>(
                    sql,
                    (employee, department, designation) =>
                    {
                        employee.Department = department;
                        employee.Designation = designation;
                        return employee;
                    },
                    param: new { EmployeeId = id },
                    splitOn: "Id, Id"
                );

                return employee.FirstOrDefault();
            }
        }

        // =========================================================================
        // EF Core Methods - These were already correct.
        // =========================================================================
        public async Task<Employee> AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee?> UpdateAsync(Guid id, Employee employee)
        {
            var trackedEmployee = await _context.Employees.FindAsync(id);

            if (trackedEmployee == null)
            {
                return null;
            }

            trackedEmployee.Name = employee.Name; // Assuming 'Name' in original code was 'FullName'
            trackedEmployee.Email = employee.Email;
            trackedEmployee.Phone = employee.Phone;
            trackedEmployee.Salary = employee.Salary;
            trackedEmployee.DepartmentId = employee.DepartmentId;
            trackedEmployee.DesignationId = employee.DesignationId;

            await _context.SaveChangesAsync();
            return trackedEmployee;
        }

        public async Task<Employee?> DeleteAsync(Guid id)
        {
            var employeeToDelete = await _context.Employees.FindAsync(id);

            if (employeeToDelete == null)
            {
                return null;
            }

            _context.Employees.Remove(employeeToDelete);
            await _context.SaveChangesAsync();
            return employeeToDelete;
        }

        // =========================================================================
        // FIX: Missing Lookup Methods
        // These methods are needed for the 'Add New Employee' form lookups (API Status 500 error on lookups).
        // Using Dapper here for consistency with your existing Dapper usage.
        // =========================================================================
        public async Task<IEnumerable<Department>> GetDepartmentsAsync()
        {
            var sql = "SELECT * FROM Departments";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<Department>(sql);
            }
        }

        public async Task<IEnumerable<Designation>> GetDesignationsAsync()
        {
            var sql = "SELECT * FROM Designations";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<Designation>(sql);
            }
        }
    }
}