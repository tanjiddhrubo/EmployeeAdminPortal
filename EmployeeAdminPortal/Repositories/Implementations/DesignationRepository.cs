// File: Repositories/Implementations/DesignationRepository.cs

using Dapper; // Required for Dapper methods
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.API.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Required for connection string
using System.Data.SqlClient; // Required for SqlConnection

namespace EmployeeAdminPortal.Repositories.Implementations
{
    public class DesignationRepository : IDesignationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public DesignationRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // ?? REFACTORED: Use Dapper for GetAllAsync ??
        public async Task<IEnumerable<Designation>> GetAllAsync()
        {
            var sql = "SELECT * FROM dbo.Designations";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<Designation>(sql);
            }
        }

        // ?? REFACTORED: Use Dapper for GetByIdAsync ??
        public async Task<Designation?> GetByIdAsync(Guid id)
        {
            var sql = "SELECT * FROM dbo.Designations WHERE Id = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<Designation>(sql, new { Id = id });
            }
        }

        // The following CUD methods remain on EF Core:
        public async Task<Designation> AddAsync(Designation designation)
        {
            await _context.Designations.AddAsync(designation);
            await _context.SaveChangesAsync();
            return designation;
        }

        public async Task<Designation?> UpdateAsync(Guid id, Designation designation)
        {
            var existingDesignation = await _context.Designations.FindAsync(id);

            if (existingDesignation == null) return null;

            existingDesignation.Name = designation.Name;

            await _context.SaveChangesAsync();
            return existingDesignation;
        }

        public async Task<Designation?> DeleteAsync(Guid id)
        {
            var designationToDelete = await _context.Designations.FindAsync(id);

            if (designationToDelete == null) return null;

            _context.Designations.Remove(designationToDelete);
            await _context.SaveChangesAsync();
            return designationToDelete;
        }
    }
}