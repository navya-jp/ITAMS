namespace ITAMS.Models;

public class LoginAuditDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public string? IpAddress { get; set; }
    public string? BrowserType { get; set; }
    public string? OperatingSystem { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
