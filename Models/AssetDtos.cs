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
    public string? Region { get; set; }
    public string UsageCategory { get; set; } = string.Empty;
    public string Criticality { get; set; } = string.Empty;
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
}

public class CreateAssetDto
{
    public string AssetTag { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public int LocationId { get; set; }
    public string UsageCategory { get; set; } = string.Empty; // TMS, ITNonTMS
    public string Criticality { get; set; } = string.Empty; // TMSCritical, TMSGeneral, ITCritical, ITGeneral
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
    public string Status { get; set; } = "InUse"; // InUse, Spare, Repair, Decommissioned
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
    public int? AssignedUserId { get; set; }
    public string? AssignedUserRole { get; set; }
}
