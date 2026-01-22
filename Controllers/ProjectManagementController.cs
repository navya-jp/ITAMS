using Microsoft.AspNetCore.Mvc;
using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;
using ITAMS.Models;

namespace ITAMS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectManagementController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILocationService _locationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuditService _auditService;

    public ProjectManagementController(
        IProjectService projectService,
        ILocationService locationService,
        IPermissionService permissionService,
        IAuditService auditService)
    {
        _projectService = projectService;
        _locationService = locationService;
        _permissionService = permissionService;
        _auditService = auditService;
    }

    #region Projects

    [HttpGet("projects")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAllProjects()
    {
        try
        {
            var projects = await _projectService.GetAllProjectsAsync();
            var projectDtos = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Code = p.Code,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                LocationCount = p.Locations.Count,
                UserCount = p.UserProjects.Count(up => up.IsActive)
            });

            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving projects", error = ex.Message });
        }
    }

    [HttpGet("projects/{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        try
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Code = project.Code,
                IsActive = project.IsActive,
                CreatedAt = project.CreatedAt,
                LocationCount = project.Locations.Count,
                UserCount = project.UserProjects.Count(up => up.IsActive)
            };

            return Ok(projectDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the project", error = ex.Message });
        }
    }

    [HttpPost("projects")]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createProjectDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new CreateProjectRequest
            {
                Name = createProjectDto.Name,
                Description = createProjectDto.Description ?? string.Empty,
                Code = createProjectDto.Code
            };

            var project = await _projectService.CreateProjectAsync(request);

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Code = project.Code,
                IsActive = project.IsActive,
                CreatedAt = project.CreatedAt,
                LocationCount = 0,
                UserCount = 0
            };

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, projectDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the project", error = ex.Message });
        }
    }

    [HttpPut("projects/{id}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] UpdateProjectDto updateProjectDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new UpdateProjectRequest
            {
                Name = updateProjectDto.Name,
                Description = updateProjectDto.Description,
                Code = updateProjectDto.Code,
                IsActive = updateProjectDto.IsActive
            };

            var project = await _projectService.UpdateProjectAsync(id, request);

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Code = project.Code,
                IsActive = project.IsActive,
                CreatedAt = project.CreatedAt,
                LocationCount = project.Locations.Count,
                UserCount = project.UserProjects.Count(up => up.IsActive)
            };

            return Ok(projectDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the project", error = ex.Message });
        }
    }

    [HttpDelete("projects/{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        try
        {
            await _projectService.DeleteProjectAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the project", error = ex.Message });
        }
    }

    #endregion

    #region Locations

    [HttpGet("projects/{projectId}/locations")]
    public async Task<ActionResult<IEnumerable<LocationDto>>> GetProjectLocations(int projectId)
    {
        try
        {
            var locations = await _locationService.GetLocationsByProjectAsync(projectId);
            var locationDtos = locations.Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Region = l.Region,
                State = l.State,
                Plaza = l.Plaza,
                Lane = l.Lane,
                Office = l.Office,
                Address = l.Address,
                IsActive = l.IsActive,
                ProjectId = l.ProjectId,
                ProjectName = l.Project.Name,
                AssetCount = l.Assets.Count
            });

            return Ok(locationDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving locations", error = ex.Message });
        }
    }

    [HttpPost("locations")]
    public async Task<ActionResult<LocationDto>> CreateLocation([FromBody] CreateLocationDto createLocationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new CreateLocationRequest
            {
                Name = createLocationDto.Name,
                Region = createLocationDto.Region,
                State = createLocationDto.State,
                Plaza = createLocationDto.Plaza,
                Lane = createLocationDto.Lane,
                Office = createLocationDto.Office,
                Address = createLocationDto.Address,
                ProjectId = createLocationDto.ProjectId
            };

            var location = await _locationService.CreateLocationAsync(request);

            var locationDto = new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Region = location.Region,
                State = location.State,
                Plaza = location.Plaza,
                Lane = location.Lane,
                Office = location.Office,
                Address = location.Address,
                IsActive = location.IsActive,
                ProjectId = location.ProjectId,
                ProjectName = location.Project.Name,
                AssetCount = 0
            };

            return CreatedAtAction(nameof(GetProject), new { id = location.Id }, locationDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the location", error = ex.Message });
        }
    }

    #endregion

    #region User Project Assignments

    [HttpPost("projects/{projectId}/users/{userId}/assign")]
    public async Task<ActionResult> AssignUserToProject(int projectId, int userId, [FromBody] AssignUserProjectDto assignDto)
    {
        try
        {
            await _projectService.AssignUserToProjectAsync(userId, projectId, assignDto.PermissionIds, assignDto.AssignedBy);
            return Ok(new { message = "User assigned to project successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while assigning user to project", error = ex.Message });
        }
    }

    [HttpDelete("projects/{projectId}/users/{userId}")]
    public async Task<ActionResult> RemoveUserFromProject(int projectId, int userId)
    {
        try
        {
            await _projectService.RemoveUserFromProjectAsync(userId, projectId);
            return Ok(new { message = "User removed from project successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while removing user from project", error = ex.Message });
        }
    }

    [HttpGet("users/{userId}/projects")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetUserProjects(int userId)
    {
        try
        {
            var projects = await _projectService.GetUserProjectsAsync(userId);
            var projectDtos = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Code = p.Code,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                LocationCount = p.Locations.Count,
                UserCount = p.UserProjects.Count(up => up.IsActive)
            });

            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user projects", error = ex.Message });
        }
    }

    #endregion

    #region Permissions

    [HttpGet("permissions")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetAllPermissions()
    {
        try
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving permissions", error = ex.Message });
        }
    }

    [HttpGet("permissions/modules/{module}")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetPermissionsByModule(string module)
    {
        try
        {
            var permissions = await _permissionService.GetPermissionsByModuleAsync(module);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving permissions", error = ex.Message });
        }
    }

    #endregion
}

// DTOs
public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LocationCount { get; set; }
    public int UserCount { get; set; }
}