using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class ContractType
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty; // Comprehensive AMC, Non-Comprehensive AMC, Breakdown Visits Only, etc.
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
}
