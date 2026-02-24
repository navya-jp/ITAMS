-- Check current session data for superadmin
SELECT 
    Id,
    UserId,
    Username,
    ActiveSessionId,
    SessionStartedAt,
    LastActivityAt,
    LastLoginAt
FROM Users 
WHERE Username = 'superadmin';

-- Check all active sessions
SELECT 
    Username,
    ActiveSessionId,
    SessionStartedAt,
    LastActivityAt,
    DATEDIFF(MINUTE, LastActivityAt, GETUTCDATE()) as MinutesSinceActivity
FROM Users 
WHERE ActiveSessionId IS NOT NULL;
