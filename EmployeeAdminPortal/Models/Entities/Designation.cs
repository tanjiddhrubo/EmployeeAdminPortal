namespace EmployeeAdminPortal.API.Models.Entities
{
    public class Designation
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}