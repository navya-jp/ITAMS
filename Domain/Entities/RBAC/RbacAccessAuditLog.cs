using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAMS.Domain.Entities.RBAC;

[Table("rbac_access_audit_log")]
public class RbacAccessAuditLog
{
    [Key]
    [Column("access_audit_id")]
    public int AccessAuditId { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    [Column("permission_code")]
    public string PermissionCode { get; set; } = string.Empty;
    
    [StringLength(100)]
    [Column("resource_id")]
    public string? ResourceId { get; set; }
    
    [StringLength(50)]
    [Column("resource_type")]
    public string? ResourceType { get; set; }
    
    [StringLength(50)]
    [Column("action_attempted")]
    public string? ActionAttempted { get; set; }
    
    [Column("access_granted")]
    public bool AccessGranted { get; set; }
    
    [StringLength(500)]
    [Column("denial_reason")]
    public string? DenialReason { get; set; }
    
    [StringLength(50)]
    [Column("resolution_method")]
    public string? ResolutionMethod { get; set; }
    
    [Column("scope_validated")]
    public bool ScopeValidated { get; set; } = false;
    
    [StringLength(45)]
    [Column("ip_address")]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    [Column("user_agent")]
    public string? UserAgent { get; set; }
    
    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    // Helper properties
    public bool WasAccessDenied => !AccessGranted;
    public bool WasResolvedByUserOverride => ResolutionMethod == ResolutionMethods.UserOverride;
    public bool WasResolvedByRolePermission => ResolutionMethod == ResolutionMethods.RolePermission;
    public bool WasDefaultDenied => ResolutionMethod == ResolutionMethods.DefaultDeny;
    public string UserName => User?.Username ?? "Unknown";
    
    // Get a human-readable description of the access attempt
    public string GetDescription()
    {
        var action = ActionAttempted ?? "access";
        var resource = ResourceType ?? "resource";
        var resourceInfo = !string.IsNullOrEmpty(ResourceId) ? $" (ID: {ResourceId})" : "";
        
        return $"User '{UserName}' attempted to {action} {resource}{resourceInfo}";
    }
    
    // Get the outcome description
    public string GetOutcome()
    {
        if (AccessGranted)
        {
            return $"Access granted via {ResolutionMethod?.Replace("_", " ").ToLower() ?? "unknown method"}";
        }
        else
        {
            return $"Access denied: {DenialReason ?? "No explicit permission"}";
        }
    }
}

// Permission resolution methods
public static class ResolutionMethods
{
    public const string UserOverride = "USER_OVERRIDE";
    public const string RolePermission = "ROLE_PERMISSION";
    public const string DefaultDeny = "DEFAULT_DENY";
    public const string ScopeViolation = "SCOPE_VIOLATION";
    public const string ResourceInactive = "RESOURCE_INACTIVE";
    public const string UserInactive = "USER_INACTIVE";
}