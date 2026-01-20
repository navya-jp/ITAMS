using ITAMS.Domain.Entities;

namespace ITAMS.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<Project?> GetByCodeAsync(string code);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetUserProjectsAsync(int userId);
    Task<Project> CreateAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
}