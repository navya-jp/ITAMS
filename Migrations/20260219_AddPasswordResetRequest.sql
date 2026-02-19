-- Add password reset request tracking columns to Users table
-- Migration: 20260219_AddPasswordResetRequest

-- Add PasswordResetRequested column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PasswordResetRequested')
BEGIN
    ALTER TABLE [Users]
    ADD [PasswordResetRequested] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Added PasswordResetRequested column to Users table';
END
ELSE
BEGIN
    PRINT 'PasswordResetRequested column already exists in Users table';
END
GO

-- Add PasswordResetRequestedAt column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PasswordResetRequestedAt')
BEGIN
    ALTER TABLE [Users]
    ADD [PasswordResetRequestedAt] DATETIME2 NULL;
    
    PRINT 'Added PasswordResetRequestedAt column to Users table';
END
ELSE
BEGIN
    PRINT 'PasswordResetRequestedAt column already exists in Users table';
END
GO

PRINT 'Migration 20260219_AddPasswordResetRequest completed successfully';
GO
