using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.Workflow;

public class ApprovalWorkflow
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string WorkflowName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string WorkflowType { get; set; } = string.Empty; // ASSET_TRANSFER, ASSET_DECOMMISSION, ASSET_CREATION
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(2000)]
    public string? TriggerConditions { get; set; } // JSON: {criticality: "TMS_CRITICAL", assetType: "LAPTOP", valueRange: {min: 0, max: 100000}}
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual ICollection<ApprovalLevel> ApprovalLevels { get; set; } = new List<ApprovalLevel>();
}
