using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities.MasterData;

public class Vendor
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string VendorName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string VendorCode { get; set; } = string.Empty; // Unique
    
    [StringLength(100)]
    public string? ContactPerson { get; set; }
    
    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(200)]
    public string? Website { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsDeleted { get; set; } = false; // Soft delete
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
}
