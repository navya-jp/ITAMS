using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class Project
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string ProjectId { get; set; } = string.Empty; // Alternate key (PRJ00001, PRJ00002, etc.)
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? PreferredName { get; set; }
    
    [StringLength(200)]
    public string? SpvName { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? States { get; set; } // Comma-separated list of states
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}