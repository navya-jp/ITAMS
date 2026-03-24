namespace ITAMS.Models;

public class AlertDto
{
    public int Id { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public int? AssetId { get; set; }
    public string? EntityType { get; set; }
    public string? EntityIdentifier { get; set; }
    public bool IsAcknowledged { get; set; }
    public bool IsResolved { get; set; }
    public int EscalationLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}

public class AlertSummaryDto
{
    public int TotalUnread { get; set; }
    public int Critical { get; set; }
    public int High { get; set; }
    public int Medium { get; set; }
    public int Low { get; set; }
}

public class DashboardReportDto
{
    public List<ExpiringItemDto> WarrantyExpiring { get; set; } = new();
    public List<ExpiringItemDto> LicenseExpiring { get; set; } = new();
    public List<ExpiringItemDto> ContractExpiring { get; set; } = new();
    public List<AssetInRepairDto> AssetsInRepair { get; set; } = new();
    public List<ComplianceFailureDto> ComplianceFailures { get; set; } = new();
    public List<AlertDto> RecentAlerts { get; set; } = new();
}

public class ExpiringItemDto
{
    public int Id { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Project { get; set; }
    public string? Location { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int DaysRemaining { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public class AssetInRepairDto
{
    public int Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Project { get; set; }
    public string? Location { get; set; }
    public int DaysInRepair { get; set; }
}

public class ComplianceFailureDto
{
    public int Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string? Project { get; set; }
}
