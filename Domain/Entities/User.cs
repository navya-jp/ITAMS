using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public int RoleId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public DateTime? PasswordChangedAt { get; set; }
    
    public bool MustChangePassword { get; set; } = false;
    
    public int FailedLoginAttempts { get; set; } = 0;
    
    public DateTime? LockedUntil { get; set; }
    
    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual ICollection<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();
    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    public virtual ICollection<Asset> AssignedAssets { get; set; } = new List<Asset>();
}

// Keep the enum for backward compatibility, but we'll use the Role entity
public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    ITStaff = 3,
    ReadOnlyUser = 4,
    Auditor = 5
}