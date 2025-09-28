// File: Repositories/Interfaces/IDepartmentRepository.cs

using EmployeeAdminPortal.API.Models.Entities;

namespace EmployeeAdminPortal.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(Guid id);
        Task<Department> AddAsync(Department department);
        Task<Department?> UpdateAsync(Guid id, Department department);
        Task<Department?> DeleteAsync(Guid id);
    }
}