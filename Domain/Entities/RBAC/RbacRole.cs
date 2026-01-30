using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAMS.Domain.Entities.RBAC;

[Table("rbac_roles")]
public class RbacRole
{
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }
    
    [Required]
    [StringLength(100)]
    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;
    
    [StringLength(500)]
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("is_system_role")]
    public bool IsSystemRole { get; set; } = false;
    
    [Required]
    [StringLength(20)]
    [Column("status")]
    public string Status { get; set; } = "ACTIVE";
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("created_by")]
    public int CreatedBy { get; set; }
    
    [Column("deactivated_at")]
    public DateTime? DeactivatedAt { get; set; }
    
    [Column("deactivated_by")]
    public int? DeactivatedBy { get; set; }
    
    // Navigation properties
    [ForeignKey("CreatedBy")]
    public virtual User CreatedByUser { get; set; } = null!;
    
    [ForeignKey("DeactivatedBy")]
    public virtual User? DeactivatedByUser { get; set; }
    
    public virtual ICollection<RbacRolePermission> RolePermissions { get; set; } = new List<RbacRolePermission>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    
    // Helper properties
    public bool IsActive => Status == "ACTIVE";
    public bool IsDeactivated => Status == "INACTIVE";
}

// Status enum for type safety
public static class RbacRoleStatus
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
}