using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAMS.Domain.Entities.RBAC;

[Table("rbac_user_scope")]
public class RbacUserScope
{
    [Key]
    [Column("user_scope_id")]
    public int UserScopeId { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(20)]
    [Column("scope_type")]
    public string ScopeType { get; set; } = string.Empty;
    
    [Column("project_id")]
    public int? ProjectId { get; set; }
    
    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    [Column("assigned_by")]
    public int AssignedBy { get; set; }
    
    [Column("removed_at")]
    public DateTime? RemovedAt { get; set; }
    
    [Column("removed_by")]
    public int? RemovedBy { get; set; }
    
    [Required]
    [StringLength(20)]
    [Column("status")]
    public string Status { get; set; } = "ACTIVE";
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    [ForeignKey("ProjectId")]
    public virtual Project? Project { get; set; }
    
    [ForeignKey("AssignedBy")]
    public virtual User AssignedByUser { get; set; } = null!;
    
    [ForeignKey("RemovedBy")]
    public virtual User? RemovedByUser { get; set; }
    
    // Helper properties
    public bool IsActive => Status == "ACTIVE";
    public bool IsRemoved => Status == "REMOVED";
    public bool IsGlobalScope => ScopeType == ScopeTypes.Global;
    public bool IsProjectScope => ScopeType == ScopeTypes.Project;
    
    // Validation methods
    public bool IsValidScope()
    {
        return (IsGlobalScope && ProjectId == null) || (IsProjectScope && ProjectId.HasValue);
    }
    
    // Check if user has access to a specific project
    public bool HasAccessToProject(int? projectId)
    {
        if (!IsActive) return false;
        
        // Global scope has access to all projects
        if (IsGlobalScope) return true;
        
        // Project scope only has access to assigned project
        if (IsProjectScope) return ProjectId == projectId;
        
        return false;
    }
}

// Scope type constants
public static class ScopeTypes
{
    public const string Global = "GLOBAL";
    public const string Project = "PROJECT";
}

// User scope status constants
public static class UserScopeStatus
{
    public const string Active = "ACTIVE";
    public const string Removed = "REMOVED";
}