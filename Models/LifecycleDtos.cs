namespace ITAMS.Models;

// ── Assignment History ────────────────────────────────────────────────────────

public class AssignmentHistoryDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int? PreviousUserId { get; set; }
    public string? PreviousUserName { get; set; }
    public int? NewUserId { get; set; }
    public string? NewUserName { get; set; }
    public int? PreviousLocationId { get; set; }
    public string? PreviousLocationName { get; set; }
    public int? NewLocationId { get; set; }
    public string? NewLocationName { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedByName { get; set; }
}

// ── Transfer ──────────────────────────────────────────────────────────────────

public class TransferRequestDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int FromLocationId { get; set; }
    public string? FromLocationName { get; set; }
    public int ToLocationId { get; set; }
    public string? ToLocationName { get; set; }
    public int? FromUserId { get; set; }
    public string? FromUserName { get; set; }
    public int? ToUserId { get; set; }
    public string? ToUserName { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Notes { get; set; }
    public DateTime TransferDate { get; set; }
    public string? RequestedByName { get; set; }
}

public class CreateTransferDto
{
    public int ToLocationId { get; set; }
    public int? ToUserId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

// ── Maintenance ───────────────────────────────────────────────────────────────

public class MaintenanceRequestDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string RequestType { get; set; } = "Maintenance";
    public string Description { get; set; } = string.Empty;
    public string? OldSpecifications { get; set; }
    public string? NewSpecifications { get; set; }
    public string? VendorName { get; set; }
    public decimal? Cost { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "Open";
    public string? Resolution { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
}

public class CreateMaintenanceDto
{
    public string RequestType { get; set; } = "Maintenance";
    public string Description { get; set; } = string.Empty;
    public string? OldSpecifications { get; set; }
    public string? NewSpecifications { get; set; }
    public string? VendorName { get; set; }
    public decimal? Cost { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateMaintenanceDto
{
    public string? Status { get; set; }
    public string? Resolution { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? Cost { get; set; }
    public string? VendorName { get; set; }
    public string? NewSpecifications { get; set; }
    public string? Remarks { get; set; }
}

// ── Compliance ────────────────────────────────────────────────────────────────

public class ComplianceCheckDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string CheckType { get; set; } = string.Empty;
    public string Result { get; set; } = "Pass";
    public string? Details { get; set; }
    public string? Remediation { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CheckedAt { get; set; }
    public string? CheckedByName { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CreateComplianceCheckDto
{
    public string CheckType { get; set; } = string.Empty;
    public string Result { get; set; } = "Pass";
    public string? Details { get; set; }
    public string? Remediation { get; set; }
}

public class ResolveComplianceDto
{
    public string? Resolution { get; set; }
}

// ── Lifecycle Summary ─────────────────────────────────────────────────────────

public class AssetLifecycleSummaryDto
{
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public List<AssignmentHistoryDto> AssignmentHistory { get; set; } = new();
    public List<TransferRequestDto> TransferHistory { get; set; } = new();
    public List<MaintenanceRequestDto> MaintenanceRequests { get; set; } = new();
    public List<ComplianceCheckDto> ComplianceChecks { get; set; } = new();
    public int OpenMaintenanceCount { get; set; }
    public int FailedComplianceCount { get; set; }
}
