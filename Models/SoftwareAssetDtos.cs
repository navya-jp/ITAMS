namespace ITAMS.Models;

public class SoftwareAssetDto
{
    public int Id { get; set; }
    public string SoftwareName { get; set; } = string.Empty;
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

public class CreateSoftwareAssetDto
{
    public string SoftwareName { get; set; } = string.Empty;
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
    public string Status { get; set; } = "Active";
}

public class UpdateSoftwareAssetDto
{
    public string? SoftwareName { get; set; }
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
