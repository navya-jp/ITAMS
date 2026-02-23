using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.Workflow;

public class ApprovalLevel
{
    public int Id { get; set; }
    
    [Required]
    public int WorkflowId { get; set; }
    
    [Required]
    public int LevelOrder { get; set; } // 1, 2, 3, 4, 5
    
    [Required]
    [StringLength(100)]
    public string LevelName { get; set; } = string.Empty; // Manager, Admin, SuperAdmin
    
    [Required]
    [StringLength(500)]
    public string RequiredApproverRoles { get; set; } = string.Empty; // JSON array: ["Manager", "Admin"]
    
    [Required]
    public int TimeoutHours { get; set; } = 24; // Hours before escalation
    
    [Required]
    [StringLength(20)]
    public string ApprovalType { get; set; } = "ANY_ONE"; // ANY_ONE or ALL_MUST_APPROVE
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    // Navigation properties
    public virtual ApprovalWorkflow Workflow { get; set; } = null!;
}
