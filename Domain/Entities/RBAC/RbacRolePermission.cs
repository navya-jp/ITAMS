using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAMS.Domain.Entities.RBAC;

[Table("rbac_role_permissions")]
public class RbacRolePermission
{
    [Key]
    [Column("role_permission_id")]
    public int RolePermissionId { get; set; }
    
    [Column("role_id")]
    public int RoleId { get; set; }
    
    [Column("permission_id")]
    public int PermissionId { get; set; }
    
    [Column("allowed")]
    public bool Allowed { get; set; } = true;
    
    [Column("granted_at")]
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    [Column("granted_by")]
    public int GrantedBy { get; set; }
    
    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }
    
    [Column("revoked_by")]
    public int? RevokedBy { get; set; }
    
    [Required]
    [StringLength(20)]
    [Column("status")]
    public string Status { get; set; } = "ACTIVE";
    
    // Navigation properties
    [ForeignKey("RoleId")]
    public virtual RbacRole Role { get; set; } = null!;
    
    [ForeignKey("PermissionId")]
    public virtual RbacPermission Permission { get; set; } = null!;
    
    [ForeignKey("GrantedBy")]
    public virtual User GrantedByUser { get; set; } = null!;
    
    [ForeignKey("RevokedBy")]
    public virtual User? RevokedByUser { get; set; }
    
    // Helper properties
    public bool IsActive => Status == "ACTIVE";
    public bool IsRevoked => Status == "REVOKED";
    public bool IsEffective => IsActive && Allowed;
}

// Role permission status constants
public static class RolePermissionStatus
{
    public const string Active = "ACTIVE";
    public const string Revoked = "REVOKED";
}