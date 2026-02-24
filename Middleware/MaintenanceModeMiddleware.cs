using ITAMS.Services;
using System.Security.Claims;

namespace ITAMS.Middleware;

public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MaintenanceModeMiddleware> _logger;

    public MaintenanceModeMiddleware(RequestDelegate next, ILogger<MaintenanceModeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISettingsService settingsService)
    {
        // Skip maintenance check for certain paths
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Allow access to login, settings API, and static files
        if (path.Contains("/api/auth/login") || 
            path.Contains("/api/settings") ||
            path.Contains("/api/auth/settings") ||
            path.Contains("/api/auth/maintenance-status") ||
            path.StartsWith("/assets") ||
            path == "/" ||
            path == "/login")
        {
            await _next(context);
            return;
        }

        // Check if maintenance mode is enabled
        var isMaintenanceMode = await settingsService.IsMaintenanceModeAsync();
        
        if (isMaintenanceMode)
        {
            // Check if user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Maintenance mode active - blocking unauthenticated request to {Path}", path);
                
                context.Response.StatusCode = 503;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "System is currently under maintenance. Please try again later.",
                    maintenanceMode = true
                });
                return;
            }

            // Check if user is a Super Admin
            var userRole = context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var isSuperAdmin = userRole == "Super Admin";
            
            _logger.LogInformation("Maintenance mode check - User: {User}, Role: {Role}, IsSuperAdmin: {IsSuperAdmin}, Path: {Path}", 
                context.User?.Identity?.Name ?? "Unknown", 
                userRole ?? "No Role", 
                isSuperAdmin,
                path);
            
            if (!isSuperAdmin)
            {
                _logger.LogWarning("Maintenance mode active - blocking access for non-SuperAdmin user: {User} with role: {Role}", 
                    context.User?.Identity?.Name ?? "Anonymous",
                    userRole ?? "No Role");
                
                context.Response.StatusCode = 503;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "System is currently under maintenance. Please try again later.",
                    maintenanceMode = true
                });
                return;
            }
            
            // Super Admin - allow all access
            _logger.LogInformation("Maintenance mode active - allowing Super Admin access: {User}", 
                context.User?.Identity?.Name ?? "Unknown");
        }

        await _next(context);
    }
}
