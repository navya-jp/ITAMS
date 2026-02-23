using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.Workflow;

public class ApprovalRequest
{
    public int Id { get; set; }
    
    [Required]
    public int WorkflowId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string RequestType { get; set; } = string.Empty; // ASSET_TRANSFER, ASSET_DECOMMISSION, ASSET_CREATION
    
    [Required]
    public int RequestedBy { get; set; }
    
    public DateTime RequestedAt { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, ESCALATED
    
    public int CurrentLevel { get; set; } = 1;
    
    [StringLength(2000)]
    public string? RequestDetails { get; set; } // JSON with request-specific data
    
    public int? AssetId { get; set; } // Related asset if applicable
    
    [StringLength(500)]
    public string? RejectionReason { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public int? CompletedBy { get; set; }
    
    // Navigation properties
    public virtual ApprovalWorkflow Workflow { get; set; } = null!;
    public virtual User RequestedByUser { get; set; } = null!;
    public virtual ICollection<ApprovalHistory> ApprovalHistories { get; set; } = new List<ApprovalHistory>();
}
