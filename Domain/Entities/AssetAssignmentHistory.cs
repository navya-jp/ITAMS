using System.ComponentModel.DataAnnotations;

namespace ITAMS.Domain.Entities;

public class AssetAssignmentHistory
{
    public int Id { get; set; }
    public int AssetId { get; set; }

    public int? PreviousUserId { get; set; }
    [StringLength(200)]
    public string? PreviousUserName { get; set; }

    public int? NewUserId { get; set; }
    [StringLength(200)]
    public string? NewUserName { get; set; }

    public int? PreviousLocationId { get; set; }
    [StringLength(200)]
    public string? PreviousLocationName { get; set; }

    public int? NewLocationId { get; set; }
    [StringLength(200)]
    public string? NewLocationName { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public int ChangedBy { get; set; }
    [StringLength(200)]
    public string? ChangedByName { get; set; }

    // Navigation
    public virtual Asset Asset { get; set; } = null!;
    public virtual User? PreviousUser { get; set; }
    public virtual User? NewUser { get; set; }
    public virtual Location? PreviousLocation { get; set; }
    public virtual Location? NewLocation { get; set; }
}
