using ITAMS.Domain.Entities;

namespace ITAMS.Domain.Interfaces;

public interface IProjectService
{
    Task<Project> CreateProjectAsync(CreateProjectRequest request);
    Task<Project> UpdateProjectAsync(int id, UpdateProjectRequest request);
    Task DeleteProjectAsync(int id);
    Task<Project?> GetProjectByIdAsync(int id);
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<IEnumerable<Project>> GetUserProjectsAsync(int userId);
    Task AssignUserToProjectAsync(int userId, int projectId, int[] permissionIds, int assignedBy);
    Task RemoveUserFromProjectAsync(int userId, int projectId);
    Task UpdateUserProjectPermissionsAsync(int userId, int projectId, int[] permissionIds, int updatedBy);
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class UpdateProjectRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
}