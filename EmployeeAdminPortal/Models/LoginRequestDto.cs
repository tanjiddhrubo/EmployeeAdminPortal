// File: Models/DTOs/LoginRequestDto.cs

using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}