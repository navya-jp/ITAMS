using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class AssetType
{
    public int Id { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty; // Unique within category
    
    [Required]
    [StringLength(50)]
    public string TypeCode { get; set; } = string.Empty; // Unique
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual AssetCategory Category { get; set; } = null!;
    public virtual ICollection<AssetSubType> SubTypes { get; set; } = new List<AssetSubType>();
    public virtual ICollection<TypeFieldMapping> FieldMappings { get; set; } = new List<TypeFieldMapping>();
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
