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
    
    [StringLength(50)]
    public string? ProjectIdRef { get; set; }
    
    public int LocationId { get; set; }
    
    [StringLength(50)]
    public string? LocationIdRef { get; set; }
    
    [Required]
    public AssetUsageCategory UsageCategory { get; set; }
    
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
    
    // Location text fields (from Excel import - display as-is)
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
    
    // Extended asset fields
    [StringLength(100)]
    public string? Classification { get; set; }
    
    [StringLength(100)]
    public string? OSType { get; set; }
    
    [StringLength(100)]
    public string? OSVersion { get; set; }
    
    [StringLength(100)]
    public string? DBType { get; set; }
    
    [StringLength(100)]
    public string? DBVersion { get; set; }
    
    [StringLength(50)]
    public string? IPAddress { get; set; }
    
    [StringLength(200)]
    public string? AssignedUserText { get; set; }
    
    [StringLength(100)]
    public string? UserRole { get; set; }
    
    [StringLength(200)]
    public string? ProcuredBy { get; set; }
    
    [StringLength(100)]
    public string? PatchStatus { get; set; }
    
    [StringLength(100)]
    public string? USBBlockingStatus { get; set; }
    
    public string? Remarks { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Placing { get; set; } = string.Empty; // lane area, booth area, plaza area, server room, control room, admin building
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
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

public enum AssetStatus
{
    InUse = 1,           // Canonical: "inuse"
    Spare = 2,           // Canonical: "spare"
    Repair = 3,          // Canonical: "repair"
    Decommissioned = 4   // Canonical: "decommissioned"
}

// Helper class for enum conversions
public static class AssetEnumHelpers
{
    // Status conversions
    public static string ToCanonicalString(this AssetStatus status)
    {
        return status switch
        {
            AssetStatus.InUse => "inuse",
            AssetStatus.Spare => "spare",
            AssetStatus.Repair => "repair",
            AssetStatus.Decommissioned => "decommissioned",
            _ => throw new ArgumentException($"Unknown status: {status}")
        };
    }

    public static string ToDisplayString(this AssetStatus status)
    {
        return status switch
        {
            AssetStatus.InUse => "In Use",
            AssetStatus.Spare => "Spare",
            AssetStatus.Repair => "Repair",
            AssetStatus.Decommissioned => "Decommissioned",
            _ => throw new ArgumentException($"Unknown status: {status}")
        };
    }

    public static AssetStatus ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be empty");

        var normalized = status.Trim().ToLower().Replace(" ", "").Replace("-", "").Replace("_", "");
        
        return normalized switch
        {
            "inuse" or "active" or "working" or "operational" or "deployed" => AssetStatus.InUse,
            "spare" or "available" or "standby" or "reserve" => AssetStatus.Spare,
            "repair" or "maintenance" or "underrepair" or "undermaintenance" or "faulty" or "broken" => AssetStatus.Repair,
            "decommissioned" or "decommitioned" or "retired" or "disposed" or "scrapped" or "obsolete" => AssetStatus.Decommissioned,
            _ => throw new ArgumentException($"Invalid status value: '{status}'. Expected: In Use, Spare, Repair, or Decommissioned")
        };
    }

    public static AssetStatus ParseStatusFromDisplay(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be empty");

        return status.Trim() switch
        {
            "In Use" => AssetStatus.InUse,
            "Spare" => AssetStatus.Spare,
            "Repair" => AssetStatus.Repair,
            "Decommissioned" => AssetStatus.Decommissioned,
            _ => ParseStatus(status) // Fall back to flexible parsing
        };
    }

    // Placing validation
    private static readonly string[] ValidPlacingValues = new[]
    {
        "Lane Area",
        "Booth Area",
        "Plaza Area",
        "Server Room",
        "Control Room",
        "Admin Building"
    };

    public static string ValidatePlacing(string? placing)
    {
        if (string.IsNullOrWhiteSpace(placing))
            throw new ArgumentException("Placing cannot be empty");

        // Normalize to title case for comparison
        var normalized = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(placing.Trim().ToLower());
        
        if (!ValidPlacingValues.Contains(normalized))
        {
            throw new ArgumentException($"Invalid placing value: '{placing}'. Expected one of: {string.Join(", ", ValidPlacingValues)}");
        }

        return normalized;
    }

    public static string[] GetValidPlacingValues() => ValidPlacingValues;
}