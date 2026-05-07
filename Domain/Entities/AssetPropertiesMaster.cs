namespace ITAMS.Domain.Entities;

public class AssetPropertiesMaster
{
    public int Id { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string? ApplicableSubtype { get; set; }
    public string DataType { get; set; } = "Text";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public ICollection<AssetPropertyValue> Values { get; set; } = new List<AssetPropertyValue>();
}
