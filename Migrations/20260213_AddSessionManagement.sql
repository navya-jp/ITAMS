-- Migration: Add Session Management to Users table
-- Date: 2026-02-13
-- Description: Add ActiveSessionId and SessionStartedAt columns for session management

-- Add session management columns to Users table
ALTER TABLE Users
ADD ActiveSessionId NVARCHAR(500) NULL,
    SessionStartedAt DATETIME2 NULL;

GO

-- Add index for faster session lookups
SET QUOTED_IDENTIFIER ON;
CREATE INDEX IX_Users_ActiveSessionId ON Users(ActiveSessionId) WHERE ActiveSessionId IS NOT NULL;

GO

PRINT 'Session management columns added successfully';
