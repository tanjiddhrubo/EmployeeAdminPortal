// File: Repositories/Interfaces/IDesignationRepository.cs

using EmployeeAdminPortal.API.Models.Entities;

namespace EmployeeAdminPortal.Repositories.Interfaces
{
    public interface IDesignationRepository
    {
        Task<IEnumerable<Designation>> GetAllAsync();
        Task<Designation?> GetByIdAsync(Guid id);
        Task<Designation> AddAsync(Designation designation);
        Task<Designation?> UpdateAsync(Guid id, Designation designation);
        Task<Designation?> DeleteAsync(Guid id);
    }
}