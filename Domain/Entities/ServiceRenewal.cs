using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class ServiceRenewal
{
    public int Id { get; set; }

    public int ServiceId { get; set; }

    public DateTime PreviousEndDate { get; set; }
    public DateTime NewStartDate { get; set; }
    public DateTime NewEndDate { get; set; }

    public decimal? RenewalCost { get; set; }

    public int RenewedBy { get; set; }
    public DateTime RenewalDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Remarks { get; set; }

    // Navigation
    public virtual ServiceAsset? ServiceAsset { get; set; }
}
