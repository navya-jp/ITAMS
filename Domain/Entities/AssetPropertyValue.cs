namespace ITAMS.Domain.Entities;

public class AssetPropertyValue
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int PropertyId { get; set; }
    public string PropertyValue { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Asset? Asset { get; set; }
    public AssetPropertiesMaster? Property { get; set; }
}
