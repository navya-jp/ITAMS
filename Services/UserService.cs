using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace ITAMS.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditService _auditService;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository, IAuditService auditService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditService = auditService;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        
        if (user == null || !user.IsActive)
        {
            return null;
        }

        // Check if user is locked
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            return null;
        }

        // Verify password
        bool isPasswordValid = false;
        bool isFirstLogin = user.LastLoginAt == null;
        
        if (user.MustChangePassword && isFirstLogin)
        {
            // For first-time users, accept any password and set it as their new password
            isPasswordValid = true;
            
            // Hash and store the password they provided as their new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.MustChangePassword = false; // They've now set their password
            user.PasswordChangedAt = DateTime.UtcNow;
        }
        else
        {
            // Normal password verification for users who have logged in before
            isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        if (!isPasswordValid)
        {
            // Increment failed login attempts
            user.FailedLoginAttempts++;
            
            // Lock user after 5 failed attempts for 30 minutes
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
            }
            
            await _userRepository.UpdateAsync(user);
            await _auditService.LogAsync("LOGIN_FAILED", "User", user.Id.ToString(), user.Id, user.Username);
            
            return null;
        }

        // Reset failed login attempts and update last login
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(user);
        await _auditService.LogAsync("LOGIN_SUCCESS", "User", user.Id.ToString(), user.Id, user.Username);
        
        return user;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Validate username uniqueness
        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Validate email uniqueness
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Validate password policy
        if (!await ValidatePasswordPolicyAsync(request.Password))
        {
            throw new InvalidOperationException("Password does not meet policy requirements");
        }

        // Determine role ID
        int roleId;
        if (request.RoleId.HasValue)
        {
            roleId = request.RoleId.Value;
        }
        else if (request.Role.HasValue)
        {
            roleId = (int)request.Role.Value; // Convert enum to int
        }
        else
        {
            throw new InvalidOperationException("Role must be specified");
        }

        // Validate role exists
        if (!await _roleRepository.ExistsAsync(roleId))
        {
            throw new InvalidOperationException("Invalid role specified");
        }

        // Get the role to fetch its RoleId alternate key
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        // Use projectId if provided, otherwise default to 1 (first project)
        int finalProjectId = request.ProjectId ?? 1;

        // Generate UserId (alternate key) - get the next available number
        var lastUser = await _userRepository.GetAllAsync();
        var maxId = lastUser.Any() ? lastUser.Max(u => u.Id) : 0;
        var userId = $"USR{(maxId + 1):D5}"; // Format: USR00001, USR00002, etc.

        var user = new User
        {
            UserId = userId,
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RoleId = roleId,
            RoleIdRef = role.RoleId, // Set the alternate key reference
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            MustChangePassword = request.MustChangePassword,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ProjectId = finalProjectId,
            RestrictedRegion = request.RestrictedRegion,
            RestrictedState = request.RestrictedState,
            RestrictedPlaza = request.RestrictedPlaza,
            RestrictedOffice = request.RestrictedOffice
        };

        var createdUser = await _userRepository.CreateAsync(user);
        await _auditService.LogAsync("USER_CREATED", "User", createdUser.Id.ToString(), createdUser.Id, createdUser.Username);
        
        // Reload user with Role navigation property
        return await _userRepository.GetByIdAsync(createdUser.Id) ?? createdUser;
    }

    public async Task<User> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var oldValues = $"Email: {user.Email}, FirstName: {user.FirstName}, LastName: {user.LastName}, RoleId: {user.RoleId}, IsActive: {user.IsActive}, ProjectId: {user.ProjectId}";

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Email))
        {
            if (await _userRepository.EmailExistsAsync(request.Email, id))
            {
                throw new InvalidOperationException("Email already exists");
            }
            user.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            user.LastName = request.LastName;
        }

        // Handle role update
        if (request.RoleId.HasValue)
        {
            if (!await _roleRepository.ExistsAsync(request.RoleId.Value))
            {
                throw new InvalidOperationException("Invalid role specified");
            }
            user.RoleId = request.RoleId.Value;
        }
        else if (request.Role.HasValue)
        {
            var roleId = (int)request.Role.Value;
            if (!await _roleRepository.ExistsAsync(roleId))
            {
                throw new InvalidOperationException("Invalid role specified");
            }
            user.RoleId = roleId;
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        // Update project and location fields
        if (request.ProjectId.HasValue)
        {
            user.ProjectId = request.ProjectId.Value;
        }

        if (request.RestrictedRegion != null)
        {
            user.RestrictedRegion = string.IsNullOrWhiteSpace(request.RestrictedRegion) ? null : request.RestrictedRegion;
        }

        if (request.RestrictedState != null)
        {
            user.RestrictedState = string.IsNullOrWhiteSpace(request.RestrictedState) ? null : request.RestrictedState;
        }

        if (request.RestrictedPlaza != null)
        {
            user.RestrictedPlaza = string.IsNullOrWhiteSpace(request.RestrictedPlaza) ? null : request.RestrictedPlaza;
        }

        if (request.RestrictedOffice != null)
        {
            user.RestrictedOffice = string.IsNullOrWhiteSpace(request.RestrictedOffice) ? null : request.RestrictedOffice;
        }

        var updatedUser = await _userRepository.UpdateAsync(user);
        
        var newValues = $"Email: {user.Email}, FirstName: {user.FirstName}, LastName: {user.LastName}, RoleId: {user.RoleId}, IsActive: {user.IsActive}, ProjectId: {user.ProjectId}";
        await _auditService.LogAsync("USER_UPDATED", "User", user.Id.ToString(), user.Id, user.Username, oldValues, newValues);
        
        // Reload user with Role navigation property
        return await _userRepository.GetByIdAsync(updatedUser.Id) ?? updatedUser;
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Prevent deletion of the last Super Admin
        if (user.RoleId == 1) // Super Admin role ID
        {
            var superAdminCount = (await _userRepository.GetByRoleIdAsync(1)).Count();
            if (superAdminCount <= 1)
            {
                throw new InvalidOperationException("Cannot delete the last Super Admin user");
            }
        }

        await _userRepository.DeleteAsync(id);
        await _auditService.LogAsync("USER_DELETED", "User", id.ToString(), user.Id, user.Username);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _userRepository.GetByRoleAsync(role);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleIdAsync(int roleId)
    {
        return await _userRepository.GetByRoleIdAsync(roleId);
    }

    public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect");
        }

        // Validate new password policy
        if (!await ValidatePasswordPolicyAsync(newPassword))
        {
            throw new InvalidOperationException("New password does not meet policy requirements");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.MustChangePassword = false;

        await _userRepository.UpdateAsync(user);
        await _auditService.LogAsync("PASSWORD_CHANGED", "User", user.Id.ToString(), user.Id, user.Username);
    }

    public async Task ResetPasswordAsync(int userId, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Validate new password policy
        if (!await ValidatePasswordPolicyAsync(newPassword))
        {
            throw new InvalidOperationException("New password does not meet policy requirements");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.MustChangePassword = true; // Force password change on next login
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;

        await _userRepository.UpdateAsync(user);
        await _auditService.LogAsync("PASSWORD_RESET", "User", user.Id.ToString(), user.Id, user.Username);
    }

    public async Task LockUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.LockedUntil = DateTime.UtcNow.AddYears(1); // Lock for a long time
        await _userRepository.UpdateAsync(user);
        await _auditService.LogAsync("USER_LOCKED", "User", user.Id.ToString(), user.Id, user.Username);
    }

    public async Task UnlockUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.LockedUntil = null;
        user.FailedLoginAttempts = 0;
        await _userRepository.UpdateAsync(user);
        await _auditService.LogAsync("USER_UNLOCKED", "User", user.Id.ToString(), user.Id, user.Username);
    }

    public async Task<bool> ValidatePasswordPolicyAsync(string password)
    {
        // Password policy: At least 8 characters, 1 uppercase, 1 lowercase, 1 digit, 1 special character
        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            return false;
        }

        var hasUpperCase = password.Any(char.IsUpper);
        var hasLowerCase = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        var existingUser = await _userRepository.GetByUsernameAsync(username);
        return existingUser == null;
    }

    public async Task UpdateSessionAsync(int userId, string sessionId, DateTime sessionStartedAt)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.ActiveSessionId = sessionId;
        user.SessionStartedAt = sessionStartedAt;
        
        await _userRepository.UpdateAsync(user);
        await _auditService.LogAsync("SESSION_STARTED", "User", user.Id.ToString(), user.Id, user.Username);
    }

    public async Task ClearSessionAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.ActiveSessionId = null;
        user.SessionStartedAt = null;
        
        await _userRepository.UpdateAsync(user);
        await _auditService.LogAsync("SESSION_ENDED", "User", user.Id.ToString(), user.Id, user.Username);
    }
}
