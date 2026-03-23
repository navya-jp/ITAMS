using System.ComponentModel.DataAnnotations;
using ITAMS.Domain.Entities.MasterData;

namespace ITAMS.Domain.Entities;

public class ServiceAsset
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string AssetId { get; set; } = string.Empty; // ASTV00001, ASTV00002, etc.

    // Foreign Keys
    public int ServiceTypeId { get; set; }
    public int? ProjectId { get; set; }
    public int? LocationId { get; set; }
    public int? VendorId { get; set; }

    // Contract Information
    [StringLength(200)]
    public string ServiceName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContractNumber { get; set; }

    [StringLength(200)]
    public string VendorName { get; set; } = string.Empty;

    public DateTime ContractStartDate { get; set; }
    public DateTime ContractEndDate { get; set; }

    public int RenewalCycleMonths { get; set; } = 12;
    public int RenewalReminderDays { get; set; } = 30;

    // Financial
    public decimal? ContractCost { get; set; }

    [StringLength(50)]
    public string? BillingCycle { get; set; } // Monthly, Quarterly, Annually

    [StringLength(10)]
    public string Currency { get; set; } = "INR";

    // Operational
    [StringLength(100)]
    public string? SLAType { get; set; }

    [StringLength(100)]
    public string? ResponseTime { get; set; }

    [StringLength(500)]
    public string? CoverageDetails { get; set; }

    [StringLength(200)]
    public string? ContactPerson { get; set; }

    [StringLength(50)]
    public string? SupportContactNumber { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    // Usage Category
    [StringLength(50)]
    public string UsageCategory { get; set; } = "TMS"; // TMS or ITNonTMS

    // Lifecycle
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Active"; // Active, Expiring, Expired, Cancelled

    public DateTime? LastRenewalDate { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public bool AutoRenewEnabled { get; set; } = false;

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation
    public virtual ServiceType? ServiceType { get; set; }
    public virtual Project? Project { get; set; }
    public virtual Location? Location { get; set; }
    public virtual Vendor? Vendor { get; set; }
    public virtual ICollection<ServiceRenewal> Renewals { get; set; } = new List<ServiceRenewal>();
}
