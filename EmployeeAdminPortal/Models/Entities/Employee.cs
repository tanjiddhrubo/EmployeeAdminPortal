namespace EmployeeAdminPortal.API.Models.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public required decimal Salary { get; set; }
        public required Guid DepartmentId { get; set; }
        public required Department Department { get; set; }
        public required Guid DesignationId { get; set; }
        public required Designation Designation { get; set; }
    }
}