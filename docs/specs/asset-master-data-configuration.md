# Requirements Document: Asset Master Data Configuration

## Introduction

The Asset Master Data Configuration feature provides Super Admins with the ability to configure and manage foundational reference data used throughout the IT Asset Management System. This encompasses four key modules:

1. **Configure Asset Masters** - Define common asset attributes and fields
2. **Configure Asset Categories and Types** - Set up asset classification hierarchy
3. **Configure Vendors, Status, and Criticality** - Manage suppliers and asset states
4. **Configure Approval and Escalation Levels** - Define authorization workflows

This configuration capability ensures the system can adapt to organizational changes without requiring code modifications and enables dynamic form rendering based on asset types.

## Glossary

- **Master Data**: Reference data that defines the valid values and business rules used throughout the system
- **Asset Master Field**: A configurable field definition that can be used across asset types
- **Asset Category**: Top-level classification (Hardware, Software, Digital Assets)
- **Asset Type**: Mid-level classification within a category (e.g., Laptop, Server under Hardware)
- **Asset Sub-Type**: Optional detailed classification under a type
- **Vendor**: A supplier or manufacturer of assets, software, or services
- **Asset Status**: The current operational state of an asset (e.g., In Use, Spare, Repair, Decommissioned)
- **Criticality Level**: The importance classification of an asset (TMS-Critical, TMS-General, IT-Critical, IT-General)
- **Approval Level**: A tier in the approval hierarchy for asset-related actions
- **Escalation Level**: A tier in the escalation chain when approvals are delayed or issues arise
- **SLA**: Service Level Agreement - defines response and resolution timeframes
- **Super Admin**: User role with full system configuration privileges
- **Dynamic Form**: Form that changes fields based on selected asset type

## Requirements

### Requirement 1: Configure Asset Master Fields

**User Story:** As a Super Admin, I want to define common asset attributes and fields, so that I can create a consistent data structure across all asset types.

#### Acceptance Criteria

1. WHEN a Super Admin accesses asset master field configuration, THE system SHALL display all configured fields with their properties
2. WHEN creating a new master field, THE system SHALL require: Field Name (unique), Data Type (Text, Number, Date, Boolean, Dropdown, Multi-select), Required Flag, Default Value (optional), and Validation Rules
3. THE system SHALL support the following data types: Text (with max length), Number (integer/decimal), Date, DateTime, Boolean, Dropdown (single select), Multi-select, Email, Phone, URL
4. WHEN a field is marked as required, THE system SHALL enforce this constraint during asset creation/editing
5. THE system SHALL allow configuration of validation rules per field: Min/Max length, Min/Max value, Regex pattern, Custom validation message
6. WHEN a master field is in use by any asset, THE system SHALL prevent deletion but allow modification of non-breaking properties (description, validation rules)
7. THE system SHALL maintain an audit trail of all field configuration changes including timestamp, user, and modified properties
8. THE system SHALL support field grouping for better organization in forms (e.g., "Hardware Specs", "Procurement Details")

### Requirement 2: Configure Asset Categories and Types

**User Story:** As a Super Admin, I want to define asset categories, types, and sub-types with dynamic field mappings, so that asset forms display only relevant fields based on the selected type.

#### Acceptance Criteria

1. WHEN a Super Admin accesses category configuration, THE system SHALL display a hierarchical view of Categories → Types → Sub-Types
2. WHEN creating an asset category, THE system SHALL require: Category Name (unique), Category Code (unique), Description, Icon/Color, and Is Active flag
3. THE system SHALL provide three predefined categories that cannot be deleted: Hardware, Software, Digital Assets
4. WHEN creating an asset type, THE system SHALL require: Type Name (unique within category), Type Code (unique), Parent Category, Description, and Is Active flag
5. WHEN creating an asset sub-type, THE system SHALL require: Sub-Type Name (unique within type), Sub-Type Code (unique), Parent Type, Description, and Is Active flag
6. THE system SHALL allow mapping of master fields to asset types through a TypeFieldMapping configuration
7. WHEN configuring field mappings for a type, THE system SHALL allow: Selection of applicable fields, Override of required flag per type, Override of default values per type, Field display order
8. WHEN an asset type is selected during asset creation, THE system SHALL dynamically render only the fields mapped to that type
9. THE system SHALL validate that at least one active category and one active type exist at all times
10. WHEN a category/type is deactivated, THE system SHALL prevent selection for new assets but retain for existing assets
11. THE system SHALL support inheritance where sub-types automatically include all fields from their parent type plus additional specific fields

### Requirement 3: Configure Vendors, Status, and Criticality

### Requirement 3: Configure Vendors, Status, and Criticality

**User Story:** As a Super Admin, I want to configure vendors, asset statuses, and criticality levels, so that I can maintain accurate supplier records and classify assets appropriately.

#### Acceptance Criteria - Vendor Management

1. WHEN a Super Admin accesses vendor configuration, THE system SHALL display a list of all configured vendors with their status
2. WHEN creating a new vendor, THE system SHALL require: Vendor Name (unique), Vendor Code (unique), Contact Person, Email (validated format), Phone Number, Address, Website (optional), and Status (Active/Inactive)
3. WHEN a vendor name or code already exists, THE system SHALL reject the creation and display an appropriate error message
4. WHEN editing a vendor, THE system SHALL allow modification of all fields except Vendor Code
5. WHEN deactivating a vendor, THE system SHALL warn if the vendor is associated with active assets or pending orders
6. THE system SHALL implement soft delete for vendors (mark as deleted but retain in database)
7. THE system SHALL prevent hard deletion of vendors that have historical associations with assets

#### Acceptance Criteria - Asset Status Configuration

1. WHEN a Super Admin accesses status configuration, THE system SHALL display all configured status values with their properties
2. WHEN creating a new status, THE system SHALL require: Status Name (unique), Status Code (unique), Description, Color Code (for UI display), Icon (optional), and Is Active flag
3. THE system SHALL provide four predefined core statuses that cannot be deleted: In Use, Spare, Repair, Decommissioned
4. THE system SHALL allow renaming of predefined statuses but not deletion
5. WHEN creating custom statuses, THE system SHALL allow Super Admin to define status transitions (which statuses can change to which other statuses)
6. WHEN a status is marked inactive, THE system SHALL prevent assignment to new assets but retain for existing assets
7. THE system SHALL validate that at least one active status exists at all times
8. THE system SHALL support status workflow rules (e.g., "Repair" status requires maintenance ticket number)

#### Acceptance Criteria - Criticality Configuration

1. WHEN a Super Admin accesses criticality configuration, THE system SHALL display all configured criticality levels with their properties
2. WHEN creating a new criticality level, THE system SHALL require: Level Name (unique), Level Code (unique), Description, Priority Order (numeric 1-10), SLA Hours (response time), Priority Level (High/Medium/Low), and Is Active flag
3. THE system SHALL provide four predefined criticality levels that cannot be deleted: TMS-Critical, TMS-General, IT-Critical, IT-General
4. THE system SHALL enforce unique priority ordering across all active criticality levels
5. WHEN an asset is created, THE system SHALL require selection of a criticality level (cannot be null)
6. WHEN a criticality level is associated with approval workflows, THE system SHALL display those associations
7. WHEN deactivating a criticality level, THE system SHALL prevent deactivation if assets are currently assigned to that level
8. THE system SHALL allow configuration of notification thresholds per criticality level (e.g., warranty expiry alerts 90 days for Critical, 30 days for General)
9. THE system SHALL support SLA mapping with configurable response and resolution timeframes per criticality level

### Requirement 4: Configure Approval and Escalation Workflows

### Requirement 4: Configure Approval and Escalation Workflows

**User Story:** As a Super Admin, I want to configure multi-level approval workflows and escalation rules, so that asset-related actions follow appropriate authorization chains with automatic escalation for delayed approvals.

#### Acceptance Criteria - Approval Workflow Configuration

1. WHEN a Super Admin accesses approval configuration, THE system SHALL display all configured approval workflows with their levels and rules
2. WHEN creating an approval workflow, THE system SHALL require: Workflow Name, Workflow Type (Asset Transfer, Asset Decommissioning, Asset Creation - optional), Trigger Conditions, and Active Status
3. WHEN defining approval levels, THE system SHALL require: Level Order (1, 2, 3), Level Name (e.g., Manager, Admin, SuperAdmin), Required Approver Role(s), Timeout Period (hours), and Approval Type (Any One/All Must Approve)
4. THE system SHALL support up to 5 approval levels per workflow
5. WHEN configuring workflow triggers, THE system SHALL allow conditions based on: Asset Criticality, Asset Type, Asset Value Range, Location (Region/Plaza), and Requesting User Role
6. THE system SHALL support parallel approval (any one approver at level can approve) and serial approval (all approvers at level must approve)
7. WHEN an approval is pending, THE system SHALL display it in the approver's dashboard with all relevant asset details
8. WHEN an approver takes action, THE system SHALL record: Approver ID, Action (Approved/Rejected), Timestamp, Comments, and Current Level
9. WHEN an approval is rejected at any level, THE system SHALL stop the workflow and notify the requester with rejection reason
10. WHEN all approval levels are completed, THE system SHALL automatically execute the requested action and notify all stakeholders

#### Acceptance Criteria - Escalation Configuration

1. WHEN a Super Admin accesses escalation configuration, THE system SHALL display all configured escalation rules with their triggers
2. WHEN creating an escalation rule, THE system SHALL require: Rule Name, Trigger Type (Time-based/Event-based), Escalation Levels, and Active Status
3. WHEN configuring time-based escalation, THE system SHALL allow: Timeout Duration (hours), Escalation Target Role(s), Notification Template, and Escalation Action (Notify/Auto-Approve/Reassign)
4. WHEN configuring event-based escalation, THE system SHALL allow triggers based on: Asset Criticality + Pending Duration, Location + Approval Level, Asset Value + Timeout
5. THE system SHALL support up to 3 escalation levels with configurable delays between each level
6. WHEN an escalation is triggered, THE system SHALL: Log the escalation event, Notify configured recipients, Update approval status, Record escalation level reached
7. THE system SHALL run a background job every hour to check for pending approvals that meet escalation criteria
8. WHEN an escalation reaches final level without action, THE system SHALL: Send critical alert to SuperAdmin, Optionally auto-approve based on configuration, Log as high-priority incident
9. THE system SHALL track complete escalation history including: All escalation levels reached, Notifications sent, Final resolution, Time taken at each level

#### Acceptance Criteria - Authorization and Traceability

1. THE system SHALL restrict approval actions to users with assigned approver roles only
2. WHEN a non-authorized user attempts to approve, THE system SHALL deny access and log the attempt
3. THE system SHALL maintain complete audit trail of all approval actions including: Request details, All approval/rejection actions, Escalations triggered, Final outcome, Complete timeline
4. THE system SHALL provide approval history view showing: Workflow path taken, Time spent at each level, Approver actions and comments, Escalations if any
5. THE system SHALL support approval dashboard with filters for: Pending approvals, Approved requests, Rejected requests, Escalated requests, My approvals (user-specific)
6. WHEN approval workflows are modified, THE system SHALL apply changes only to new requests and allow existing requests to complete under original rules

### Requirement 5: Master Data Validation and Integrity

**User Story:** As a Super Admin, I want the system to validate master data configurations, so that invalid or conflicting configurations cannot be saved.

#### Acceptance Criteria

1. WHEN saving any master data configuration, THE system SHALL validate all required fields are populated
2. WHEN saving configurations with unique constraints, THE system SHALL prevent duplicates and display clear error messages
3. THE system SHALL validate that approval and escalation workflows form valid chains without circular dependencies
4. WHEN deactivating master data, THE system SHALL check for dependencies and either prevent deactivation or require reassignment
5. THE system SHALL validate that at least one active value exists for each master data type
6. WHEN importing master data, THE system SHALL validate all records before committing and provide detailed error reports for invalid records
7. THE system SHALL maintain referential integrity between master data and transactional data at all times

### Requirement 6: Master Data Import/Export

**User Story:** As a Super Admin, I want to import and export master data configurations, so that I can migrate configurations between environments or backup configuration data.

#### Acceptance Criteria

1. WHEN exporting master data, THE system SHALL generate a structured file (JSON or CSV) containing all configuration data
2. WHEN importing master data, THE system SHALL validate the file format and data integrity before processing
3. THE system SHALL support selective import/export (e.g., export only vendors, import only criticality levels)
4. WHEN importing data that conflicts with existing records, THE system SHALL provide options to: Skip, Overwrite, or Merge
5. THE system SHALL log all import/export operations with timestamp, user, and record counts
6. WHEN import fails, THE system SHALL rollback all changes and provide detailed error report
7. THE system SHALL support bulk operations for activating/deactivating multiple records simultaneously

### Requirement 7: Master Data Audit and History

**User Story:** As a Super Admin, I want to view complete audit history of master data changes, so that I can track configuration changes and troubleshoot issues.

#### Acceptance Criteria

1. WHEN viewing master data audit logs, THE system SHALL display: Timestamp, User, Action (Create/Update/Delete/Activate/Deactivate), Entity Type, Entity ID, Old Values, New Values
2. THE system SHALL support filtering audit logs by: Date Range, User, Entity Type, Action Type
3. THE system SHALL support exporting audit logs to CSV or PDF for compliance reporting
4. THE system SHALL retain audit logs for a configurable period (default: 7 years) with automatic archival
5. WHEN viewing a specific master data record, THE system SHALL provide a "View History" option showing all changes to that record
6. THE system SHALL support audit log search by entity name or code
7. THE system SHALL prevent deletion or modification of audit log records by any user including Super Admins

### Requirement 8: Master Data UI and Usability

**User Story:** As a Super Admin, I want an intuitive interface for managing master data, so that I can efficiently configure the system without extensive training.

#### Acceptance Criteria

1. WHEN accessing master data configuration, THE system SHALL provide a unified navigation menu with sections for each master data type
2. THE system SHALL provide inline editing capabilities with immediate validation feedback
3. WHEN viewing master data lists, THE system SHALL support sorting, filtering, and searching across all columns
4. THE system SHALL provide bulk action capabilities (activate/deactivate multiple records, bulk import)
5. WHEN creating or editing records, THE system SHALL provide contextual help and field descriptions
6. THE system SHALL use consistent UI patterns across all master data configuration screens
7. THE system SHALL provide confirmation dialogs for destructive actions (delete, deactivate) with impact warnings

### Requirement 9: Master Data Security and Access Control

**User Story:** As a Super Admin, I want master data configuration to be restricted to authorized users, so that critical system configurations cannot be modified by unauthorized personnel.

#### Acceptance Criteria

1. THE system SHALL restrict master data configuration access to users with Super Admin role only
2. WHEN a non-Super Admin attempts to access master data configuration, THE system SHALL deny access and log the attempt
3. THE system SHALL support read-only access to master data for users with specific permissions (e.g., for reporting purposes)
4. WHEN Super Admin makes configuration changes, THE system SHALL require re-authentication for sensitive operations
5. THE system SHALL support IP-based restrictions for master data configuration access if configured
6. THE system SHALL log all access attempts (successful and failed) to master data configuration
7. THE system SHALL support emergency access procedures with mandatory post-action review and approval
