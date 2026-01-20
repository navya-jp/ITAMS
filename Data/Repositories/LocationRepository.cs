using Microsoft.EntityFrameworkCore;
using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;

namespace ITAMS.Data.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly ITAMSDbContext _context;

    public LocationRepository(ITAMSDbContext context)
    {
        _context = context;
    }

    public async Task<Location?> GetByIdAsync(int id)
    {
        return await _context.Locations
            .Include(l => l.Project)
            .Include(l => l.Assets)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Location>> GetAllAsync()
    {
        return await _context.Locations
            .Include(l => l.Project)
            .Include(l => l.Assets)
            .OrderBy(l => l.Region)
            .ThenBy(l => l.State)
            .ThenBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Location>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Locations
            .Include(l => l.Project)
            .Include(l => l.Assets)
            .Where(l => l.ProjectId == projectId)
            .OrderBy(l => l.Region)
            .ThenBy(l => l.State)
            .ThenBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<Location> CreateAsync(Location location)
    {
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        
        // Load related data
        await _context.Entry(location)
            .Reference(l => l.Project)
            .LoadAsync();
            
        return location;
    }

    public async Task<Location> UpdateAsync(Location location)
    {
        _context.Locations.Update(location);
        await _context.SaveChangesAsync();
        return location;
    }

    public async Task DeleteAsync(int id)
    {
        var location = await GetByIdAsync(id);
        if (location != null)
        {
            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Locations.AnyAsync(l => l.Id == id);
    }
}