using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.Workflow;

public class EscalationLog
{
    public int Id { get; set; }
    
    [Required]
    public int RequestId { get; set; }
    
    [Required]
    public int RuleId { get; set; }
    
    [Required]
    public int EscalationLevel { get; set; }
    
    public DateTime EscalatedAt { get; set; }
    
    [Required]
    [StringLength(20)]
    public string TriggerReason { get; set; } = string.Empty; // TIMEOUT, EVENT, MANUAL
    
    [StringLength(500)]
    public string? NotificationsSent { get; set; } // JSON array of user IDs notified
    
    [StringLength(20)]
    public string? ActionTaken { get; set; } // NOTIFIED, AUTO_APPROVED, REASSIGNED
    
    [StringLength(1000)]
    public string? Details { get; set; }
    
    // Navigation properties
    public virtual ApprovalRequest Request { get; set; } = null!;
    public virtual EscalationRule Rule { get; set; } = null!;
}
