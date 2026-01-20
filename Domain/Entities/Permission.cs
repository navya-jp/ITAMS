using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class Permission
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Module { get; set; } = string.Empty; // Assets, Users, Reports, etc.
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<UserProjectPermission> UserProjectPermissions { get; set; } = new List<UserProjectPermission>();
}

public enum PermissionModule
{
    Assets,
    Users,
    Projects,
    Locations,
    Reports,
    Settings,
    Audit
}