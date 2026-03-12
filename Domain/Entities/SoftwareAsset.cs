using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class SoftwareAsset
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string SoftwareName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Version { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string LicenseKey { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string LicenseType { get; set; } = string.Empty; // Subscription, Open Source, Per User, Per Core, Per Device
    
    [Required]
    public int NumberOfLicenses { get; set; }
    
    [Required]
    public DateTime PurchaseDate { get; set; }
    
    [Required]
    public DateTime ValidityStartDate { get; set; }
    
    [Required]
    public DateTime ValidityEndDate { get; set; }
    
    [Required]
    [StringLength(50)]
    public string ValidityType { get; set; } = string.Empty; // Renewable, Perennial
    
    [Required]
    [StringLength(100)]
    public string Vendor { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Publisher { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string AssetTag { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Active, Expired, Available
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
}
