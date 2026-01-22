using System.ComponentModel.DataAnnotations;

namespace ITAMS.Models;

public class CreateProjectDto
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Project name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Project code is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Project code must be between 2 and 50 characters")]
    [RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "Project code can only contain uppercase letters, numbers, hyphens, and underscores")]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class UpdateProjectDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
}

public class ProjectSummaryDto
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

public class AssignUserProjectDto
{
    public int[] PermissionIds { get; set; } = Array.Empty<int>();
    public int AssignedBy { get; set; }
}