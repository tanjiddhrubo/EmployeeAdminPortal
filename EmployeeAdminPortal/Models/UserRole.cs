namespace EmployeeAdminPortal.Models
{
    public class UserRole
    {
        // FIX: Removed = string.Empty; and added 'required'
        public required string Username { get; set; }

        // FIX: Removed = string.Empty; and added 'required'
        public required string Role { get; set; }
    }
}