using ITAMS.Domain.Entities;
using ITAMS.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services;

public interface IAccessControlService
{
    Task<int?> GetUserProjectId(int userId);
    Task<bool> CanAccessProject(int userId, int projectId);
    Task<bool> CanAccessLocation(int userId, int locationId);
    Task<IQueryable<T>> ApplyProjectFilter<T>(IQueryable<T> query, int userId) where T : class;
    Task<IQueryable<Location>> ApplyLocationFilter(IQueryable<Location> query, int userId);
    bool IsSuperAdmin(int userId, int roleId);
}

public class AccessControlService : IAccessControlService
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<AccessControlService> _logger;

    public AccessControlService(ITAMSDbContext context, ILogger<AccessControlService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public bool IsSuperAdmin(int userId, int roleId)
    {
        // RoleId = 1 is SuperAdmin
        return roleId == 1;
    }

    public async Task<int?> GetUserProjectId(int userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.ProjectId, u.RoleId })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return null;
        }

        // SuperAdmin has no project restriction
        if (IsSuperAdmin(userId, user.RoleId))
        {
            return null; // null means access to all projects
        }

        return user.ProjectId;
    }

    public async Task<bool> CanAccessProject(int userId, int projectId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.ProjectId, u.RoleId })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return false;
        }

        // SuperAdmin can access all projects
        if (IsSuperAdmin(userId, user.RoleId))
        {
            return true;
        }

        // User can only access their assigned project
        return user.ProjectId == projectId;
    }

    public async Task<bool> CanAccessLocation(int userId, int locationId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { 
                u.ProjectId, 
                u.RoleId,
                u.RestrictedRegion,
                u.RestrictedState,
                u.RestrictedPlaza,
                u.RestrictedOffice
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return false;
        }

        // SuperAdmin can access all locations
        if (IsSuperAdmin(userId, user.RoleId))
        {
            return true;
        }

        var location = await _context.Locations
            .Where(l => l.Id == locationId)
            .Select(l => new {
                l.ProjectId,
                l.Region,
                l.State,
                l.Site,
                l.Office
            })
            .FirstOrDefaultAsync();

        if (location == null)
        {
            _logger.LogWarning("Location {LocationId} not found", locationId);
            return false;
        }

        // Check project isolation first
        if (location.ProjectId != user.ProjectId)
        {
            _logger.LogWarning("User {UserId} attempted to access location {LocationId} from different project", userId, locationId);
            return false;
        }

        // If no location restrictions, user has full access within their project
        if (string.IsNullOrEmpty(user.RestrictedOffice) && 
            string.IsNullOrEmpty(user.RestrictedPlaza) && 
            string.IsNullOrEmpty(user.RestrictedState) && 
            string.IsNullOrEmpty(user.RestrictedRegion))
        {
            return true;
        }

        // Apply hierarchical location filtering
        // Most specific restriction takes precedence

        // Office level (most specific)
        if (!string.IsNullOrEmpty(user.RestrictedOffice))
        {
            return location.Office == user.RestrictedOffice;
        }

        // Plaza level
        if (!string.IsNullOrEmpty(user.RestrictedPlaza))
        {
            return location.Site == user.RestrictedPlaza;
        }

        // State level
        if (!string.IsNullOrEmpty(user.RestrictedState))
        {
            return location.State == user.RestrictedState;
        }

        // Region level (least specific)
        if (!string.IsNullOrEmpty(user.RestrictedRegion))
        {
            return location.Region == user.RestrictedRegion;
        }

        return true;
    }

    public async Task<IQueryable<T>> ApplyProjectFilter<T>(IQueryable<T> query, int userId) where T : class
    {
        var userProjectId = await GetUserProjectId(userId);

        // SuperAdmin (userProjectId == null) sees all projects
        if (userProjectId == null)
        {
            return query;
        }

        // Filter by ProjectId property if it exists
        var entityType = typeof(T);
        var projectIdProperty = entityType.GetProperty("ProjectId");

        if (projectIdProperty != null)
        {
            // Use reflection to filter by ProjectId
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, "ProjectId");
            var constant = System.Linq.Expressions.Expression.Constant(userProjectId.Value);
            var equality = System.Linq.Expressions.Expression.Equal(property, constant);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equality, parameter);

            return query.Where(lambda);
        }

        _logger.LogWarning("Entity type {EntityType} does not have ProjectId property", entityType.Name);
        return query;
    }

    public async Task<IQueryable<Location>> ApplyLocationFilter(IQueryable<Location> query, int userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { 
                u.ProjectId, 
                u.RoleId,
                u.RestrictedRegion,
                u.RestrictedState,
                u.RestrictedPlaza,
                u.RestrictedOffice
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return query.Where(l => false); // Return empty query
        }

        // SuperAdmin sees all locations
        if (IsSuperAdmin(userId, user.RoleId))
        {
            return query;
        }

        // Filter by project first (mandatory)
        query = query.Where(l => l.ProjectId == user.ProjectId);

        // Apply hierarchical location filtering
        if (!string.IsNullOrEmpty(user.RestrictedOffice))
        {
            query = query.Where(l => l.Office == user.RestrictedOffice);
        }
        else if (!string.IsNullOrEmpty(user.RestrictedPlaza))
        {
            query = query.Where(l => l.Site == user.RestrictedPlaza);
        }
        else if (!string.IsNullOrEmpty(user.RestrictedState))
        {
            query = query.Where(l => l.State == user.RestrictedState);
        }
        else if (!string.IsNullOrEmpty(user.RestrictedRegion))
        {
            query = query.Where(l => l.Region == user.RestrictedRegion);
        }

        return query;
    }
}
