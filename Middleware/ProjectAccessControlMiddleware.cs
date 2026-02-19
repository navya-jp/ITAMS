using System.Security.Claims;
using ITAMS.Services;

namespace ITAMS.Middleware;

/// <summary>
/// Middleware to enforce project-based access control on all API requests
/// </summary>
public class ProjectAccessControlMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProjectAccessControlMiddleware> _logger;

    public ProjectAccessControlMiddleware(RequestDelegate next, ILogger<ProjectAccessControlMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAccessControlService accessControlService)
    {
        // Skip access control for authentication endpoints
        if (context.Request.Path.StartsWithSegments("/api/auth") || 
            context.Request.Path.StartsWithSegments("/api/superadmin/login"))
        {
            await _next(context);
            return;
        }

        // Only apply to authenticated requests
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            var roleIdClaim = context.User.FindFirst("RoleId");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                // Store user context in HttpContext.Items for use in controllers/services
                context.Items["UserId"] = userId;
                
                if (roleIdClaim != null && int.TryParse(roleIdClaim.Value, out int roleId))
                {
                    context.Items["RoleId"] = roleId;
                    context.Items["IsSuperAdmin"] = accessControlService.IsSuperAdmin(userId, roleId);
                }

                // Get user's project ID
                var projectId = await accessControlService.GetUserProjectId(userId);
                if (projectId.HasValue)
                {
                    context.Items["ProjectId"] = projectId.Value;
                }

                _logger.LogDebug("Access control context set for user {UserId}, project {ProjectId}", 
                    userId, projectId);
            }
        }

        await _next(context);
    }
}
