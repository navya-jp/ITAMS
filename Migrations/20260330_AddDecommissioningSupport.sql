-- Migration: Add Decommissioning Support (P11)
-- Run on ITAMS_Shared database

-- Add decommission fields to Assets table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'IsDecommissioned')
BEGIN
    ALTER TABLE Assets ADD IsDecommissioned BIT NOT NULL DEFAULT 0;
    ALTER TABLE Assets ADD DecommissionedAt DATETIME2 NULL;
    ALTER TABLE Assets ADD DecommissionedBy INT NULL;
    PRINT 'Added decommission fields to Assets table.';
END
ELSE PRINT 'Decommission fields already exist.';

-- Create DecommissionArchives table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DecommissionArchives')
BEGIN
    CREATE TABLE DecommissionArchives (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AssetId INT NOT NULL,
        AssetTag NVARCHAR(50) NOT NULL,
        AssetSnapshot NVARCHAR(MAX) NOT NULL,
        DecommissionReason NVARCHAR(2000) NOT NULL,
        DisposalMethod NVARCHAR(100) NOT NULL,
        Notes NVARCHAR(2000) NULL,
        ApprovalRequestId INT NOT NULL,
        ApprovalChainSnapshot NVARCHAR(MAX) NOT NULL,
        ArchivedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ArchivedBy INT NOT NULL,
        ArchivedByName NVARCHAR(200) NULL,
        CONSTRAINT FK_DecommissionArchives_Assets FOREIGN KEY (AssetId) REFERENCES Assets(Id)
    );
    CREATE INDEX IX_DecommissionArchives_AssetId ON DecommissionArchives(AssetId);
    PRINT 'DecommissionArchives table created.';
END
ELSE PRINT 'DecommissionArchives table already exists.';
