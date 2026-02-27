namespace ITAMS.Models;

public class AssetDto
{
    public int Id { get; set; }
    public string AssetId { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int LocationId { get; set; }
    public string? LocationName { get; set; }
    
    // Location text fields (from Excel - display as-is)
    public string? Region { get; set; }
    public string? State { get; set; }
    public string? Site { get; set; }
    public string? PlazaName { get; set; }
    public string? LocationText { get; set; }
    public string? Department { get; set; }
    
    // Extended asset fields
    public string? Classification { get; set; }
    public string? OSType { get; set; }
    public string? OSVersion { get; set; }
    public string? DBType { get; set; }
    public string? DBVersion { get; set; }
    public string? IPAddress { get; set; }
    public string? AssignedUserText { get; set; }
    public string? UserRole { get; set; }
    public string? ProcuredBy { get; set; }
    public string? PatchStatus { get; set; }
    public string? USBBlockingStatus { get; set; }
    public string? Remarks { get; set; }
    public string Placing { get; set; } = string.Empty; // Required: lane area, booth area, plaza area, server room, control room, admin building
    
    public string UsageCategory { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string? SubType { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime? ProcurementDate { get; set; }
    public decimal? ProcurementCost { get; set; }
    public string? Vendor { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? CommissioningDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public string? AssignedUserRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAssetDto
{
    public string AssetTag { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public int LocationId { get; set; }
    public string UsageCategory { get; set; } = string.Empty; // TMS, ITNonTMS
    public string AssetType { get; set; } = string.Empty;
    public string? SubType { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime? ProcurementDate { get; set; }
    public decimal? ProcurementCost { get; set; }
    public string? Vendor { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? CommissioningDate { get; set; }
    public string Status { get; set; } = "In Use"; // Display: In Use, Spare, Repair, Decommissioned
    public string Placing { get; set; } = string.Empty; // Required: lane area, booth area, plaza area, server room, control room, admin building
    public int? AssignedUserId { get; set; }
    public string? AssignedUserRole { get; set; }
}

public class UpdateAssetDto
{
    public string? AssetTag { get; set; }
    public int? LocationId { get; set; }
    public string? AssetType { get; set; }
    public string? SubType { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ProcurementDate { get; set; }
    public decimal? ProcurementCost { get; set; }
    public string? Vendor { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? CommissioningDate { get; set; }
    public string? Status { get; set; }
    public string? Placing { get; set; }
    public int? AssignedUserId { get; set; }
    public string? AssignedUserRole { get; set; }
}
