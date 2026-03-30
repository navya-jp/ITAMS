using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class DecommissionArchive
{
    public int Id { get; set; }
    public int AssetId { get; set; }

    [Required]
    [StringLength(50)]
    public string AssetTag { get; set; } = string.Empty;

    [Required]
    public string AssetSnapshot { get; set; } = string.Empty; // Full JSON snapshot

    [Required]
    [StringLength(2000)]
    public string DecommissionReason { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DisposalMethod { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Notes { get; set; }

    public int ApprovalRequestId { get; set; }

    public string ApprovalChainSnapshot { get; set; } = string.Empty; // JSON

    public DateTime ArchivedAt { get; set; }
    public int ArchivedBy { get; set; }

    [StringLength(200)]
    public string? ArchivedByName { get; set; }

    // Navigation
    public virtual Asset Asset { get; set; } = null!;
}
