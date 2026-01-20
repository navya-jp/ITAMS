namespace ITAMS.Domain.Entities;

public class UserProject
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int ProjectId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    public int AssignedBy { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<UserProjectPermission> UserProjectPermissions { get; set; } = new List<UserProjectPermission>();
}