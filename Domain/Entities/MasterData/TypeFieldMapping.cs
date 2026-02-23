using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class TypeFieldMapping
{
    public int Id { get; set; }
    
    [Required]
    public int AssetTypeId { get; set; }
    
    [Required]
    public int FieldId { get; set; }
    
    public bool IsRequired { get; set; } = false; // Override field's default required flag
    
    [StringLength(500)]
    public string? DefaultValue { get; set; } // Override field's default value
    
    public int DisplayOrder { get; set; } = 0; // Override field's display order for this type
    
    public bool IsVisible { get; set; } = true; // Show/hide field for this type
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    // Navigation properties
    public virtual AssetType AssetType { get; set; } = null!;
    public virtual AssetMasterField Field { get; set; } = null!;
}
