
namespace EmployeeAdminPortal.Models
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal Salary { get; set; }

        
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
    }
}