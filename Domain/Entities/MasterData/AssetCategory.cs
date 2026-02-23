using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class AssetCategory
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty; // Unique
    
    [Required]
    [StringLength(50)]
    public string CategoryCode { get; set; } = string.Empty; // Unique
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Icon { get; set; } // Icon name or class
    
    [StringLength(7)]
    public string? ColorCode { get; set; } // Hex color for UI
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsPredefined { get; set; } = false; // Cannot be deleted if true
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual ICollection<AssetType> AssetTypes { get; set; } = new List<AssetType>();
}
