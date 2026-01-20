using ITAMS.Domain.Entities;

namespace ITAMS.Domain.Interfaces;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(int id);
    Task<IEnumerable<Location>> GetAllAsync();
    Task<IEnumerable<Location>> GetByProjectIdAsync(int projectId);
    Task<Location> CreateAsync(Location location);
    Task<Location> UpdateAsync(Location location);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}