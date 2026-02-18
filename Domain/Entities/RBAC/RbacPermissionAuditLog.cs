using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAMS.Domain.Entities.RBAC;

[Table("rbac_permission_audit_log")]
public class RbacPermissionAuditLog
{
    [Key]
    [Column("audit_id")]
    public int AuditId { get; set; }
    
    [Column("actor_user_id")]
    public int ActorUserId { get; set; }
    
    [Column("target_user_id")]
    public int? TargetUserId { get; set; }
    
    [Column("role_id")]
    public int? RoleId { get; set; }
    
    [Column("permission_id")]
    public int? PermissionId { get; set; }
    
    [Required]
    [StringLength(50)]
    [Column("action_type")]
    public string ActionType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    [Column("entity_type")]
    public string EntityType { get; set; } = string.Empty;
    
    [Column("old_value")]
    public string? OldValue { get; set; }
    
    [Column("new_value")]
    public string? NewValue { get; set; }
    
    [StringLength(500)]
    [Column("reason")]
    public string? Reason { get; set; }
    
    [StringLength(45)]
    [Column("ip_address")]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    [Column("user_agent")]
    public string? UserAgent { get; set; }
    
    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("ActorUserId")]
    public virtual User ActorUser { get; set; } = null!;
    
    [ForeignKey("TargetUserId")]
    public virtual User? TargetUser { get; set; }
    
    [ForeignKey("RoleId")]
    public virtual RbacRole? Role { get; set; }
    
    [ForeignKey("PermissionId")]
    public virtual RbacPermission? Permission { get; set; }
    
    // Helper properties
    public bool IsUserAction => TargetUserId.HasValue;
    public bool IsRoleAction => RoleId.HasValue;
    public bool IsPermissionAction => PermissionId.HasValue;
    public string ActorName => ActorUser?.Username ?? "System";
    public string TargetName => TargetUser?.Username ?? "N/A";
    public string EntityName => Role?.RoleName ?? Permission?.PermissionCode ?? "Unknown";
}

// Audit action types
public static class AuditActionTypes
{
    public const string Grant = "GRANT";
    public const string Revoke = "REVOKE";
    public const string RoleAssign = "ROLE_ASSIGN";
    public const string ScopeChange = "SCOPE_CHANGE";
    public const string RoleCreate = "ROLE_CREATE";
    public const string RoleUpdate = "ROLE_UPDATE";
    public const string RoleDeactivate = "ROLE_DEACTIVATE";
    public const string PermissionCreate = "PERMISSION_CREATE";
    public const string PermissionDeactivate = "PERMISSION_DEACTIVATE";
}

// Audit entity types
public static class AuditEntityTypes
{
    public const string UserPermission = "USER_PERMISSION";
    public const string RolePermission = "ROLE_PERMISSION";
    public const string UserScope = "USER_SCOPE";
    public const string Role = "ROLE";
    public const string Permission = "PERMISSION";
}