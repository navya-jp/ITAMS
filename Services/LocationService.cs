using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;

namespace ITAMS.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAuditService _auditService;

    public LocationService(
        ILocationRepository locationRepository,
        IProjectRepository projectRepository,
        IAuditService auditService)
    {
        _locationRepository = locationRepository;
        _projectRepository = projectRepository;
        _auditService = auditService;
    }

    public async Task<Location> CreateLocationAsync(CreateLocationRequest request)
    {
        // Validate project exists
        var project = await _projectRepository.GetByIdAsync(request.ProjectId);
        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        // Generate LocationId (alternate key) - get the next available number
        var allLocations = await _locationRepository.GetAllAsync();
        var maxId = allLocations.Any() ? allLocations.Max(l => l.Id) : 0;
        var locationId = $"LOC{(maxId + 1):D5}"; // Format: LOC00001, LOC00002, etc.

        var location = new Location
        {
            LocationId = locationId,
            Name = request.Name,
            Region = request.Region,
            State = request.State,
            Site = request.Site,
            Lane = request.Lane,
            Office = request.Office,
            Address = request.Address,
            ProjectId = request.ProjectId,
            ProjectIdRef = project.ProjectId, // Set the alternate key reference
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdLocation = await _locationRepository.CreateAsync(location);
        await _auditService.LogAsync("LOCATION_CREATED", "Location", createdLocation.Id.ToString(), 1, "superadmin");
        
        return createdLocation;
    }

    public async Task<Location> UpdateLocationAsync(int id, UpdateLocationRequest request)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        if (location == null)
        {
            throw new InvalidOperationException("Location not found");
        }

        var oldValues = $"Name: {location.Name}, Region: {location.Region}, State: {location.State}, Site: {location.Site}, Lane: {location.Lane}, Office: {location.Office}, Address: {location.Address}, IsActive: {location.IsActive}";

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Name))
        {
            location.Name = request.Name;
        }

        if (!string.IsNullOrEmpty(request.Region))
        {
            location.Region = request.Region;
        }

        if (!string.IsNullOrEmpty(request.State))
        {
            location.State = request.State;
        }

        if (request.Site != null)
        {
            location.Site = request.Site;
        }

        if (request.Lane != null)
        {
            location.Lane = request.Lane;
        }

        if (request.Office != null)
        {
            location.Office = request.Office;
        }

        if (request.Address != null)
        {
            location.Address = request.Address;
        }

        if (request.IsActive.HasValue)
        {
            location.IsActive = request.IsActive.Value;
        }

        var updatedLocation = await _locationRepository.UpdateAsync(location);
        
        var newValues = $"Name: {location.Name}, Region: {location.Region}, State: {location.State}, Site: {location.Site}, Lane: {location.Lane}, Office: {location.Office}, Address: {location.Address}, IsActive: {location.IsActive}";
        await _auditService.LogAsync("LOCATION_UPDATED", "Location", location.Id.ToString(), 1, "superadmin", oldValues, newValues);
        
        return updatedLocation;
    }

    public async Task DeleteLocationAsync(int id)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        if (location == null)
        {
            throw new InvalidOperationException("Location not found");
        }

        // Check if location has assets
        if (location.Assets.Any())
        {
            throw new InvalidOperationException("Cannot delete location with existing assets");
        }

        await _locationRepository.DeleteAsync(id);
        await _auditService.LogAsync("LOCATION_DELETED", "Location", id.ToString(), 1, "superadmin");
    }

    public async Task<Location?> GetLocationByIdAsync(int id)
    {
        return await _locationRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Location>> GetAllLocationsAsync()
    {
        return await _locationRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Location>> GetLocationsByProjectAsync(int projectId)
    {
        return await _locationRepository.GetByProjectIdAsync(projectId);
    }
}