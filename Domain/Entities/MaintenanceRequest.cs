using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class MaintenanceRequest
{
    public int Id { get; set; }
    public int AssetId { get; set; }

    [Required]
    [StringLength(50)]
    public string RequestType { get; set; } = "Maintenance"; // Maintenance, Upgrade, Repair

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    // Old vs new specs (for upgrades)
    [StringLength(1000)]
    public string? OldSpecifications { get; set; }

    [StringLength(1000)]
    public string? NewSpecifications { get; set; }

    [StringLength(200)]
    public string? VendorName { get; set; }

    public decimal? Cost { get; set; }

    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Open"; // Open, In Progress, Completed, Cancelled

    [StringLength(500)]
    public string? Resolution { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    [StringLength(200)]
    public string? CreatedByName { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation
    public virtual Asset Asset { get; set; } = null!;
}
