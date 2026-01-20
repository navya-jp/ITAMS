using ITAMS.Domain.Entities;

namespace ITAMS.Domain.Interfaces;

public interface IPermissionService
{
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task<IEnumerable<Permission>> GetPermissionsByModuleAsync(string module);
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId);
    Task<IEnumerable<Permission>> GetUserProjectPermissionsAsync(int userId, int projectId);
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId);
    Task<bool> HasPermissionAsync(int userId, string permissionCode);
    Task<bool> HasProjectPermissionAsync(int userId, int projectId, string permissionCode);
    Task UpdateRolePermissionsAsync(int roleId, int[] permissionIds);
}

public class UserPermissionDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public IEnumerable<Permission> Permissions { get; set; } = new List<Permission>();
}