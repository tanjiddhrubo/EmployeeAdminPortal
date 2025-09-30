// File: EmployeeAdminClient/Models/LoginViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminClient.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username (Email) is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        // We use Username here to align with the API's Register/Login DTO, 
        // which typically uses email as the username.
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}