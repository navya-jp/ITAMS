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
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(2); // 2 minutes without heartbeat = forced logout

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

            var cutoffTime = DateTimeHelper.Now.Subtract(_sessionTimeout);

            // Find users with active sessions but no recent activity
            var staleUsers = await context.Users
                .Where(u => !string.IsNullOrEmpty(u.ActiveSessionId) &&
                           u.LastActivityAt.HasValue &&
                           u.LastActivityAt.Value < cutoffTime)
                .ToListAsync();

            if (staleUsers.Any())
            {
                _logger.LogInformation("Found {Count} stale sessions to clean up", staleUsers.Count);

                foreach (var user in staleUsers)
                {
                    // Find the active login audit record
                    var loginAudit = await context.LoginAudits
                        .Where(la => la.UserId == user.Id && la.Status == "ACTIVE")
                        .OrderByDescending(la => la.LoginTime)
                        .FirstOrDefaultAsync();

                    if (loginAudit != null)
                    {
                        loginAudit.LogoutTime = DateTimeHelper.Now;
                        loginAudit.Status = "FORCED_LOGOUT";
                        _logger.LogInformation("Marked session as FORCED_LOGOUT for user {Username} (UserId={UserId})", 
                            user.Username, user.Id);
                    }

                    // Clear the user's session
                    user.ActiveSessionId = null;
                    user.SessionStartedAt = null;
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
