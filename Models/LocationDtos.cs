using System.ComponentModel.DataAnnotations;

namespace ITAMS.Models;

public class CreateLocationDto
{
    [Required(ErrorMessage = "Location name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Location name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Project is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid project")]
    public int ProjectId { get; set; }
    
    [Required(ErrorMessage = "Region is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Region must be between 2 and 100 characters")]
    public string Region { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "State is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "State must be between 2 and 100 characters")]
    public string State { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Site cannot exceed 100 characters")]
    public string? Site { get; set; } // Changed from Plaza to Site
    
    [StringLength(100, ErrorMessage = "Lane cannot exceed 100 characters")]
    public string? Lane { get; set; }
    
    [StringLength(100, ErrorMessage = "Office cannot exceed 100 characters")]
    public string? Office { get; set; }
    
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
}

public class UpdateLocationDto
{
    [Required(ErrorMessage = "Location name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Location name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Region is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Region must be between 2 and 100 characters")]
    public string Region { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "State is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "State must be between 2 and 100 characters")]
    public string State { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Site cannot exceed 100 characters")]
    public string? Site { get; set; } // Changed from Plaza to Site
    
    [StringLength(100, ErrorMessage = "Lane cannot exceed 100 characters")]
    public string? Lane { get; set; }
    
    [StringLength(100, ErrorMessage = "Office cannot exceed 100 characters")]
    public string? Office { get; set; }
    
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class LocationSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Site { get; set; } // Changed from Plaza to Site
    public string? Lane { get; set; }
    public string? Office { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public int ProjectId { get; set; }
    public int AssetCount { get; set; }
}

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Site { get; set; } // Changed from Plaza to Site
    public string? Lane { get; set; }
    public string? Office { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int AssetCount { get; set; }
}