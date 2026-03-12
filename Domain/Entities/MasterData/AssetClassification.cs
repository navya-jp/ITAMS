using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class AssetClassification
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string ClassificationId { get; set; } = string.Empty; // Alternate key
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Critical", "High", "Medium", "Low"
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
