using ITAMS.Data;
using ITAMS.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SessionCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _forcedLogoutThreshold = TimeSpan.FromMinutes(2); // If no heartbeat for 2 minutes, browser likely closed

        public SessionCleanupService(
            IServiceProvider serviceProvider,
            ILogger<SessionCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupAbandonedSessions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during session cleanup");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Session Cleanup Service stopped");
        }

        private async Task CleanupAbandonedSessions()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITAMSDbContext>();

            var now = DateTimeHelper.Now;
            var forcedLogoutCutoff = now.AddMinutes(-2); // No heartbeat for 2 minutes = browser closed
            var sessionTimeoutCutoff = now.AddMinutes(-30); // Session started 30+ minutes ago = timeout

            // Find users with active sessions
            var activeUsers = await context.Users
                .Where(u =>
                    !string.IsNullOrEmpty(u.ActiveSessionId) &&
                    u.SessionStartedAt.HasValue)
                .ToListAsync();

            if (!activeUsers.Any())
                return;

            var cleanedCount = 0;

            foreach (var user in activeUsers)
            {
                // Find ACTIVE audit record
                var activeAudit = await context.LoginAudits
                    .Where(a => a.UserId == user.Id && a.Status == "ACTIVE")
                    .OrderByDescending(a => a.LoginTime)
                    .FirstOrDefaultAsync();

                if (activeAudit == null)
                    continue;

                var sessionDuration = now - user.SessionStartedAt.Value;
                var timeSinceLastHeartbeat = user.LastActivityAt.HasValue 
                    ? now - user.LastActivityAt.Value 
                    : sessionDuration;

                // Check for SESSION_TIMEOUT first (30 minutes from login)
                if (sessionDuration.TotalMinutes >= 30)
                {
                    activeAudit.Status = "SESSION_TIMEOUT";
                    activeAudit.LogoutTime = now;
                    
                    user.ActiveSessionId = null;
                    user.SessionStartedAt = null;
                    
                    cleanedCount++;
                    _logger.LogInformation(
                        "Session timeout for user {Username} - session duration: {Minutes:F1} minutes",
                        user.Username, sessionDuration.TotalMinutes
                    );
                }
                // Check for FORCED_LOGOUT (browser closed - no heartbeat for 2+ minutes)
                else if (timeSinceLastHeartbeat.TotalMinutes >= 2)
                {
                    activeAudit.Status = "FORCED_LOGOUT";
                    activeAudit.LogoutTime = now;
                    
                    user.ActiveSessionId = null;
                    user.SessionStartedAt = null;
                    
                    cleanedCount++;
                    _logger.LogInformation(
                        "Forced logout for user {Username} - no heartbeat for {Minutes:F1} minutes (browser closed)",
                        user.Username, timeSinceLastHeartbeat.TotalMinutes
                    );
                }
            }

            if (cleanedCount > 0)
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} stale sessions", cleanedCount);
            }
        }
    }
}
