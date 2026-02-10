using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class Asset
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string AssetId { get; set; } = string.Empty; // Alternate key (AST00001, AST00002, etc.)
    
    [Required]
    [StringLength(50)]
    public string AssetTag { get; set; } = string.Empty;
    
    public int ProjectId { get; set; }
    
    public int LocationId { get; set; }
    
    [Required]
    public AssetUsageCategory UsageCategory { get; set; }
    
    [Required]
    public AssetCriticality Criticality { get; set; }
    
    [Required]
    [StringLength(100)]
    public string AssetType { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? SubType { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Make { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? SerialNumber { get; set; }
    
    public DateTime? ProcurementDate { get; set; }
    
    public decimal? ProcurementCost { get; set; }
    
    [StringLength(200)]
    public string? Vendor { get; set; }
    
    public DateTime? WarrantyStartDate { get; set; }
    
    public DateTime? WarrantyEndDate { get; set; }
    
    public DateTime? CommissioningDate { get; set; }
    
    [Required]
    public AssetStatus Status { get; set; } = AssetStatus.InUse;
    
    public int? AssignedUserId { get; set; }
    
    [StringLength(100)]
    public string? AssignedUserRole { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
    public virtual User? AssignedUser { get; set; }
}

public enum AssetUsageCategory
{
    TMS = 1,
    ITNonTMS = 2
}

public enum AssetCriticality
{
    TMSCritical = 1,
    TMSGeneral = 2,
    ITCritical = 3,
    ITGeneral = 4
}

public enum AssetStatus
{
    InUse = 1,
    Spare = 2,
    Repair = 3,
    Decommissioned = 4
}