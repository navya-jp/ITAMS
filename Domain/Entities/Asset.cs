using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITAMS.Domain.Entities.MasterData;

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
    
    // Asset Category (Hardware, Licensing, Services)
    public int? AssetCategoryId { get; set; }
    
    public int ProjectId { get; set; }
    
    [StringLength(50)]
    public string? ProjectIdRef { get; set; }
    
    public int LocationId { get; set; }
    
    [StringLength(50)]
    public string? LocationIdRef { get; set; }
    
    [Required]
    public AssetUsageCategory UsageCategory { get; set; }
    
    // NORMALIZED: AssetType now uses FK instead of text
    public int? AssetTypeId { get; set; }
    
    [Column("AssetType")]
    [StringLength(100)]
    public string AssetTypeName { get; set; } = string.Empty;
    
    // NORMALIZED: SubType now uses FK instead of text
    public int? AssetSubTypeId { get; set; }
    
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
    
    // NORMALIZED: Vendor now uses FK instead of text
    public int? VendorId { get; set; }
    
    public DateTime? WarrantyStartDate { get; set; }
    
    public DateTime? WarrantyEndDate { get; set; }
    
    public DateTime? CommissioningDate { get; set; }

    [StringLength(100)]
    public string? CommissioningDateText { get; set; } // Raw text from sheet e.g. "December/2023"
    
    // NORMALIZED: Status now uses FK instead of enum
    public int? AssetStatusId { get; set; }
    
    // Legacy status column (kept for DB compatibility)
    public int Status { get; set; } = 0;
    
    public int? AssignedUserId { get; set; }

    [StringLength(200)]
    public string? AssignedUserText { get; set; } // Raw text from bulk upload (user may not exist in system)
    
    // Location text fields (from Excel import - display as-is, NOT used for queries)
    [StringLength(100)]
    public string? Region { get; set; }
    
    [StringLength(100)]
    public string? State { get; set; }
    
    [StringLength(200)]
    public string? Site { get; set; }
    
    [StringLength(200)]
    public string? PlazaName { get; set; }
    
    [StringLength(200)]
    public string? LocationText { get; set; }
    
    [StringLength(100)]
    public string? Department { get; set; }
    
    // NORMALIZED: Classification now uses FK instead of text
    public int? AssetClassificationId { get; set; }
    
    // NORMALIZED: OSType now uses FK instead of text
    public int? OperatingSystemId { get; set; }
    
    [StringLength(100)]
    public string? OSVersion { get; set; }
    
    // NORMALIZED: DBType now uses FK instead of text
    public int? DatabaseTypeId { get; set; }
    
    [StringLength(100)]
    public string? DBVersion { get; set; }
    
    [StringLength(50)]
    public string? IPAddress { get; set; }
    
    [StringLength(200)]
    public string? ProcuredBy { get; set; }
    
    // NORMALIZED: PatchStatus now uses FK instead of text
    public int? PatchStatusId { get; set; }
    
    // NORMALIZED: USBBlockingStatus now uses FK instead of text
    public int? USBBlockingStatusId { get; set; }
    
    public string? Remarks { get; set; }
    
    // NORMALIZED: Placing now uses FK instead of text
    public int? AssetPlacingId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
    public virtual User? AssignedUser { get; set; }
    public virtual AssetType? AssetType { get; set; }
    public virtual AssetSubType? SubType { get; set; }
    public virtual Vendor? Vendor { get; set; }
    public virtual Domain.Entities.MasterData.AssetStatus? AssetStatus { get; set; }
    public virtual AssetClassification? Classification { get; set; }
    public virtual MasterData.OperatingSystem? OperatingSystem { get; set; }
    public virtual DatabaseType? DatabaseType { get; set; }
    public virtual PatchStatus? PatchStatus { get; set; }
    public virtual USBBlockingStatus? USBBlockingStatus { get; set; }
    public virtual AssetPlacing? Placing { get; set; }
    public virtual MasterData.AssetCategory? Category { get; set; }
}

public enum AssetUsageCategory
{
    TMS = 1,
    ITNonTMS = 2
}