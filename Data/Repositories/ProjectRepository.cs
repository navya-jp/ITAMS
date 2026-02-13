using Microsoft.EntityFrameworkCore;
using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;

namespace ITAMS.Data.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ITAMSDbContext _context;

    public ProjectRepository(ITAMSDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Locations)
            .Include(p => p.UserProjects.Where(up => up.IsActive))
            .ThenInclude(up => up.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project?> GetByCodeAsync(string code)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Locations)
            .Include(p => p.UserProjects.Where(up => up.IsActive))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(int userId)
    {
        return await _context.Projects
            .Include(p => p.Locations)
            .Include(p => p.UserProjects.Where(up => up.IsActive))
            .Where(p => p.UserProjects.Any(up => up.UserId == userId && up.IsActive))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Project> CreateAsync(Project project)
    {
        // Generate ProjectId if not set
        if (string.IsNullOrEmpty(project.ProjectId))
        {
            var maxId = await _context.Projects
                .Where(p => !string.IsNullOrEmpty(p.ProjectId))
                .Select(p => p.ProjectId)
                .ToListAsync();
            
            int nextNumber = 1;
            if (maxId.Any())
            {
                // Extract numbers from ProjectIds like "PRJ00001", "PRJ00002"
                var numbers = maxId
                    .Where(id => id.StartsWith("PRJ"))
                    .Select(id => {
                        var numPart = id.Substring(3);
                        return int.TryParse(numPart, out int num) ? num : 0;
                    })
                    .Where(num => num > 0);
                
                if (numbers.Any())
                {
                    nextNumber = numbers.Max() + 1;
                }
            }
            
            project.ProjectId = $"PRJ{nextNumber:D5}";
        }
        
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task DeleteAsync(int id)
    {
        var project = await GetByIdAsync(id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Projects.AnyAsync(p => p.Id == id);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _context.Projects.Where(p => p.Code == code);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }
}