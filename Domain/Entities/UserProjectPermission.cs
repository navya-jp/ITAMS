namespace ITAMS.Domain.Entities;

public class UserProjectPermission
{
    public int Id { get; set; }
    
    public int UserProjectId { get; set; }
    
    public int PermissionId { get; set; }
    
    public bool IsGranted { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int GrantedBy { get; set; }
    
    // Navigation properties
    public virtual UserProject UserProject { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}