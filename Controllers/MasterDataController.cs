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
}
