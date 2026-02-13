using Microsoft.AspNetCore.Mvc;
using ITAMS.Services;

namespace ITAMS.Controllers;

/// <summary>
/// Base controller with access control helpers
/// </summary>
public abstract class BaseController : ControllerBase
{
    protected int? GetCurrentUserId()
    {
        if (HttpContext.Items.TryGetValue("UserId", out var userId))
        {
            return (int)userId;
        }
        return null;
    }

    protected int? GetCurrentUserProjectId()
    {
        if (HttpContext.Items.TryGetValue("ProjectId", out var projectId))
        {
            return (int)projectId;
        }
        return null;
    }

    protected bool IsSuperAdmin()
    {
        if (HttpContext.Items.TryGetValue("IsSuperAdmin", out var isSuperAdmin))
        {
            return (bool)isSuperAdmin;
        }
        return false;
    }

    protected int? GetCurrentUserRoleId()
    {
        if (HttpContext.Items.TryGetValue("RoleId", out var roleId))
        {
            return (int)roleId;
        }
        return null;
    }

    /// <summary>
    /// Verify user can access the specified project
    /// </summary>
    protected async Task<bool> CanAccessProject(int projectId, IAccessControlService accessControlService)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return false;
        }

        return await accessControlService.CanAccessProject(userId.Value, projectId);
    }

    /// <summary>
    /// Verify user can access the specified location
    /// </summary>
    protected async Task<bool> CanAccessLocation(int locationId, IAccessControlService accessControlService)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return false;
        }

        return await accessControlService.CanAccessLocation(userId.Value, locationId);
    }

    /// <summary>
    /// Return 403 Forbidden with message
    /// </summary>
    protected ActionResult Forbidden(string message = "Access denied")
    {
        return StatusCode(403, new { message });
    }
}
