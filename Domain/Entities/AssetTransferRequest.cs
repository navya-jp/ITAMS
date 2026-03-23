using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class AssetTransferRequest
{
    public int Id { get; set; }
    public int AssetId { get; set; }

    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }

    public int? FromUserId { get; set; }
    public int? ToUserId { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Completed"; // Completed, Pending, Cancelled

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public int RequestedBy { get; set; }
    [StringLength(200)]
    public string? RequestedByName { get; set; }

    // Navigation
    public virtual Asset Asset { get; set; } = null!;
    public virtual Location FromLocation { get; set; } = null!;
    public virtual Location ToLocation { get; set; } = null!;
    public virtual User? FromUser { get; set; }
    public virtual User? ToUser { get; set; }
}
