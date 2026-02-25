-- Migration: Add Extended Asset Fields
-- Date: 2026-02-25
-- Description: Add Classification, OS, DB, Network, and Status tracking fields to Assets table

-- Add new columns to Assets table (IPAddress and UpdatedAt already exist)
ALTER TABLE Assets ADD 
    Classification NVARCHAR(100) NULL,
    OSType NVARCHAR(100) NULL,
    OSVersion NVARCHAR(100) NULL,
    DBType NVARCHAR(100) NULL,
    DBVersion NVARCHAR(100) NULL,
    AssignedUser NVARCHAR(200) NULL,
    UserRole NVARCHAR(100) NULL,
    ProcuredBy NVARCHAR(200) NULL,
    PatchStatus NVARCHAR(100) NULL,
    USBBlockingStatus NVARCHAR(100) NULL,
    Remarks NVARCHAR(MAX) NULL;

GO

PRINT 'Extended asset fields added successfully';
