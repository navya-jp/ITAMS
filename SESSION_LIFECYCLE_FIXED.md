# Session Lifecycle - Single Session System (FIXED)

## Overview
This document describes the correct session lifecycle for a single-session-per-user system.

## Core Principle
**Status represents HOW the session ended, not temporary inactivity.**

## Session States

| Status | Meaning | When It Happens |
|--------|---------|-----------------|
| `ACTIVE` | User is logged in and session is valid | During login, remains until session ends |
| `LOGGED_OUT` | User clicked logout button | User explicitly logs out |
| `SESSION_TIMEOUT` | Session expired due to inactivity | No activity for configured timeout period (default: 30 minutes) |
| `FORCED_LOGOUT` | Browser closed without logout | User tries to login again but previous session never logged out properly |

## Correct Session Lifecycle

```
LOGIN
  ↓
Status = ACTIVE
SessionStartedAt = now
LastActivityAt = now
ActiveSessionId = new Guid
  ↓
User idle 2 minutes → STILL ACTIVE ✅
  ↓
User idle 10 minutes → STILL ACTIVE ✅
  ↓
User idle 29 minutes → STILL ACTIVE ✅
  ↓
User idle 31 minutes (timeout = 30) → SESSION_TIMEOUT ⏱️
```

## Implementation Details

### 1. On Login
```csharp
Status = "ACTIVE"
SessionStartedAt = DateTimeHelper.Now
LastActivityAt = DateTimeHelper.Now
ActiveSessionId = Guid.NewGuid().ToString()
```

### 2. On Every Heartbeat (Every 30 seconds)
```csharp
// Middleware updates LastActivityAt
LastActivityAt = DateTimeHelper.Now

// ⚠️ DO NOT touch Status here
// Middleware should NEVER update Status
```

### 3. On Logout Button Click
```csharp
Status = "LOGGED_OUT"
LogoutTime = DateTimeHelper.Now
ActiveSessionId = null
SessionStartedAt = null
```

### 4. On Session Timeout (Background Service)
```csharp
// SessionCleanupService runs every minute
// Checks if: (now - LastActivityAt) >= ConfiguredTimeout

if (timeSinceActivity.TotalMinutes >= configuredTimeoutMinutes)
{
    Status = "SESSION_TIMEOUT"
    LogoutTime = DateTimeHelper.Now
    ActiveSessionId = null
    SessionStartedAt = null
}
```

### 5. On Browser Closed Without Logout (Login Detection)
```csharp
// When user tries to login again:
// If ActiveSessionId exists AND Status is ACTIVE
// But inactivity > configured timeout

if (timeSinceActivity.TotalMinutes >= configuredTimeoutMinutes)
{
    // Mark previous session as FORCED_LOGOUT
    previousSession.Status = "FORCED_LOGOUT"
    previousSession.LogoutTime = DateTimeHelper.Now
    
    // Clear expired session
    user.ActiveSessionId = null
    user.SessionStartedAt = null
}
```

## What Changed

### ❌ Before (INCORRECT)
- SessionCleanupService marked sessions as FORCED_LOGOUT after 2 minutes
- Status changed based on temporary inactivity
- Hardcoded timeouts instead of using configured values

### ✅ After (CORRECT)
- SessionCleanupService only marks SESSION_TIMEOUT after configured timeout (default: 30 minutes)
- Status only changes when session actually ends
- Uses configurable timeout from database (SystemSettings table)
- FORCED_LOGOUT only happens when user tries to login again with an expired session

## Configuration

Session timeout is configurable via the Settings page:
- Default: 30 minutes
- Stored in: `SystemSettings` table
- Key: `SessionTimeoutMinutes`
- Category: `Security`

## Key Rules

1. **Only one session allowed per user** - No concurrent logins
2. **Middleware only updates LastActivityAt** - Never touches Status
3. **Status changes only in:**
   - Logout endpoint (LOGGED_OUT)
   - Timeout validation logic (SESSION_TIMEOUT)
   - Re-login detection (FORCED_LOGOUT)
4. **2 minutes inactivity ≠ session ended** - Session is valid until timeout threshold
5. **Status represents session termination reason** - Not temporary inactivity

## Testing

To test the fixed session lifecycle:

1. Login as any user
2. Stay active (heartbeats every 30 seconds)
3. Session should remain ACTIVE for the entire configured timeout period
4. After timeout period with no activity, session should be marked as SESSION_TIMEOUT
5. If you close browser without logout and login again, previous session should be marked as FORCED_LOGOUT

## Files Modified

- `Services/SessionCleanupService.cs` - Fixed timeout logic, uses configurable timeout
- `Controllers/AuthController.cs` - Added FORCED_LOGOUT detection on login
- `itams-frontend/src/app/services/auth.service.ts` - Removed incorrect beforeunload handler

## Date Fixed
February 20, 2026
