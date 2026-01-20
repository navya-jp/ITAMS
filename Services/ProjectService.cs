using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;
using ITAMS.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ITAMSDbContext _context;

    public ProjectService(
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IAuditService auditService,
        ITAMSDbContext context)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _auditService = auditService;
        _context = context;
    }

    public async Task<Project> CreateProjectAsync(CreateProjectRequest request)
    {
        // Validate code uniqueness
        if (await _projectRepository.CodeExistsAsync(request.Code))
        {
            throw new InvalidOperationException("Project code already exists");
        }

        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            Code = request.Code,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1 // TODO: Get from current user context
        };

        var createdProject = await _projectRepository.CreateAsync(project);
        await _auditService.LogAsync("PROJECT_CREATED", "Project", createdProject.Id.ToString(), 1, "superadmin");
        
        return createdProject;
    }

    public async Task<Project> UpdateProjectAsync(int id, UpdateProjectRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        var oldValues = $"Name: {project.Name}, Description: {project.Description}, Code: {project.Code}, IsActive: {project.IsActive}";

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Name))
        {
            project.Name = request.Name;
        }

        if (request.Description != null)
        {
            project.Description = request.Description;
        }

        if (!string.IsNullOrEmpty(request.Code))
        {
            if (await _projectRepository.CodeExistsAsync(request.Code, id))
            {
                throw new InvalidOperationException("Project code already exists");
            }
            project.Code = request.Code;
        }

        if (request.IsActive.HasValue)
        {
            project.IsActive = request.IsActive.Value;
        }

        var updatedProject = await _projectRepository.UpdateAsync(project);
        
        var newValues = $"Name: {project.Name}, Description: {project.Description}, Code: {project.Code}, IsActive: {project.IsActive}";
        await _auditService.LogAsync("PROJECT_UPDATED", "Project", project.Id.ToString(), 1, "superadmin", oldValues, newValues);
        
        return updatedProject;
    }

    public async Task DeleteProjectAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        // Check if project has locations or assets
        if (project.Locations.Any())
        {
            throw new InvalidOperationException("Cannot delete project with existing locations");
        }

        await _projectRepository.DeleteAsync(id);
        await _auditService.LogAsync("PROJECT_DELETED", "Project", id.ToString(), 1, "superadmin");
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        return await _projectRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await _projectRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(int userId)
    {
        return await _projectRepository.GetUserProjectsAsync(userId);
    }

    public async Task AssignUserToProjectAsync(int userId, int projectId, int[] permissionIds, int assignedBy)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        // Check if user is already assigned to project
        var existingAssignment = await _context.UserProjects
            .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId);

        if (existingAssignment != null)
        {
            if (existingAssignment.IsActive)
            {
                throw new InvalidOperationException("User is already assigned to this project");
            }
            else
            {
                // Reactivate existing assignment
                existingAssignment.IsActive = true;
                existingAssignment.AssignedAt = DateTime.UtcNow;
                existingAssignment.AssignedBy = assignedBy;
                _context.UserProjects.Update(existingAssignment);
            }
        }
        else
        {
            // Create new assignment
            var userProject = new UserProject
            {
                UserId = userId,
                ProjectId = projectId,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = assignedBy
            };

            _context.UserProjects.Add(userProject);
            await _context.SaveChangesAsync();
            existingAssignment = userProject;
        }

        // Assign permissions
        await UpdateUserProjectPermissionsAsync(userId, projectId, permissionIds, assignedBy);

        await _auditService.LogAsync("USER_ASSIGNED_TO_PROJECT", "UserProject", 
            $"{userId}-{projectId}", assignedBy, "superadmin");
    }

    public async Task RemoveUserFromProjectAsync(int userId, int projectId)
    {
        var userProject = await _context.UserProjects
            .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId && up.IsActive);

        if (userProject == null)
        {
            throw new InvalidOperationException("User is not assigned to this project");
        }

        userProject.IsActive = false;
        _context.UserProjects.Update(userProject);

        // Remove all project permissions
        var permissions = await _context.UserProjectPermissions
            .Where(upp => upp.UserProjectId == userProject.Id)
            .ToListAsync();

        _context.UserProjectPermissions.RemoveRange(permissions);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("USER_REMOVED_FROM_PROJECT", "UserProject", 
            $"{userId}-{projectId}", 1, "superadmin");
    }

    public async Task UpdateUserProjectPermissionsAsync(int userId, int projectId, int[] permissionIds, int updatedBy)
    {
        var userProject = await _context.UserProjects
            .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId && up.IsActive);

        if (userProject == null)
        {
            throw new InvalidOperationException("User is not assigned to this project");
        }

        // Remove existing permissions
        var existingPermissions = await _context.UserProjectPermissions
            .Where(upp => upp.UserProjectId == userProject.Id)
            .ToListAsync();

        _context.UserProjectPermissions.RemoveRange(existingPermissions);

        // Add new permissions
        var newPermissions = permissionIds.Select(permissionId => new UserProjectPermission
        {
            UserProjectId = userProject.Id,
            PermissionId = permissionId,
            IsGranted = true,
            CreatedAt = DateTime.UtcNow,
            GrantedBy = updatedBy
        }).ToList();

        _context.UserProjectPermissions.AddRange(newPermissions);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("USER_PROJECT_PERMISSIONS_UPDATED", "UserProjectPermission", 
            $"{userId}-{projectId}", updatedBy, "superadmin");
    }
}