using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class AssetSubType
{
    public int Id { get; set; }
    
    [Required]
    public int TypeId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string SubTypeName { get; set; } = string.Empty; // Unique within type
    
    [Required]
    [StringLength(50)]
    public string SubTypeCode { get; set; } = string.Empty; // Unique
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual AssetType Type { get; set; } = null!;
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
