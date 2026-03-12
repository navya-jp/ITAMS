using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class OperatingSystem
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OSId { get; set; } = string.Empty; // Alternate key
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Windows 10", "Windows 11", "Ubuntu 20.04", "macOS"
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
