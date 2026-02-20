using ITAMS.Data;
using Microsoft.EntityFrameworkCore;
using ITAMS.Utilities;

namespace ITAMS.Services
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SessionCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30); // 30 minutes without activity = session timeout
        private readonly TimeSpan _forcedLogoutThreshold = TimeSpan.FromMinutes(2); // 2 minutes without heartbeat = forced logout (browser closed without logout)

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
                    await CleanupStaleSessions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Session Cleanup Service");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Session Cleanup Service stopped");
        }

        private async Task CleanupStaleSessions()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITAMSDbContext>();

            var now = DateTimeHelper.Now;
            var sessionTimeoutCutoff = now.Subtract(_sessionTimeout);
            var forcedLogoutCutoff = now.Subtract(_forcedLogoutThreshold);

            // Find users with active sessions but no recent activity
            var staleUsers = await context.Users
                .Where(u => !string.IsNullOrEmpty(u.ActiveSessionId) &&
                           u.LastActivityAt.HasValue)
                .ToListAsync();

            if (staleUsers.Any())
            {
                foreach (var user in staleUsers)
                {
                    var timeSinceActivity = now - user.LastActivityAt.Value;
                    
                    // Skip if session is still active (within 30 minutes)
                    if (timeSinceActivity.TotalMinutes < 30)
                    {
                        continue;
                    }

                    // Find the active login audit record
                    var loginAudit = await context.LoginAudits
                        .Where(la => la.UserId == user.Id && la.Status == "ACTIVE")
                        .OrderByDescending(la => la.LoginTime)
                        .FirstOrDefaultAsync();

                    if (loginAudit != null)
                    {
                        loginAudit.LogoutTime = now;
                        
                        // Determine logout type based on time since last activity:
                        // - FORCED_LOGOUT: 2-5 minutes (browser closed without logout, heartbeat stopped)
                        // - SESSION_TIMEOUT: 30+ minutes (automatic timeout due to inactivity)
                        if (timeSinceActivity.TotalMinutes >= 2 && timeSinceActivity.TotalMinutes < 5)
                        {
                            // Browser closed without clicking logout (heartbeat stopped suddenly)
                            loginAudit.Status = "FORCED_LOGOUT";
                            _logger.LogInformation("Marked session as FORCED_LOGOUT for user {Username} (UserId={UserId}) - last activity {Minutes:F1} minutes ago", 
                                user.Username, user.Id, timeSinceActivity.TotalMinutes);
                        }
                        else
                        {
                            // Session timeout due to 30 minutes of inactivity
                            loginAudit.Status = "SESSION_TIMEOUT";
                            _logger.LogInformation("Marked session as SESSION_TIMEOUT for user {Username} (UserId={UserId}) - last activity {Minutes:F1} minutes ago", 
                                user.Username, user.Id, timeSinceActivity.TotalMinutes);
                        }
                    }

                    // Clear the user's session
                    user.ActiveSessionId = null;
                    user.SessionStartedAt = null;
                }

                if (staleUsers.Any(u => u.ActiveSessionId == null))
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Cleaned up {Count} stale sessions", 
                        staleUsers.Count(u => u.ActiveSessionId == null));
                }
            }
        }
    }
}
