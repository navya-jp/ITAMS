using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.Workflow;

public class ApprovalHistory
{
    public int Id { get; set; }
    
    [Required]
    public int RequestId { get; set; }
    
    [Required]
    public int Level { get; set; }
    
    [Required]
    public int ApproverId { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Action { get; set; } = string.Empty; // APPROVED, REJECTED, ESCALATED
    
    public DateTime ActionAt { get; set; }
    
    [StringLength(1000)]
    public string? Comments { get; set; }
    
    [StringLength(50)]
    public string? IpAddress { get; set; }
    
    // Navigation properties
    public virtual ApprovalRequest Request { get; set; } = null!;
    public virtual User Approver { get; set; } = null!;
}
