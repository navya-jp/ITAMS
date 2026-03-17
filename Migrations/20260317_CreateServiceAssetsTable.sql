-- ============================================================
-- Create ServiceAssets and ServiceRenewals tables
-- Date: 2026-03-17
-- ============================================================

-- 1. Create ServiceAssets table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServiceAssets')
BEGIN
    CREATE TABLE ServiceAssets (
        Id                    INT IDENTITY(1,1) PRIMARY KEY,
        AssetId               NVARCHAR(50)  NOT NULL UNIQUE,
        ServiceName           NVARCHAR(200) NOT NULL DEFAULT '',
        ServiceTypeId         INT           NOT NULL DEFAULT 1,
        ProjectId             INT           NULL,
        LocationId            INT           NULL,
        VendorId              INT           NULL,
        VendorName            NVARCHAR(200) NOT NULL DEFAULT '',
        ContractNumber        NVARCHAR(100) NULL,
        ContractStartDate     DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        ContractEndDate       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        RenewalCycleMonths    INT           NOT NULL DEFAULT 12,
        RenewalReminderDays   INT           NOT NULL DEFAULT 30,
        ContractCost          DECIMAL(18,2) NULL,
        BillingCycle          NVARCHAR(50)  NULL,
        Currency              NVARCHAR(10)  NOT NULL DEFAULT 'INR',
        SLAType               NVARCHAR(100) NULL,
        ResponseTime          NVARCHAR(100) NULL,
        CoverageDetails       NVARCHAR(500) NULL,
        ContactPerson         NVARCHAR(200) NULL,
        SupportContactNumber  NVARCHAR(50)  NULL,
        Description           NVARCHAR(500) NULL,
        Remarks               NVARCHAR(500) NULL,
        UsageCategory         NVARCHAR(50)  NOT NULL DEFAULT 'TMS',
        Status                NVARCHAR(50)  NOT NULL DEFAULT 'Active',
        AutoRenewEnabled      BIT           NOT NULL DEFAULT 0,
        NextRenewalDate       DATETIME2     NULL,
        LastRenewalDate       DATETIME2     NULL,
        CreatedAt             DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy             INT           NOT NULL DEFAULT 1,
        UpdatedAt             DATETIME2     NULL,
        UpdatedBy             INT           NULL
    );
    PRINT 'ServiceAssets table created';
END
ELSE
BEGIN
    -- Table exists — add any missing columns
    IF COL_LENGTH('ServiceAssets', 'ServiceName') IS NULL
        ALTER TABLE ServiceAssets ADD ServiceName NVARCHAR(200) NOT NULL DEFAULT '';
    IF COL_LENGTH('ServiceAssets', 'VendorName') IS NULL
        ALTER TABLE ServiceAssets ADD VendorName NVARCHAR(200) NOT NULL DEFAULT '';
    IF COL_LENGTH('ServiceAssets', 'ContractStartDate') IS NULL
        ALTER TABLE ServiceAssets ADD ContractStartDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    IF COL_LENGTH('ServiceAssets', 'ContractEndDate') IS NULL
        ALTER TABLE ServiceAssets ADD ContractEndDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    IF COL_LENGTH('ServiceAssets', 'RenewalCycleMonths') IS NULL
        ALTER TABLE ServiceAssets ADD RenewalCycleMonths INT NOT NULL DEFAULT 12;
    IF COL_LENGTH('ServiceAssets', 'RenewalReminderDays') IS NULL
        ALTER TABLE ServiceAssets ADD RenewalReminderDays INT NOT NULL DEFAULT 30;
    IF COL_LENGTH('ServiceAssets', 'ContractCost') IS NULL
        ALTER TABLE ServiceAssets ADD ContractCost DECIMAL(18,2) NULL;
    IF COL_LENGTH('ServiceAssets', 'BillingCycle') IS NULL
        ALTER TABLE ServiceAssets ADD BillingCycle NVARCHAR(50) NULL;
    IF COL_LENGTH('ServiceAssets', 'Currency') IS NULL
        ALTER TABLE ServiceAssets ADD Currency NVARCHAR(10) NOT NULL DEFAULT 'INR';
    IF COL_LENGTH('ServiceAssets', 'SLAType') IS NULL
        ALTER TABLE ServiceAssets ADD SLAType NVARCHAR(100) NULL;
    IF COL_LENGTH('ServiceAssets', 'ResponseTime') IS NULL
        ALTER TABLE ServiceAssets ADD ResponseTime NVARCHAR(100) NULL;
    IF COL_LENGTH('ServiceAssets', 'CoverageDetails') IS NULL
        ALTER TABLE ServiceAssets ADD CoverageDetails NVARCHAR(500) NULL;
    IF COL_LENGTH('ServiceAssets', 'ContactPerson') IS NULL
        ALTER TABLE ServiceAssets ADD ContactPerson NVARCHAR(200) NULL;
    IF COL_LENGTH('ServiceAssets', 'SupportContactNumber') IS NULL
        ALTER TABLE ServiceAssets ADD SupportContactNumber NVARCHAR(50) NULL;
    IF COL_LENGTH('ServiceAssets', 'UsageCategory') IS NULL
        ALTER TABLE ServiceAssets ADD UsageCategory NVARCHAR(50) NOT NULL DEFAULT 'TMS';
    IF COL_LENGTH('ServiceAssets', 'AutoRenewEnabled') IS NULL
        ALTER TABLE ServiceAssets ADD AutoRenewEnabled BIT NOT NULL DEFAULT 0;
    IF COL_LENGTH('ServiceAssets', 'ProjectId') IS NULL
        ALTER TABLE ServiceAssets ADD ProjectId INT NULL;
    IF COL_LENGTH('ServiceAssets', 'LocationId') IS NULL
        ALTER TABLE ServiceAssets ADD LocationId INT NULL;
    IF COL_LENGTH('ServiceAssets', 'VendorId') IS NULL
        ALTER TABLE ServiceAssets ADD VendorId INT NULL;
    IF COL_LENGTH('ServiceAssets', 'NextRenewalDate') IS NULL
        ALTER TABLE ServiceAssets ADD NextRenewalDate DATETIME2 NULL;
    IF COL_LENGTH('ServiceAssets', 'LastRenewalDate') IS NULL
        ALTER TABLE ServiceAssets ADD LastRenewalDate DATETIME2 NULL;
    IF COL_LENGTH('ServiceAssets', 'Description') IS NULL
        ALTER TABLE ServiceAssets ADD Description NVARCHAR(500) NULL;
    IF COL_LENGTH('ServiceAssets', 'Remarks') IS NULL
        ALTER TABLE ServiceAssets ADD Remarks NVARCHAR(500) NULL;
    IF COL_LENGTH('ServiceAssets', 'Status') IS NULL
        ALTER TABLE ServiceAssets ADD Status NVARCHAR(50) NOT NULL DEFAULT 'Active';
    PRINT 'ServiceAssets table updated';
END

-- 2. Create ServiceRenewals table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ServiceRenewals')
BEGIN
    CREATE TABLE ServiceRenewals (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        ServiceId       INT           NOT NULL,
        PreviousEndDate DATETIME2     NOT NULL,
        NewStartDate    DATETIME2     NOT NULL,
        NewEndDate      DATETIME2     NOT NULL,
        RenewalCost     DECIMAL(18,2) NULL,
        RenewedBy       INT           NOT NULL,
        RenewalDate     DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        Remarks         NVARCHAR(500) NULL,
        CONSTRAINT FK_ServiceRenewals_ServiceAssets
            FOREIGN KEY (ServiceId) REFERENCES ServiceAssets(Id) ON DELETE CASCADE
    );
    PRINT 'ServiceRenewals table created';
END

-- 3. Seed ServiceTypes if empty
IF NOT EXISTS (SELECT 1 FROM ServiceTypes)
BEGIN
    INSERT INTO ServiceTypes (TypeName, Description, CreatedAt, CreatedBy)
    VALUES
        ('AMC Comprehensive',       'Annual Maintenance Contract - Comprehensive coverage',     GETUTCDATE(), 1),
        ('AMC Non-Comprehensive',   'Annual Maintenance Contract - Non-Comprehensive coverage', GETUTCDATE(), 1),
        ('Leased Line',             'Internet / Network leased line service',                   GETUTCDATE(), 1),
        ('Maintenance + Breakdown', 'Maintenance contract with breakdown visits',               GETUTCDATE(), 1),
        ('Breakdown Only',          'Breakdown visits only contract',                           GETUTCDATE(), 1);
    PRINT 'ServiceTypes seeded';
END
