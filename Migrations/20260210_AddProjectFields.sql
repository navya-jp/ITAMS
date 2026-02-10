-- Migration: Add PreferredName, SpvName, and States to Projects table
-- Date: 2026-02-10
-- Description: Add missing fields to support project management UI

-- Add new columns to Projects table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND name = 'PreferredName')
BEGIN
    ALTER TABLE Projects ADD PreferredName NVARCHAR(200) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND name = 'SpvName')
BEGIN
    ALTER TABLE Projects ADD SpvName NVARCHAR(200) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Projects]') AND name = 'States')
BEGIN
    ALTER TABLE Projects ADD States NVARCHAR(200) NULL;
END
