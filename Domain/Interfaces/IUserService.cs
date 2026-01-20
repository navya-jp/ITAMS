using ITAMS.Domain.Entities;

namespace ITAMS.Domain.Interfaces;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string username, string password);
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User> UpdateUserAsync(int id, UpdateUserRequest request);
    Task DeleteUserAsync(int id);
    Task<User?> GetUserByIdAsync(int id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetUsersByRoleIdAsync(int roleId);
    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task ResetPasswordAsync(int userId, string newPassword);
    Task LockUserAsync(int userId);
    Task UnlockUserAsync(int userId);
    Task<bool> ValidatePasswordPolicyAsync(string password);
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole? Role { get; set; } // For backward compatibility
    public int? RoleId { get; set; } // New role system
    public string Password { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; } = true;
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole? Role { get; set; } // For backward compatibility
    public int? RoleId { get; set; } // New role system
    public bool? IsActive { get; set; }
}