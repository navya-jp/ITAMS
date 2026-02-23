namespace ITAMS.Domain.Entities.MasterData;

public class AssetStatusTransition
{
    public int Id { get; set; }
    
    public int FromStatusId { get; set; }
    
    public int ToStatusId { get; set; }
    
    public bool IsAllowed { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    // Navigation properties
    public virtual AssetStatus FromStatus { get; set; } = null!;
    public virtual AssetStatus ToStatus { get; set; } = null!;
}
