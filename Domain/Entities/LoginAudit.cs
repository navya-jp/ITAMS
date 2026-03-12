using System.ComponentModel.DataAnnotations;
using ITAMS.Domain.Entities.MasterData;

namespace ITAMS.Domain.Entities;

public class LoginAudit
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    [Required]
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    
    [StringLength(50)]
    public string? IpAddress { get; set; }
    
    [StringLength(200)]
    public string? BrowserType { get; set; }
    
    [StringLength(200)]
    public string? OperatingSystem { get; set; }
    
    public DateTime? LogoutTime { get; set; }
    
    [StringLength(500)]
    public string? SessionId { get; set; }
    
    // NORMALIZED: Status now uses FK instead of text
    public int? SessionStatusId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual SessionStatus? SessionStatus { get; set; }
}
