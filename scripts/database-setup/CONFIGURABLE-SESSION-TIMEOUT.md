# Configurable Session Timeout Feature

## Overview
Super Admin can now configure the automatic logout timeout and other security settings through the System Settings page.

## Features Implemented

### 1. Database
- **SystemSettings Table**: Stores all configurable application settings
- **Default Settings**:
  - SessionTimeoutMinutes: 30 (auto-logout after inactivity)
  - SessionWarningMinutes: 5 (warning before auto-logout)
  - MaxLoginAttempts: 5
  - LockoutDurationMinutes: 30
  - PasswordExpiryDays: 90
  - RequirePasswordChange: true
  - AllowMultipleSessions: false

### 2. Backend
- **SystemSetting Entity**: Domain model for settings
- **SettingsController**: API endpoints for managing settings
  - GET /api/settings - Get all settings
  - GET /api/settings/category/{category} - Get settings by category
  - PUT /api/settings/{id} - Update a single setting
  - POST /api/settings/bulk-update - Update multiple settings
- **AuthController**: Updated to load settings from database
  - GET /api/auth/security-settings - Returns current security settings

### 3. Frontend
- **Settings Component**: Admin UI for managing settings
  - Filter by category (Security, General)
  - Edit settings with appropriate input types (text, number, checkbox)
  - Bulk save changes
  - Visual indication of modified settings
- **Navigation**: Added "Settings" menu item for Super Admin
- **API Service**: Added methods for settings management

## How It Works

### Session Timeout Flow
1. User logs in
2. Frontend loads security settings from backend
3. Auth service sets up activity tracking
4. After configured timeout (default 30 minutes) of inactivity:
   - User is automatically logged out
   - Logout type is set to "SESSION_TIMEOUT"
   - Status shows as yellow badge in audit trail

### Configuration Flow
1. Super Admin navigates to Settings page
2. Modifies SessionTimeoutMinutes value (e.g., change from 30 to 60)
3. Clicks "Save Changes"
4. New timeout applies to all new logins
5. Existing sessions continue with their original timeout

## Usage

### For Super Admin
1. Login at http://localhost:4200
2. Navigate to "Settings" in the sidebar
3. Filter by "Security" category
4. Modify "SessionTimeoutMinutes" value
5. Click "Save Changes"

### Setting Values
- **SessionTimeoutMinutes**: 1-1440 (1 minute to 24 hours)
- **SessionWarningMinutes**: 1-30 (warning time before logout)
- **MaxLoginAttempts**: 3-10 (failed login attempts)
- **LockoutDurationMinutes**: 5-1440 (lockout duration)

## Database Migration
```sql
-- Run this migration
Migrations/20260219_CreateSystemSettingsTable.sql
```

## API Endpoints

### Get All Settings
```http
GET /api/settings
Authorization: Bearer {token}
```

### Update Setting
```http
PUT /api/settings/{id}
Content-Type: application/json

{
  "id": 1,
  "settingValue": "60"
}
```

### Bulk Update
```http
POST /api/settings/bulk-update
Content-Type: application/json

[
  { "id": 1, "settingValue": "60" },
  { "id": 2, "settingValue": "10" }
]
```

## Security
- Only Super Admin can view and modify settings
- Settings are validated based on data type
- Non-editable settings cannot be modified
- All changes are logged with user ID and timestamp

## Testing
1. Login as superadmin
2. Go to Settings page
3. Change SessionTimeoutMinutes to 2 (2 minutes)
4. Save changes
5. Logout and login again
6. Wait 2 minutes without activity
7. Should be automatically logged out with SESSION_TIMEOUT status

## Notes
- Changes take effect for new logins only
- Existing sessions maintain their original timeout
- Session timeout is based on last activity (mouse, keyboard, scroll, etc.)
- Warning is shown before auto-logout (configurable)
