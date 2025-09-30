using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        // FIX: Removed = string.Empty; and added 'required'
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        // FIX: Removed = string.Empty; and added 'required'
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        // FIX: Removed = string.Empty; and added 'required'
        public required string Password { get; set; }
    }
}