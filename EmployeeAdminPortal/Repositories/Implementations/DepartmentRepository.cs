// File: Repositories/Implementations/DepartmentRepository.cs

using Dapper; // Required for Dapper methods
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.API.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Required for connection string
using System.Data.SqlClient; // Required for SqlConnection

namespace EmployeeAdminPortal.Repositories.Implementations
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public DepartmentRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // ?? REFACTORED: Use Dapper for GetAllAsync ??
        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            var sql = "SELECT * FROM dbo.Departments";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<Department>(sql);
            }
        }

        // ?? REFACTORED: Use Dapper for GetByIdAsync ??
        public async Task<Department?> GetByIdAsync(Guid id)
        {
            var sql = "SELECT * FROM dbo.Departments WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<Department>(sql, new { Id = id });
            }
        }

        // The following CUD methods remain on EF Core for reliable change tracking:
        public async Task<Department> AddAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task<Department?> UpdateAsync(Guid id, Department department)
        {
            var existingDepartment = await _context.Departments.FindAsync(id);

            if (existingDepartment == null) return null;

            existingDepartment.Name = department.Name;

            await _context.SaveChangesAsync();
            return existingDepartment;
        }

        public async Task<Department?> DeleteAsync(Guid id)
        {
            var departmentToDelete = await _context.Departments.FindAsync(id);

            if (departmentToDelete == null) return null;

            _context.Departments.Remove(departmentToDelete);
            await _context.SaveChangesAsync();
            return departmentToDelete;
        }
    }
}