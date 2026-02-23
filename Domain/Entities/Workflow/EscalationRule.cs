using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.Workflow;

public class EscalationRule
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string RuleName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string TriggerType { get; set; } = "TIME_BASED"; // TIME_BASED, EVENT_BASED, BOTH
    
    public int? TimeoutHours { get; set; } // For time-based escalation
    
    [StringLength(1000)]
    public string? EventConditions { get; set; } // JSON: {criticality: "CRITICAL", pendingDuration: 24}
    
    [Required]
    public int EscalationLevel { get; set; } // 1, 2, 3
    
    [Required]
    [StringLength(500)]
    public string EscalationTargetRoles { get; set; } = string.Empty; // JSON array: ["Admin", "SuperAdmin"]
    
    [Required]
    [StringLength(20)]
    public string EscalationAction { get; set; } = "NOTIFY"; // NOTIFY, AUTO_APPROVE, REASSIGN
    
    [StringLength(100)]
    public string? NotificationTemplate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
}
