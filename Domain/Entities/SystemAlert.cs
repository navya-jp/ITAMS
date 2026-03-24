using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class SystemAlert
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string AlertType { get; set; } = string.Empty;
    // WARRANTY_EXPIRY, LICENSE_EXPIRY, CONTRACT_EXPIRY, ASSET_AGE, REPAIR_STUCK, COMPLIANCE_FAILURE

    [Required]
    [StringLength(50)]
    public string Severity { get; set; } = "Medium";
    // Low, Medium, High, Critical

    [Required]
    [StringLength(500)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Message { get; set; }

    // Reference to the affected entity
    public int? AssetId { get; set; }
    public int? LicensingAssetId { get; set; }
    public int? ServiceAssetId { get; set; }

    [StringLength(100)]
    public string? EntityType { get; set; } // Asset, LicensingAsset, ServiceAsset

    [StringLength(100)]
    public string? EntityIdentifier { get; set; } // AssetTag, license name, etc.

    public bool IsAcknowledged { get; set; } = false;
    public int? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }

    public bool EmailSent { get; set; } = false;
    public DateTime? EmailSentAt { get; set; }

    public int EscalationLevel { get; set; } = 1; // 1=IT Staff, 2=Project Admin, 3=Super Admin
    public DateTime? LastEscalatedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved { get; set; } = false;

    // Navigation
    public virtual Asset? Asset { get; set; }
}
