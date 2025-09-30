using Dapper;
using EmployeeAdminPortal.API.Models.Entities;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// NOTE: Ensure Microsoft.EntityFrameworkCore is installed in the project, though not explicitly used in usings.

namespace EmployeeAdminPortal.Repositories.Implementations
{
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

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            const string storedProcedureName = "dbo.usp_GetAllEmployees";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var employees = await connection.QueryAsync<Employee, Department, Designation, Employee>(
                    storedProcedureName,
                    (employee, department, designation) =>
                    {
                        employee.Department = department;
                        employee.Designation = designation;
                        return employee;
                    },
                    // FIX 1: Correct Dapper splitOn to map nested objects.
                    splitOn: "DepartmentId,DesignationId",
                    // Using System.Data prefix since the namespace was removed to fix conflicts.
                    commandType: System.Data.CommandType.StoredProcedure
                );

                return employees;
            }
        }

        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            var sql = @"
                     SELECT 
                         E.Id, E.Name AS FullName, E.Email, E.Phone, E.Salary, E.DepartmentId, E.DesignationId,
                         D.Id AS DepartmentIdSplit, D.Name AS DepartmentName, 
                         G.Id AS DesignationIdSplit, G.Name AS DesignationName
                     FROM dbo.Employees E
                     INNER JOIN dbo.Departments D ON E.DepartmentId = D.Id
                     -- FIX 2: Removed redundant 'JOIN' keyword
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
                    // FIX 3: splitOn matches the explicit aliases in the query
                    splitOn: "DepartmentIdSplit, DesignationIdSplit"
                );

                return employee.FirstOrDefault();
            }
        }

        // CUD methods remain on EF Core:

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

            trackedEmployee.Name = employee.Name;
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
    }
}