using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAMS.Domain.Entities.RBAC;

[Table("RbacRoles")]
public class RbacRole
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string RbacRoleId { get; set; } = string.Empty; // Alternate key (RBR00001, RBR00002, etc.)
    
    // Keep RoleId as alias for Id for backward compatibility with existing code
    [NotMapped]
    public int RoleId => Id;
    
    [Required]
    [StringLength(100)]
    [Column("Name")]
    public string RoleName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsSystemRole { get; set; } = false;
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "ACTIVE";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    public DateTime? DeactivatedAt { get; set; }
    
    public int? DeactivatedBy { get; set; }
    
    // Navigation properties
    [ForeignKey("CreatedBy")]
    public virtual User CreatedByUser { get; set; } = null!;
    
    [ForeignKey("DeactivatedBy")]
    public virtual User? DeactivatedByUser { get; set; }
    
    public virtual ICollection<RbacRolePermission> RolePermissions { get; set; } = new List<RbacRolePermission>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    
    // Helper properties
    public bool IsActive => Status == "ACTIVE";
    public bool IsDeactivated => Status == "INACTIVE";
}

// Status enum for type safety
public static class RbacRoleStatus
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
}