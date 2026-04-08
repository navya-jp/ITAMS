namespace ITAMS.Models;

public class AssetReportFilter
{
    public int? ProjectId { get; set; }
    public int? LocationId { get; set; }
    public string? AssetType { get; set; }
    public string? Status { get; set; }
    public string? UsageCategory { get; set; }
    public DateTime? ProcuredFrom { get; set; }
    public DateTime? ProcuredTo { get; set; }
    public bool? IsDecommissioned { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

public class MaintenanceFilter
{
    public string? Status { get; set; }
    public int? AssetId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class ComplianceFilter
{
    public int? ProjectId { get; set; }
    public string? Result { get; set; }
}

public class TransferFilter
{
    public int? ProjectId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class UserActivityFilter
{
    public DateTime From { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime To { get; set; } = DateTime.Today.AddDays(1);
    public int? UserId { get; set; }
}

public class ExportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, string>? Filter { get; set; }
}

// ── KPI ───────────────────────────────────────────────────────────────────────

public class DashboardKpiDto
{
    public int TotalHardwareAssets { get; set; }
    public int TotalLicenses { get; set; }
    public int TotalServiceContracts { get; set; }
    public int AssetsInUse { get; set; }
    public int AssetsInRepair { get; set; }
    public int AssetsDecommissioned { get; set; }
    public int WarrantyExpiringIn30Days { get; set; }
    public int LicensesExpiringIn30Days { get; set; }
    public int ContractsExpiringIn30Days { get; set; }
    public int OpenMaintenanceRequests { get; set; }
    public int FailedComplianceChecks { get; set; }
    public int UnacknowledgedAlerts { get; set; }
    public int ActiveUsers { get; set; }
    public List<ChartItemDto> AssetsByType { get; set; } = new();
    public List<ChartItemDto> AssetsByLocation { get; set; } = new();
    public List<ChartItemDto> AssetsByStatus { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyProcurementTrend { get; set; } = new();
}

public class ChartItemDto { public string Label { get; set; } = ""; public int Count { get; set; } }
public class MonthlyTrendDto { public int Year { get; set; } public int Month { get; set; } public string Label { get; set; } = ""; public int Count { get; set; } }

// ── Report Items ──────────────────────────────────────────────────────────────

public class AssetInventoryItem
{
    public string AssetId { get; set; } = "";
    public string AssetTag { get; set; } = "";
    public string? Project { get; set; }
    public string? Location { get; set; }
    public string? AssetType { get; set; }
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public string? SerialNumber { get; set; }
    public string? Status { get; set; }
    public string? AssignedUser { get; set; }
    public string? Vendor { get; set; }
    public DateTime? ProcurementDate { get; set; }
    public decimal? ProcurementCost { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string? PatchStatus { get; set; }
    public string? USBStatus { get; set; }
    public string? UsageCategory { get; set; }
    public bool IsDecommissioned { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssetInventoryReport
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<AssetInventoryItem> Items { get; set; } = new();
}

public class ExpiryReportItem
{
    public int Id { get; set; }
    public string Identifier { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Project { get; set; }
    public string? Location { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int DaysRemaining { get; set; }
    public string Severity { get; set; } = "";
    public string? Extra { get; set; }
}

public class MaintenanceReportItem
{
    public int Id { get; set; }
    public string AssetTag { get; set; } = "";
    public string? AssetType { get; set; }
    public string? Project { get; set; }
    public string? Location { get; set; }
    public string RequestType { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? Cost { get; set; }
    public string? VendorName { get; set; }
    public string? CreatedByName { get; set; }
    public int DaysOpen { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ComplianceReportItem
{
    public string AssetTag { get; set; } = "";
    public string? Project { get; set; }
    public string? Location { get; set; }
    public string CheckType { get; set; } = "";
    public string Result { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CheckedAt { get; set; }
    public string? Details { get; set; }
}

public class TransferReportItem
{
    public int Id { get; set; }
    public DateTime TransferDate { get; set; }
    public string AssetTag { get; set; } = "";
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public string? FromUser { get; set; }
    public string? ToUser { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "";
    public string? RequestedBy { get; set; }
}

public class UserActivityItem
{
    public string Username { get; set; } = "";
    public string? Role { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public int? SessionMinutes { get; set; }
    public string? IpAddress { get; set; }
    public string? BrowserType { get; set; }
    public string? OperatingSystem { get; set; }
    public string? SessionStatus { get; set; }
}

public class UserActivityReport
{
    public int TotalSessions { get; set; }
    public double AverageSessionMinutes { get; set; }
    public int UniqueUsers { get; set; }
    public int PeakLoginHour { get; set; }
    public List<UserActivityItem> Items { get; set; } = new();
}

public class AlertReportItem
{
    public int Id { get; set; }
    public string AlertType { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Title { get; set; } = "";
    public string? EntityIdentifier { get; set; }
    public int EscalationLevel { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}
