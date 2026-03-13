using System.ComponentModel.DataAnnotations;
using ITAMS.Domain.Entities.MasterData;

namespace ITAMS.Domain.Entities;

public class ServiceAsset
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string AssetId { get; set; } = string.Empty; // Auto-generated: ASTV00001, ASTV00002, etc.
    
    [Required]
    [StringLength(200)]
    public string ServiceName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    // Foreign Keys to Master Data
    [Required]
    public int ServiceTypeId { get; set; }
    
    [Required]
    public int ContractTypeId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Vendor { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    [StringLength(50)]
    public string RenewalCycle { get; set; } = string.Empty; // Monthly, Quarterly, Annually, etc.
    
    [Required]
    public int RenewalReminderDays { get; set; } // Days before expiry to send reminder
    
    public DateTime? LastRenewalDate { get; set; }
    
    public DateTime? NextRenewalDate { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Active, Expired, Pending Renewal, etc.
    
    public decimal? ContractValue { get; set; }
    
    [StringLength(500)]
    public string? ContractNumber { get; set; }
    
    [StringLength(500)]
    public string? Remarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual ServiceType? ServiceType { get; set; }
    public virtual ContractType? ContractType { get; set; }
}
