using System.ComponentModel.DataAnnotations;

namespace ITAMS.Models;

// =============================================
// Vendor DTOs
// =============================================
public class VendorDto
{
    public int Id { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string VendorCode { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVendorRequest
{
    [Required]
    [StringLength(100)]
    public string VendorName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string VendorCode { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ContactPerson { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(200)]
    public string? Website { get; set; }
}

public class UpdateVendorRequest
{
    [Required]
    [StringLength(100)]
    public string VendorName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ContactPerson { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(200)]
    public string? Website { get; set; }
    
    public bool IsActive { get; set; }
}

// =============================================
// Asset Status DTOs
// =============================================
public class AssetStatusDto
{
    public int Id { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    public bool IsPredefined { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAssetStatusRequest
{
    [Required]
    [StringLength(50)]
    public string StatusName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string StatusCode { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [StringLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color code must be in hex format (e.g., #FF5733)")]
    public string ColorCode { get; set; } = "#808080";
    
    [StringLength(50)]
    public string? Icon { get; set; }
}

public class UpdateAssetStatusRequest
{
    [Required]
    [StringLength(50)]
    public string StatusName { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [StringLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color code must be in hex format (e.g., #FF5733)")]
    public string ColorCode { get; set; } = "#808080";
    
    [StringLength(50)]
    public string? Icon { get; set; }
    
    public bool IsActive { get; set; }
}

// =============================================
// Criticality Level DTOs
// =============================================
public class CriticalityLevelDto
{
    public int Id { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string LevelCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PriorityOrder { get; set; }
    public int SlaHours { get; set; }
    public string PriorityLevel { get; set; } = string.Empty;
    public int NotificationThresholdDays { get; set; }
    public bool IsActive { get; set; }
    public bool IsPredefined { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCriticalityLevelRequest
{
    [Required]
    [StringLength(50)]
    public string LevelName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string LevelCode { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int PriorityOrder { get; set; }
    
    [Required]
    [Range(1, 168)] // Max 1 week
    public int SlaHours { get; set; }
    
    [Required]
    [StringLength(20)]
    public string PriorityLevel { get; set; } = "Medium"; // High, Medium, Low
    
    [Range(1, 365)]
    public int NotificationThresholdDays { get; set; } = 30;
}

public class UpdateCriticalityLevelRequest
{
    [Required]
    [StringLength(50)]
    public string LevelName { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int PriorityOrder { get; set; }
    
    [Required]
    [Range(1, 168)]
    public int SlaHours { get; set; }
    
    [Required]
    [StringLength(20)]
    public string PriorityLevel { get; set; } = "Medium";
    
    [Range(1, 365)]
    public int NotificationThresholdDays { get; set; } = 30;
    
    public bool IsActive { get; set; }
}
