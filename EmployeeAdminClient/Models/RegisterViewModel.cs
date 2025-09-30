// File: EmployeeAdminClient/Models/RegisterViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminClient.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        // NOTE: Must meet API's Identity complexity requirements (e.g., 6+ chars, uppercase, number)
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // IMPORTANT: We hardcode the role to "User" to prevent front-end users 
        // from creating new "Admin" accounts by default.
        public List<string> Roles { get; set; } = new List<string> { "User" };
    }
}