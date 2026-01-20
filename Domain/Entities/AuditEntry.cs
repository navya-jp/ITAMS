using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class AuditEntry
{
    public int Id { get; set; }
    
    [Required]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    public string EntityType { get; set; } = string.Empty;
    
    public string? EntityId { get; set; }
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public int UserId { get; set; }
    
    [Required]
    public string UserName { get; set; } = string.Empty;
    
    public string? IpAddress { get; set; }
    
    public string? UserAgent { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}