using ITAMS.Domain.Entities;

namespace ITAMS.Domain.Interfaces;

public interface ILocationService
{
    Task<Location> CreateLocationAsync(CreateLocationRequest request);
    Task<Location> UpdateLocationAsync(int id, UpdateLocationRequest request);
    Task DeleteLocationAsync(int id);
    Task<Location?> GetLocationByIdAsync(int id);
    Task<IEnumerable<Location>> GetAllLocationsAsync();
    Task<IEnumerable<Location>> GetLocationsByProjectAsync(int projectId);
}

public class CreateLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Site { get; set; }
    public string? Lane { get; set; }
    public string? Office { get; set; }
    public string? Address { get; set; }
    public int ProjectId { get; set; }
}

public class UpdateLocationRequest
{
    public string? Name { get; set; }
    public string? Region { get; set; }
    public string? State { get; set; }
    public string? Site { get; set; }
    public string? Lane { get; set; }
    public string? Office { get; set; }
    public string? Address { get; set; }
    public bool? IsActive { get; set; }
}