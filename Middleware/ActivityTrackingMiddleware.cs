using System.Security.Claims;
using ITAMS.Data;
using ITAMS.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Middleware
{
    public class ActivityTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActivityTrackingMiddleware> _logger;

        public ActivityTrackingMiddleware(
            RequestDelegate next,
            ILogger<ActivityTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITAMSDbContext dbContext)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

                    if (userIdClaim != null &&
                        int.TryParse(userIdClaim.Value, out int userId))
                    {
                        var user = await dbContext.Users
                            .Where(u => u.Id == userId && !string.IsNullOrEmpty(u.ActiveSessionId))
                            .FirstOrDefaultAsync();

                        if (user != null)
                        {
                            user.LastActivityAt = DateTimeHelper.Now;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating LastActivityAt");
                }
            }

            await _next(context);
        }
    }
}
