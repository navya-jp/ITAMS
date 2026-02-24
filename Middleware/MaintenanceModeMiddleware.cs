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
            // Allow Super Admins to access during maintenance
            var userRole = context.User?.FindFirst(ClaimTypes.Role)?.Value;
            
            if (userRole != "Super Admin")
            {
                _logger.LogWarning("Maintenance mode active - blocking access for user: {User}", 
                    context.User?.Identity?.Name ?? "Anonymous");
                
                context.Response.StatusCode = 503; // Service Unavailable
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "System is currently under maintenance. Please try again later.",
                    maintenanceMode = true
                });
                return;
            }
        }

        await _next(context);
    }
}
