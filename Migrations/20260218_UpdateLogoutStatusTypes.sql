-- Migration: Update Logout Status Types
-- Date: 2026-02-18
-- Description: Updates existing EXPIRED status to SESSION_TIMEOUT and adds support for FORCED_LOGOUT

USE ITAMS_Shared;
GO

PRINT 'Starting logout status types update...';
GO

-- Update existing EXPIRED status to SESSION_TIMEOUT
UPDATE LoginAudit
SET Status = 'SESSION_TIMEOUT'
WHERE Status = 'EXPIRED';

PRINT 'Updated EXPIRED status to SESSION_TIMEOUT';
GO

-- Show summary of status types
SELECT 
    Status,
    COUNT(*) as Count
FROM LoginAudit
GROUP BY Status
ORDER BY Count DESC;

PRINT 'Logout status types update completed successfully.';
PRINT 'Available status types: ACTIVE, LOGGED_OUT, SESSION_TIMEOUT, FORCED_LOGOUT';
GO
