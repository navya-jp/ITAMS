using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class SessionStatus
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string StatusId { get; set; } = string.Empty; // Alternate key
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "ACTIVE", "LOGGED_OUT", "SESSION_TIMEOUT", "FORCED_LOGOUT"
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<LoginAudit> LoginAudits { get; set; } = new List<LoginAudit>();
}
