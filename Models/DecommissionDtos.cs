namespace ITAMS.Models;

public class EolCandidateDto
{
    public int Id { get; set; }
    public string AssetId { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? AssetType { get; set; }
    public string? Location { get; set; }
    public string? Project { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? CommissioningDate { get; set; }
    public string? CurrentStatus { get; set; }
    public string EolReason { get; set; } = string.Empty;
    public int? DaysSinceWarrantyExpired { get; set; }
    public double? AssetAgeYears { get; set; }
    public bool HasPendingRequest { get; set; }
}

public class InitiateDecommissionDto
{
    public int AssetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string DisposalMethod { get; set; } = string.Empty; // Scrap / Donate / Return to Vendor / Write-off
    public string? Notes { get; set; }
}

public class DecommissionRequestDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetMake { get; set; } = string.Empty;
    public string AssetModel { get; set; } = string.Empty;
    public string? AssetLocation { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public string? Reason { get; set; }
    public string? DisposalMethod { get; set; }
    public string? Notes { get; set; }
    public DateTime RequestedAt { get; set; }
    public string? RequestedByName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<ApprovalHistoryDto> ApprovalHistory { get; set; } = new();
}

public class ApprovalHistoryDto
{
    public int Level { get; set; }
    public string? ApproverName { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime ActionAt { get; set; }
    public string? Comments { get; set; }
}

public class ApproveDecommissionDto
{
    public string? Comments { get; set; }
}

public class RejectDecommissionDto
{
    public string Reason { get; set; } = string.Empty;
}

public class DecommissionArchiveDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string DecommissionReason { get; set; } = string.Empty;
    public string DisposalMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime ArchivedAt { get; set; }
    public string? ArchivedByName { get; set; }
    public string AssetSnapshot { get; set; } = string.Empty;
    public string ApprovalChainSnapshot { get; set; } = string.Empty;
}
