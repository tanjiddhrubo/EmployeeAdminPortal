namespace EmployeeAdminPortal.API.Models.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public required decimal Salary { get; set; }

        // ==========================================================
        // NEW PROPERTIES FOR RELATIONSHIPS
        // ==========================================================

        // 1. Department Relationship (Required Foreign Key)
        public required Guid DepartmentId { get; set; }
        public required Department Department { get; set; }

        // 2. Designation Relationship (Required Foreign Key)
        public required Guid DesignationId { get; set; }
        public required Designation Designation { get; set; }
    }
}