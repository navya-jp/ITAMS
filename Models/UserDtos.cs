using System.ComponentModel.DataAnnotations;

namespace ITAMS.Models;

public class UserDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty; // Alternate key (USR00001, USR00002, etc.)
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool MustChangePassword { get; set; }
    public bool IsLocked { get; set; }
}

public class CreateUserDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and dots")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid role")]
    public int RoleId { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*?&)")]
    public string Password { get; set; } = string.Empty;
    
    public bool MustChangePassword { get; set; } = true;
}

public class UpdateUserDto
{
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string? Email { get; set; }
    
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
    public string? FirstName { get; set; }
    
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
    public string? LastName { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid role")]
    public int? RoleId { get; set; }
    
    public bool? IsActive { get; set; }
}

public class ResetPasswordDto
{
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*?&)")]
    public string NewPassword { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}