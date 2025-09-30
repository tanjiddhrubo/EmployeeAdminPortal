// File: EmployeeAdminClient/Models/EmployeeViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminClient.Models
{
    public class EmployeeViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty; // Based on your DB table

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0, 500000, ErrorMessage = "Salary must be a positive amount.")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public Guid DepartmentId { get; set; }

        [Required(ErrorMessage = "Designation is required.")]
        public Guid DesignationId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
    }
}