using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAMS.Data;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MasterDataController : BaseController
{
    private readonly ITAMSDbContext _context;

    public MasterDataController(ITAMSDbContext context)
    {
        _context = context;
    }

    [HttpGet("service-types")]
    public async Task<IActionResult> GetServiceTypes()
    {
        var types = await _context.ServiceTypes
            .OrderBy(t => t.TypeName)
            .Select(t => new { t.Id, t.TypeName, t.Description })
            .ToListAsync();
        return Ok(types);
    }

    [HttpGet("asset-types")]
    public async Task<IActionResult> GetAssetTypes()
    {
        var types = await _context.AssetTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.TypeName)
            .Select(t => new { t.Id, t.TypeName, t.TypeCode, t.CategoryId })
            .ToListAsync();
        return Ok(types);
    }

    [HttpGet("asset-subtypes")]
    public async Task<IActionResult> GetAssetSubTypes([FromQuery] int? typeId)
    {
        var query = _context.AssetSubTypes
            .Where(s => s.IsActive)
            .AsQueryable();
        if (typeId.HasValue)
            query = query.Where(s => s.TypeId == typeId.Value);
        var subtypes = await query
            .OrderBy(s => s.SubTypeName)
            .Select(s => new { s.Id, s.SubTypeName, s.SubTypeCode, s.TypeId })
            .ToListAsync();
        return Ok(subtypes);
    }

    [HttpGet("asset-properties")]
    public async Task<IActionResult> GetAssetProperties([FromQuery] string? subtype)
    {
        var query = _context.AssetPropertiesMaster.Where(p => p.IsActive);
        if (!string.IsNullOrEmpty(subtype))
            query = query.Where(p => p.ApplicableSubtype == null || p.ApplicableSubtype.ToLower() == subtype.ToLower());
        var props = await query.OrderBy(p => p.PropertyName)
            .Select(p => new { p.Id, p.PropertyName, p.ApplicableSubtype, p.DataType })
            .ToListAsync();
        return Ok(props);
    }

    [HttpPost("asset-properties")]
    public async Task<IActionResult> CreateAssetProperty([FromBody] CreatePropertyDto dto)
    {
        var prop = new ITAMS.Domain.Entities.AssetPropertiesMaster
        {
            PropertyName = dto.PropertyName,
            ApplicableSubtype = dto.ApplicableSubtype,
            DataType = dto.DataType ?? "Text",
            IsActive = true,
            CreatedAt = ITAMS.Utilities.DateTimeHelper.Now,
            CreatedBy = GetCurrentUserId() ?? 1
        };
        _context.AssetPropertiesMaster.Add(prop);
        await _context.SaveChangesAsync();
        return Ok(new { prop.Id, prop.PropertyName, prop.ApplicableSubtype, prop.DataType });
    }

    [HttpGet("asset-property-values/{assetId}")]
    public async Task<IActionResult> GetAssetPropertyValues(int assetId)
    {
        var values = await _context.AssetPropertyValues
            .Include(v => v.Property)
            .Where(v => v.AssetId == assetId)
            .Select(v => new { v.Id, v.AssetId, v.PropertyId, PropertyName = v.Property!.PropertyName, v.PropertyValue })
            .ToListAsync();
        return Ok(values);
    }

    [HttpPost("asset-property-values")]
    public async Task<IActionResult> SaveAssetPropertyValues([FromBody] SavePropertyValuesDto dto)
    {
        // Remove existing values for this asset
        var existing = _context.AssetPropertyValues.Where(v => v.AssetId == dto.AssetId);
        _context.AssetPropertyValues.RemoveRange(existing);

        // Add new values
        foreach (var item in dto.Values)
        {
            if (string.IsNullOrWhiteSpace(item.PropertyValue)) continue;
            _context.AssetPropertyValues.Add(new ITAMS.Domain.Entities.AssetPropertyValue
            {
                AssetId = dto.AssetId,
                PropertyId = item.PropertyId,
                PropertyValue = item.PropertyValue,
                CreatedAt = ITAMS.Utilities.DateTimeHelper.Now
            });
        }
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}

public class CreatePropertyDto
{
    public string PropertyName { get; set; } = string.Empty;
    public string? ApplicableSubtype { get; set; }
    public string? DataType { get; set; }
}

public class SavePropertyValuesDto
{
    public int AssetId { get; set; }
    public List<PropertyValueItem> Values { get; set; } = new();
}

public class PropertyValueItem
{
    public int PropertyId { get; set; }
    public string PropertyValue { get; set; } = string.Empty;
}
