using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAMS.Domain.Entities.RBAC;

[Table("rbac_user_permissions")]
public class RbacUserPermission
{
    [Key]
    [Column("user_permission_id")]
    public int UserPermissionId { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("permission_id")]
    public int PermissionId { get; set; }
    
    [Column("allowed")]
    public bool Allowed { get; set; }
    
    [Column("granted_at")]
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    [Column("granted_by")]
    public int GrantedBy { get; set; }
    
    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }
    
    [Column("revoked_by")]
    public int? RevokedBy { get; set; }
    
    [StringLength(500)]
    [Column("reason")]
    public string? Reason { get; set; }
    
    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }
    
    [Required]
    [StringLength(20)]
    [Column("status")]
    public string Status { get; set; } = "ACTIVE";
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    [ForeignKey("PermissionId")]
    public virtual RbacPermission Permission { get; set; } = null!;
    
    [ForeignKey("GrantedBy")]
    public virtual User GrantedByUser { get; set; } = null!;
    
    [ForeignKey("RevokedBy")]
    public virtual User? RevokedByUser { get; set; }
    
    // Helper properties
    public bool IsActive => Status == "ACTIVE";
    public bool IsRevoked => Status == "REVOKED";
    public bool IsExpired => Status == "EXPIRED" || (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow);
    public bool IsEffective => IsActive && !IsExpired;
    public bool IsTemporary => ExpiresAt.HasValue;
    
    // Check if this override is currently valid
    public bool IsCurrentlyValid()
    {
        if (!IsActive || IsRevoked) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow) return false;
        return true;
    }
    
    // Get the effective permission value (considering expiration)
    public bool? GetEffectiveValue()
    {
        if (!IsCurrentlyValid()) return null;
        return Allowed;
    }
}

// User permission status constants
public static class UserPermissionStatus
{
    public const string Active = "ACTIVE";
    public const string Revoked = "REVOKED";
    public const string Expired = "EXPIRED";
}