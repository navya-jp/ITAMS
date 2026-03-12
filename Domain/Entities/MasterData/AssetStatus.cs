using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class AssetStatus
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string StatusName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string StatusCode { get; set; } = string.Empty; // Unique
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [StringLength(7)]
    public string ColorCode { get; set; } = "#808080"; // Hex color for UI
    
    [StringLength(50)]
    public string? Icon { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsPredefined { get; set; } = false; // Cannot be deleted if true
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
