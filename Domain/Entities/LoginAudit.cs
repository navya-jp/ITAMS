using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class LoginAudit
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    
    [StringLength(50)]
    public string? IpAddress { get; set; }
    
    [StringLength(200)]
    public string? BrowserType { get; set; }
    
    [StringLength(200)]
    public string? OperatingSystem { get; set; }
    
    [Required]
    [StringLength(500)]
    public string SessionId { get; set; } = string.Empty;
    
    public DateTime? LogoutTime { get; set; }
    
    [StringLength(50)]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, LOGGED_OUT, EXPIRED
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}
