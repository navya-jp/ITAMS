namespace ITAMS.Domain.Entities;

public class RolePermission
{
    public int Id { get; set; }
    
    public int RoleId { get; set; }
    
    public int PermissionId { get; set; }
    
    public bool IsGranted { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}