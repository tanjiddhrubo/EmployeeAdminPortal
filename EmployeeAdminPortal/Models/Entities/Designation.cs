namespace EmployeeAdminPortal.API.Models.Entities
{
    public class Designation
    {
        // Primary Key
        public Guid Id { get; set; }

        public required string Name { get; set; }

        // Navigation Collection (The 'One' side to 'Many' Employees)
        // Initialize the collection to prevent NullReferenceExceptions when adding employees
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}