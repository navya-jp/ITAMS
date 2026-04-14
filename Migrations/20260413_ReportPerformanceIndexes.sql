-- Performance indexes for report queries
-- Run on ITAMS_Shared database

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Assets_IsDecommissioned')
    CREATE INDEX IX_Assets_IsDecommissioned ON Assets(IsDecommissioned);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Assets_AssetStatusId')
    CREATE INDEX IX_Assets_AssetStatusId ON Assets(AssetStatusId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Assets_ProjectId_LocationId')
    CREATE INDEX IX_Assets_ProjectId_LocationId ON Assets(ProjectId, LocationId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Assets_WarrantyEndDate')
    CREATE INDEX IX_Assets_WarrantyEndDate ON Assets(WarrantyEndDate) WHERE WarrantyEndDate IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LicensingAssets_ValidityEndDate')
    CREATE INDEX IX_LicensingAssets_ValidityEndDate ON LicensingAssets(ValidityEndDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ServiceAssets_ContractEndDate')
    CREATE INDEX IX_ServiceAssets_ContractEndDate ON ServiceAssets(ContractEndDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MaintenanceRequests_Status')
    CREATE INDEX IX_MaintenanceRequests_Status ON MaintenanceRequests(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SystemAlerts_IsResolved_IsAcknowledged')
    CREATE INDEX IX_SystemAlerts_IsResolved_IsAcknowledged ON SystemAlerts(IsResolved, IsAcknowledged);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoginAudits_LoginTime')
    CREATE INDEX IX_LoginAudits_LoginTime ON LoginAudit(LoginTime);

PRINT 'Performance indexes created.';
