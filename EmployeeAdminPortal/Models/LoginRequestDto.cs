// File: Models/DTOs/LoginRequestDto.cs

using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
          [DataType(DataType.EmailAddress)]
        public required string Username { get; set; } // Correct

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; } // Correct
    }
}