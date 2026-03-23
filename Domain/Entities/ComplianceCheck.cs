using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class ComplianceCheck
{
    public int Id { get; set; }
    public int AssetId { get; set; }

    [Required]
    [StringLength(100)]
    public string CheckType { get; set; } = string.Empty; // PatchStatus, USBBlocking, WarrantyExpiry, Security

    [StringLength(20)]
    public string Result { get; set; } = "Pass"; // Pass, Fail, Warning

    [StringLength(500)]
    public string? Details { get; set; }

    [StringLength(500)]
    public string? Remediation { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Open"; // Open, Resolved, Waived

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public int CheckedBy { get; set; }
    [StringLength(200)]
    public string? CheckedByName { get; set; }

    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedBy { get; set; }

    // Navigation
    public virtual Asset Asset { get; set; } = null!;
}
