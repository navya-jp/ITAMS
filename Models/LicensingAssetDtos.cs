namespace ITAMS.Models;

public class LicensingAssetDto
{
    public int Id { get; set; }
    public string AssetId { get; set; } = string.Empty;
    public string LicenseName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public int NumberOfLicenses { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ValidityStartDate { get; set; }
    public DateTime ValidityEndDate { get; set; }
    public string ValidityType { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateLicensingAssetDto
{
    public string LicenseName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public int NumberOfLicenses { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ValidityStartDate { get; set; }
    public DateTime ValidityEndDate { get; set; }
    public string ValidityType { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string? AssetTag { get; set; }
    public string Status { get; set; } = "Active";
}

public class UpdateLicensingAssetDto
{
    public string? LicenseName { get; set; }
    public string? Version { get; set; }
    public string? LicenseKey { get; set; }
    public string? LicenseType { get; set; }
    public int? NumberOfLicenses { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ValidityStartDate { get; set; }
    public DateTime? ValidityEndDate { get; set; }
    public string? ValidityType { get; set; }
    public string? Vendor { get; set; }
    public string? Publisher { get; set; }
    public string? AssetTag { get; set; }
    public string? Status { get; set; }
}

public class ServiceAssetDto
{
    public int Id { get; set; }
    public string AssetId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ServiceTypeId { get; set; }
    public string? ServiceTypeName { get; set; }
    public int ContractTypeId { get; set; }
    public string? ContractTypeName { get; set; }
    public string Vendor { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string RenewalCycle { get; set; } = string.Empty;
    public int RenewalReminderDays { get; set; }
    public DateTime? LastRenewalDate { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? ContractValue { get; set; }
    public string? ContractNumber { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateServiceAssetDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ServiceTypeId { get; set; }
    public int ContractTypeId { get; set; }
    public string Vendor { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string RenewalCycle { get; set; } = string.Empty;
    public int RenewalReminderDays { get; set; }
    public string Status { get; set; } = "Active";
    public decimal? ContractValue { get; set; }
    public string? ContractNumber { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateServiceAssetDto
{
    public string? ServiceName { get; set; }
    public string? Description { get; set; }
    public int? ServiceTypeId { get; set; }
    public int? ContractTypeId { get; set; }
    public string? Vendor { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? RenewalCycle { get; set; }
    public int? RenewalReminderDays { get; set; }
    public DateTime? LastRenewalDate { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public string? Status { get; set; }
    public decimal? ContractValue { get; set; }
    public string? ContractNumber { get; set; }
    public string? Remarks { get; set; }
}
