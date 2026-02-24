# Testing Dynamic System Settings

All system settings are now fully dynamic and will take effect immediately when changed in the Settings UI.

## Implementation Summary

### Files Modified:
1. **Services/SettingsService.cs** (NEW)
   - Centralized settings service with 5-minute caching
   - Provides typed methods: `GetIntSettingAsync`, `GetBoolSettingAsync`, `GetSecuritySettingsAsync`
   - Cache automatically refreshes every 5 minutes

2. **Middleware/MaintenanceModeMiddleware.cs** (NEW)
   - Blocks non-admin access when MaintenanceMode is enabled
   - Returns 503 status with JSON message
   - Allows Super Admins to access during maintenance

3. **Services/UserService.cs** (UPDATED)
   - Uses dynamic `MaxLoginAttempts` from settings (line 62)
   - Uses dynamic `LockoutDurationMinutes` from settings (line 65)
   - Checks password expiry using `PasswordExpiryDays` from settings
   - Enforces `RequirePasswordChange` for first-time users

4. **Controllers/AuthController.cs** (UPDATED)
   - Removed duplicate `SecuritySettings` class
   - Uses `ISettingsService` to load settings dynamically
   - Simplified `GetSecuritySettings` endpoint

5. **Services/SessionCleanupService.cs** (UPDATED)
   - Uses dynamic `SessionTimeoutMinutes` from settings
   - Logs the timeout value being used

6. **Program.cs** (UPDATED)
   - Registered `ISettingsService` as scoped service
   - Added `MaintenanceModeMiddleware` to pipeline (before activity tracking)

---

## Settings to Test

### 1. MaxLoginAttempts
**Current Default:** 5 attempts
**How to Test:**
1. Go to Settings UI and change MaxLoginAttempts to 3
2. Try logging in with wrong password 3 times
3. User should be locked out after 3 attempts (not 5)

**Expected Behavior:**
- After X failed attempts, user is locked
- Lockout duration uses LockoutDurationMinutes setting

---

### 2. LockoutDurationMinutes
**Current Default:** 30 minutes
**How to Test:**
1. Go to Settings UI and change LockoutDurationMinutes to 5
2. Trigger a lockout (fail login MaxLoginAttempts times)
3. User should be unlocked after 5 minutes (not 30)

**Expected Behavior:**
- User locked for X minutes
- Can login again after X minutes

---

### 3. SessionTimeoutMinutes
**Current Default:** 30 minutes
**How to Test:**
1. Go to Settings UI and change SessionTimeoutMinutes to 5
2. Login and wait 5 minutes without activity
3. SessionCleanupService should mark session as SESSION_TIMEOUT
4. User should be logged out

**Expected Behavior:**
- Session expires after X minutes of inactivity
- LoginAudit record shows "SESSION_TIMEOUT" status
- User must login again

**Note:** SessionCleanupService runs every 1 minute, so timeout may take up to 1 minute extra to trigger.

---

### 4. PasswordExpiryDays
**Current Default:** 90 days
**How to Test:**
1. Go to Settings UI and change PasswordExpiryDays to 1
2. Find a user whose password was changed more than 1 day ago
3. Try logging in as that user
4. User should be forced to change password

**Expected Behavior:**
- If password is older than X days, `MustChangePassword` flag is set
- User must change password before accessing system

**Note:** To test immediately, you can manually update a user's `PasswordChangedAt` in database to an old date.

---

### 5. RequirePasswordChange
**Current Default:** true
**How to Test:**
1. Create a new user (first-time user with no LastLoginAt)
2. Set RequirePasswordChange to false in Settings UI
3. New user should be able to login without being forced to change password
4. Set RequirePasswordChange to true
5. Create another new user
6. New user should be forced to change password on first login

**Expected Behavior:**
- When true: First-time users must change password
- When false: First-time users can use default password

---

### 6. MaintenanceMode
**Current Default:** false
**How to Test:**
1. Login as a regular user (not Super Admin)
2. Go to Settings UI as Super Admin
3. Enable MaintenanceMode (set to true)
4. Regular user should be blocked with 503 error
5. Super Admin should still have full access
6. Disable MaintenanceMode
7. Regular user should regain access

**Expected Behavior:**
- When enabled: Only Super Admins can access system
- Non-admin users get 503 status with message: "System is currently under maintenance. Please try again later."
- Login page and settings API remain accessible

**Allowed Paths During Maintenance:**
- `/api/auth/login`
- `/api/settings`
- `/api/auth/settings`
- `/assets/*`
- `/` (root)
- `/login`

---

## Cache Behavior

The `SettingsService` caches settings for 5 minutes to reduce database queries. This means:

- Settings changes take effect within 5 minutes maximum
- For immediate testing, restart the backend to clear cache
- In production, 5-minute delay is acceptable for settings changes

---

## Testing Checklist

- [ ] MaxLoginAttempts - Change to 3, verify lockout after 3 attempts
- [ ] LockoutDurationMinutes - Change to 5, verify unlock after 5 minutes
- [ ] SessionTimeoutMinutes - Change to 5, verify timeout after 5 minutes
- [ ] PasswordExpiryDays - Change to 1, verify old passwords expire
- [ ] RequirePasswordChange - Toggle and verify first-login behavior
- [ ] MaintenanceMode - Enable and verify non-admin users blocked

---

## Database Query to Check Settings

```sql
SELECT * FROM SystemSettings 
WHERE SettingKey IN (
    'MaxLoginAttempts',
    'LockoutDurationMinutes', 
    'SessionTimeoutMinutes',
    'PasswordExpiryDays',
    'RequirePasswordChange',
    'MaintenanceMode'
);
```

---

## Troubleshooting

### Settings not taking effect?
1. Check if cache expired (wait 5 minutes or restart backend)
2. Verify setting value in database
3. Check backend logs for errors loading settings

### Maintenance mode not working?
1. Verify middleware is registered in Program.cs
2. Check user role (Super Admins bypass maintenance mode)
3. Check backend logs for middleware execution

### Session timeout not working?
1. Verify SessionCleanupService is running (check logs)
2. Wait up to 1 minute for cleanup service to run
3. Check user's LastActivityAt timestamp in database
