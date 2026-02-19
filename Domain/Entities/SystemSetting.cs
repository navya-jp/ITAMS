using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class SystemSetting
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string SettingKey { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string SettingValue { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "General";
    
    [Required]
    [StringLength(20)]
    public string DataType { get; set; } = "String"; // String, Integer, Boolean, Decimal
    
    public bool IsEditable { get; set; } = true;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
