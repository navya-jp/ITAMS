# ITAMS Database Migrations

## Quick Start — Fresh Database Setup

To set up the database from scratch on a new server, run **only this one file**:

```sql
-- Run in SQL Server Management Studio or sqlcmd
sqlcmd -S <server> -U <user> -P <password> -i Migrations/schema.sql
```

This single file creates all 56 tables with correct columns, primary keys, and foreign key constraints. It uses `IF OBJECT_ID ... IS NULL` guards so it's safe to run on an existing database too.

---

## Individual Migration Files (History)

The numbered files in this folder are the incremental scripts applied during development, in chronological order. They are kept for historical reference and audit purposes. **You do not need to run these on a fresh setup** — `schema.sql` covers everything.

| File | Purpose |
|------|---------|
| `20260130_CreateRBACCoreTables.sql` | Initial RBAC roles, permissions, user-role tables |
| `20260209_AddAuditColumnsToAllTables.sql` | CreatedAt, UpdatedAt, CreatedBy on all tables |
| `20260209_AssignDefaultRolePermissions.sql` | Seed default role-permission mappings |
| `20260210_AddAlternateKeys.sql` | Unique constraints / alternate keys |
| `20260210_AddProjectFields.sql` | Project assignment fields on assets/users |
| `20260210_MigrateForeignKeys_Phase*.sql` | FK migration in phases (data integrity) |
| `20260213_AddProjectLocationAccessControl.sql` | Project/location scoped access control |
| `20260213_AddSessionManagement.sql` | Session tracking tables |
| `20260213_CreateLoginAuditTable.sql` | Login audit log |
| `20260213_DistributeUsersAcrossProjects.sql` | Data migration — assign users to projects |
| `20260218_RenamePlazaToSite.sql` | Renamed "Plaza" to "Site" in enums |
| `20260219_CreateSystemSettingsTable.sql` | System-wide settings key-value store |
| `20260223_CreateApprovalWorkflowTables.sql` | Approval workflow engine tables |
| `20260223_CreateMasterDataTables.sql` | Master data lookup tables |
| `20260225_AddAssetExtendedFields.sql` | Extended asset fields (patch, USB, etc.) |
| `20260227_AddPlacingFieldAndUpdateEnums.sql` | Asset placing (Office/Site/Tunnel) |
| `20260311_DatabaseNormalization_Phase1.sql` | Normalize asset type/status to FK tables |
| `20260311_MigrateAssetDataToFK.sql` | Migrate existing asset data to new FKs |
| `20260311_SeedAssetTypes.sql` | Seed asset type master data |
| `20260313_RefactorAssetCategorization_v2.sql` | Final asset categorization refactor |
| `20260316_ServicesModule.sql` | Service assets / contracts module |
| `20260316_AddTunnelPlacing.sql` | Add Tunnel as asset placing option |
| `20260317_CreateServiceAssetsTable.sql` | Service assets table |
| `20260323_AssetLifecycleManagement.sql` | Lifecycle management (maintenance, compliance) |
| `20260324_SystemAlerts.sql` | System alerts and notifications |
| `20260325_InsertHOUsers.sql` | Seed Head Office users |
| `20260330_AddDecommissioningSupport.sql` | Decommission archive table |
| `20260413_ReportPerformanceIndexes.sql` | Performance indexes for reports |

---

## Database Details

- **Database**: `ITAMS_Shared`
- **Server**: SQL Server 2019+
- **Tables**: 56
- **Schema generated**: 27-Apr-2026
