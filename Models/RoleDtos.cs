using System.ComponentModel.DataAnnotations;

namespace ITAMS.Models;

public class CreateRoleDto
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    public bool IsSystemRole { get; set; } = false;
}

public class UpdateRoleDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters")]
    public string? Name { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    public bool? IsActive { get; set; }
}

public class UpdateRolePermissionsDto
{
    [Required]
    public int[] PermissionIds { get; set; } = Array.Empty<int>();
}

public class AuditEntryDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}