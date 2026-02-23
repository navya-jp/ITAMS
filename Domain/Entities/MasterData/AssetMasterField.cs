using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class AssetMasterField
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty; // Unique
    
    [Required]
    [StringLength(50)]
    public string FieldCode { get; set; } = string.Empty; // Unique, used in code
    
    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = "Text"; // Text, Number, Date, DateTime, Boolean, Dropdown, MultiSelect, Email, Phone, URL
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    public bool IsRequired { get; set; } = false;
    
    [StringLength(500)]
    public string? DefaultValue { get; set; }
    
    [StringLength(1000)]
    public string? ValidationRules { get; set; } // JSON string with validation rules
    
    public int? MaxLength { get; set; } // For text fields
    
    public decimal? MinValue { get; set; } // For number fields
    
    public decimal? MaxValue { get; set; } // For number fields
    
    [StringLength(500)]
    public string? RegexPattern { get; set; } // For custom validation
    
    [StringLength(200)]
    public string? ValidationMessage { get; set; } // Custom error message
    
    [StringLength(1000)]
    public string? DropdownOptions { get; set; } // JSON array for dropdown/multiselect options
    
    [StringLength(100)]
    public string? FieldGroup { get; set; } // Group fields together (e.g., "Hardware Specs", "Procurement Details")
    
    public int DisplayOrder { get; set; } = 0; // Order in which fields appear in forms
    
    public bool IsActive { get; set; } = true;
    
    public bool IsSystemField { get; set; } = false; // Cannot be deleted if true
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
}
