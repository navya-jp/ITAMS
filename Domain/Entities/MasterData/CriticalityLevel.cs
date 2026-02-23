using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class CriticalityLevel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string LevelName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string LevelCode { get; set; } = string.Empty; // Unique
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [Required]
    public int PriorityOrder { get; set; } // 1-10, unique
    
    [Required]
    public int SlaHours { get; set; } // Response time in hours
    
    [Required]
    [StringLength(20)]
    public string PriorityLevel { get; set; } = "Medium"; // High, Medium, Low
    
    public int NotificationThresholdDays { get; set; } = 30; // Days before expiry to notify
    
    public bool IsActive { get; set; } = true;
    
    public bool IsPredefined { get; set; } = false; // Cannot be deleted if true
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
}
