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
    public int ServiceTypeId { get; set; }
    public string? ServiceTypeName { get; set; }
    public int? ProjectId { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int? VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? ContractNumber { get; set; }
    public DateTime ContractStartDate { get; set; }
    public DateTime ContractEndDate { get; set; }
    public int RenewalCycleMonths { get; set; }
    public int RenewalReminderDays { get; set; }
    public decimal? ContractCost { get; set; }
    public string? BillingCycle { get; set; }
    public string Currency { get; set; } = "INR";
    public string? SLAType { get; set; }
    public string? ResponseTime { get; set; }
    public string? CoverageDetails { get; set; }
    public string? ContactPerson { get; set; }
    public string? SupportContactNumber { get; set; }
    public string? Description { get; set; }
    public string? Remarks { get; set; }
    public string UsageCategory { get; set; } = "TMS";
    public string Status { get; set; } = string.Empty;
    public DateTime? LastRenewalDate { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ServiceRenewalDto> Renewals { get; set; } = new();
}

public class CreateServiceAssetDto
{
    public string ServiceName { get; set; } = string.Empty;
    public int ServiceTypeId { get; set; }
    public int? ProjectId { get; set; }
    public int? LocationId { get; set; }
    public int? VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? ContractNumber { get; set; }
    public DateTime ContractStartDate { get; set; }
    public DateTime ContractEndDate { get; set; }
    public int RenewalCycleMonths { get; set; } = 12;
    public int RenewalReminderDays { get; set; } = 30;
    public decimal? ContractCost { get; set; }
    public string? BillingCycle { get; set; }
    public string Currency { get; set; } = "INR";
    public string? SLAType { get; set; }
    public string? ResponseTime { get; set; }
    public string? CoverageDetails { get; set; }
    public string? ContactPerson { get; set; }
    public string? SupportContactNumber { get; set; }
    public string? Description { get; set; }
    public string? Remarks { get; set; }
    public string UsageCategory { get; set; } = "TMS";
    public bool AutoRenewEnabled { get; set; } = false;
}

public class UpdateServiceAssetDto
{
    public string? ServiceName { get; set; }
    public int? ServiceTypeId { get; set; }
    public int? LocationId { get; set; }
    public int? VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public int? RenewalCycleMonths { get; set; }
    public int? RenewalReminderDays { get; set; }
    public decimal? ContractCost { get; set; }
    public string? BillingCycle { get; set; }
    public string? Currency { get; set; }
    public string? SLAType { get; set; }
    public string? ResponseTime { get; set; }
    public string? CoverageDetails { get; set; }
    public string? ContactPerson { get; set; }
    public string? SupportContactNumber { get; set; }
    public string? Description { get; set; }
    public string? Remarks { get; set; }
    public string? UsageCategory { get; set; }
    public string? Status { get; set; }
    public bool? AutoRenewEnabled { get; set; }
}

public class RenewServiceDto
{
    public DateTime NewEndDate { get; set; }
    public decimal? RenewalCost { get; set; }
    public string? Remarks { get; set; }
}

public class ServiceRenewalDto
{
    public int Id { get; set; }
    public DateTime PreviousEndDate { get; set; }
    public DateTime NewStartDate { get; set; }
    public DateTime NewEndDate { get; set; }
    public decimal? RenewalCost { get; set; }
    public int RenewedBy { get; set; }
    public DateTime RenewalDate { get; set; }
    public string? Remarks { get; set; }
}
