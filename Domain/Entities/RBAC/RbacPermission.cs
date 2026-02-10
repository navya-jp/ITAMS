using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAMS.Domain.Entities.RBAC;

[Table("RbacPermissions")]
public class RbacPermission
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string RbacPermissionId { get; set; } = string.Empty; // Alternate key (RBP00001, RBP00002, etc.)
    
    // Keep PermissionId as alias for Id for backward compatibility with existing code
    [NotMapped]
    public int PermissionId => Id;
    
    [Required]
    [StringLength(100)]
    [Column("Code")]
    public string PermissionCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Module { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string ResourceType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "ACTIVE";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    public DateTime? DeactivatedAt { get; set; }
    
    public int? DeactivatedBy { get; set; }
    
    // Navigation properties
    [ForeignKey("CreatedBy")]
    public virtual User CreatedByUser { get; set; } = null!;
    
    [ForeignKey("DeactivatedBy")]
    public virtual User? DeactivatedByUser { get; set; }
    
    public virtual ICollection<RbacRolePermission> RolePermissions { get; set; } = new List<RbacRolePermission>();
    public virtual ICollection<RbacUserPermission> UserPermissions { get; set; } = new List<RbacUserPermission>();
    
    // Helper properties
    public bool IsActive => Status == "ACTIVE";
    public bool IsDeactivated => Status == "INACTIVE";
    public string FullName => $"{Module}.{PermissionCode}";
}

// Permission modules for organization
public static class PermissionModules
{
    public const string UserManagement = "USER_MANAGEMENT";
    public const string AssetManagement = "ASSET_MANAGEMENT";
    public const string LifecycleRepairs = "LIFECYCLE_REPAIRS";
    public const string ReportsAudits = "REPORTS_AUDITS";
    public const string RbacManagement = "RBAC_MANAGEMENT";
}

// Common permission actions
public static class PermissionActions
{
    public const string Create = "CREATE";
    public const string View = "VIEW";
    public const string Edit = "EDIT";
    public const string Delete = "DELETE";
    public const string Deactivate = "DEACTIVATE";
    public const string Reactivate = "REACTIVATE";
    public const string Transfer = "TRANSFER";
    public const string Approve = "APPROVE";
    public const string Export = "EXPORT";
    public const string Download = "DOWNLOAD";
    public const string Log = "LOG";
    public const string Schedule = "SCHEDULE";
    public const string Assign = "ASSIGN";
    public const string Override = "OVERRIDE";
}

// Predefined permission codes for type safety
public static class PermissionCodes
{
    // User Management
    public const string UserCreate = "USER_CREATE";
    public const string UserView = "USER_VIEW";
    public const string UserEdit = "USER_EDIT";
    public const string UserDeactivate = "USER_DEACTIVATE";
    public const string UserReactivate = "USER_REACTIVATE";
    public const string RoleAssign = "ROLE_ASSIGN";
    public const string PermissionOverride = "PERMISSION_OVERRIDE";
    
    // Asset Management
    public const string AssetCreate = "ASSET_CREATE";
    public const string AssetView = "ASSET_VIEW";
    public const string AssetEdit = "ASSET_EDIT";
    public const string AssetTransfer = "ASSET_TRANSFER";
    public const string AssetDecommission = "ASSET_DECOMMISSION";
    public const string AssetReactivate = "ASSET_REACTIVATE";
    
    // Lifecycle & Repairs
    public const string LifecycleLog = "LIFECYCLE_LOG";
    public const string LifecycleView = "LIFECYCLE_VIEW";
    public const string LifecycleApprove = "LIFECYCLE_APPROVE";
    public const string RepairAdd = "REPAIR_ADD";
    public const string RepairView = "REPAIR_VIEW";
    public const string MaintenanceSchedule = "MAINTENANCE_SCHEDULE";
    
    // Reports & Audits
    public const string ReportView = "REPORT_VIEW";
    public const string ReportExport = "REPORT_EXPORT";
    public const string AuditView = "AUDIT_VIEW";
    public const string AuditDownload = "AUDIT_DOWNLOAD";
    public const string FinancialView = "FINANCIAL_VIEW";
    
    // RBAC Management
    public const string RoleCreate = "ROLE_CREATE";
    public const string RoleView = "ROLE_VIEW";
    public const string RoleEdit = "ROLE_EDIT";
    public const string RoleDeactivate = "ROLE_DEACTIVATE";
    public const string PermissionCreate = "PERMISSION_CREATE";
    public const string PermissionView = "PERMISSION_VIEW";
    public const string PermissionEdit = "PERMISSION_EDIT";
}

// Permission status constants
public static class PermissionStatus
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
}