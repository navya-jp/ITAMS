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
}
