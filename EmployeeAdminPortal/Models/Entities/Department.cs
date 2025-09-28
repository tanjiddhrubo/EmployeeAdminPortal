namespace EmployeeAdminPortal.API.Models.Entities
{
    public class Department
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}