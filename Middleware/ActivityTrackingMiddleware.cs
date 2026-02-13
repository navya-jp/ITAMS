using System.Security.Claims;
using ITAMS.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Middleware;

public class ActivityTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ActivityTrackingMiddleware> _logger;

    public ActivityTrackingMiddleware(RequestDelegate next, ILogger<ActivityTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITAMSDbContext dbContext)
    {
        // Update user activity BEFORE processing the request if authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            try
            {
                var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(username))
                {
                    // Update LastActivityAt synchronously
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE Users SET LastActivityAt = GETUTCDATE() WHERE Username = {0}",
                        username
                    );
                    _logger.LogInformation("Updated LastActivityAt for user {Username}", username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating LastActivityAt");
            }
        }

        // Call the next middleware
        await _next(context);
    }
}
