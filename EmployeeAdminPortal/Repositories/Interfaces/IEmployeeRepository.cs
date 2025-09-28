using EmployeeAdminPortal.API.Models.Entities;

namespace EmployeeAdminPortal.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(Guid id);
        Task<Employee> AddAsync(Employee employee);
        Task<Employee?> UpdateAsync(Guid id, Employee employee);
        Task<Employee?> DeleteAsync(Guid id);
    }
}