// File: Models/DTOs/LoginRequestDto.cs

using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        // FIX: Removed = string.Empty; and added 'required'
        public required string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        // FIX: Removed = string.Empty; and added 'required'
        public required string Password { get; set; }
    }
}