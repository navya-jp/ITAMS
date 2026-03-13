using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class ServiceType
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty; // AMC, LeasedLine, Maintenance, etc.
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
}
