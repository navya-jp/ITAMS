using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class Location
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string LocationId { get; set; } = string.Empty; // Alternate key (LOC00001, LOC00002, etc.)
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Region { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Plaza { get; set; }
    
    [StringLength(100)]
    public string? Lane { get; set; }
    
    [StringLength(100)]
    public string? Office { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int ProjectId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Sensitive location flag (for future use)
    public bool IsSensitive { get; set; } = false;
    
    [StringLength(500)]
    public string? SensitiveReason { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}