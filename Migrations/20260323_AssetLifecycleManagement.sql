-- ============================================================
-- P8: Asset Lifecycle Management Tables
-- Date: 2026-03-23
-- ============================================================

-- 1. AssetAssignmentHistory
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetAssignmentHistories')
BEGIN
    CREATE TABLE AssetAssignmentHistories (
        Id                   INT IDENTITY(1,1) PRIMARY KEY,
        AssetId              INT           NOT NULL,
        PreviousUserId       INT           NULL,
        PreviousUserName     NVARCHAR(200) NULL,
        NewUserId            INT           NULL,
        NewUserName          NVARCHAR(200) NULL,
        PreviousLocationId   INT           NULL,
        PreviousLocationName NVARCHAR(200) NULL,
        NewLocationId        INT           NULL,
        NewLocationName      NVARCHAR(200) NULL,
        Reason               NVARCHAR(500) NULL,
        ChangedAt            DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        ChangedBy            INT           NOT NULL DEFAULT 1,
        ChangedByName        NVARCHAR(200) NULL,
        CONSTRAINT FK_AssignmentHistory_Assets    FOREIGN KEY (AssetId)           REFERENCES Assets(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AssignmentHistory_PrevUser  FOREIGN KEY (PreviousUserId)    REFERENCES Users(Id)  ON DELETE NO ACTION,
        CONSTRAINT FK_AssignmentHistory_NewUser   FOREIGN KEY (NewUserId)         REFERENCES Users(Id)  ON DELETE NO ACTION,
        CONSTRAINT FK_AssignmentHistory_PrevLoc   FOREIGN KEY (PreviousLocationId) REFERENCES Locations(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_AssignmentHistory_NewLoc    FOREIGN KEY (NewLocationId)     REFERENCES Locations(Id) ON DELETE NO ACTION
    );
    PRINT 'AssetAssignmentHistories table created';
END

-- 2. AssetTransferRequests
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetTransferRequests')
BEGIN
    CREATE TABLE AssetTransferRequests (
        Id                INT IDENTITY(1,1) PRIMARY KEY,
        AssetId           INT           NOT NULL,
        FromLocationId    INT           NOT NULL,
        ToLocationId      INT           NOT NULL,
        FromUserId        INT           NULL,
        ToUserId          INT           NULL,
        Reason            NVARCHAR(500) NULL,
        Status            NVARCHAR(20)  NOT NULL DEFAULT 'Completed',
        Notes             NVARCHAR(500) NULL,
        TransferDate      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        RequestedBy       INT           NOT NULL DEFAULT 1,
        RequestedByName   NVARCHAR(200) NULL,
        CONSTRAINT FK_Transfer_Assets    FOREIGN KEY (AssetId)        REFERENCES Assets(Id)    ON DELETE CASCADE,
        CONSTRAINT FK_Transfer_FromLoc   FOREIGN KEY (FromLocationId) REFERENCES Locations(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Transfer_ToLoc     FOREIGN KEY (ToLocationId)   REFERENCES Locations(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Transfer_FromUser  FOREIGN KEY (FromUserId)     REFERENCES Users(Id)     ON DELETE NO ACTION,
        CONSTRAINT FK_Transfer_ToUser    FOREIGN KEY (ToUserId)       REFERENCES Users(Id)     ON DELETE NO ACTION
    );
    PRINT 'AssetTransferRequests table created';
END

-- 3. MaintenanceRequests
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MaintenanceRequests')
BEGIN
    CREATE TABLE MaintenanceRequests (
        Id                 INT IDENTITY(1,1) PRIMARY KEY,
        AssetId            INT           NOT NULL,
        RequestType        NVARCHAR(50)  NOT NULL DEFAULT 'Maintenance',
        Description        NVARCHAR(500) NOT NULL DEFAULT '',
        OldSpecifications  NVARCHAR(1000) NULL,
        NewSpecifications  NVARCHAR(1000) NULL,
        VendorName         NVARCHAR(200) NULL,
        Cost               DECIMAL(18,2) NULL,
        ScheduledDate      DATETIME2     NULL,
        CompletedDate      DATETIME2     NULL,
        Status             NVARCHAR(20)  NOT NULL DEFAULT 'Open',
        Resolution         NVARCHAR(500) NULL,
        Remarks            NVARCHAR(500) NULL,
        CreatedAt          DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy          INT           NOT NULL DEFAULT 1,
        CreatedByName      NVARCHAR(200) NULL,
        UpdatedAt          DATETIME2     NULL,
        UpdatedBy          INT           NULL,
        CONSTRAINT FK_Maintenance_Assets FOREIGN KEY (AssetId) REFERENCES Assets(Id) ON DELETE CASCADE
    );
    PRINT 'MaintenanceRequests table created';
END

-- 4. ComplianceChecks
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ComplianceChecks')
BEGIN
    CREATE TABLE ComplianceChecks (
        Id             INT IDENTITY(1,1) PRIMARY KEY,
        AssetId        INT           NOT NULL,
        CheckType      NVARCHAR(100) NOT NULL,
        Result         NVARCHAR(20)  NOT NULL DEFAULT 'Pass',
        Details        NVARCHAR(500) NULL,
        Remediation    NVARCHAR(500) NULL,
        Status         NVARCHAR(20)  NOT NULL DEFAULT 'Open',
        CheckedAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        CheckedBy      INT           NOT NULL DEFAULT 1,
        CheckedByName  NVARCHAR(200) NULL,
        ResolvedAt     DATETIME2     NULL,
        ResolvedBy     INT           NULL,
        CONSTRAINT FK_Compliance_Assets FOREIGN KEY (AssetId) REFERENCES Assets(Id) ON DELETE CASCADE
    );
    PRINT 'ComplianceChecks table created';
END
