# Production-Level Session Architecture

## Overview
This document describes the complete, production-ready session management system implementing a single-session-per-user architecture with proper audit trail.

## Core Principles

1. **Only ONE session allowed per user** - No concurrent logins
2. **Status represents HOW session ended** - Not temporary inactivity
3. **ACTIVE stays ACTIVE until real end** - No premature timeouts
4. **Clean separation of concerns** - Each component has one responsibility

## Session States

| Status | Meaning | Triggered By |
|--------|---------|--------------|
| `ACTIVE` | User is currently logged in | Login endpoint |
| `LOGGED_OUT` | User clicked logout button | Logout endpoint |
| `SESSION_TIMEOUT` | Session expired due to inactivity | SessionCleanupService (background) |
| `FORCED_LOGOUT` | Browser closed without logout | Login endpoint (on next login) |

## Architecture Components

### 1. SessionCleanupService (Background Service)

**Responsibility:** Handle SESSION_TIMEOUT only

**How it works:**
- Runs every 1 minute
- Loads configurable timeout from database (default: 30 minutes)
- Finds users where `LastActivityAt <= (now - timeout)`
- Marks their audit as `SESSION_TIMEOUT`
- Clears their session

**Key improvements:**
- Filters expired sessions at DB level (efficient)
- No memory loading of all users
- Clean change tracking
- Uses IST timezone consistently

```csharp
// Pseudo-code
var cutoffTime = now.AddMinutes(-timeoutMinutes);
var expiredUsers = await context.Users
    .Where(u => u.ActiveSessionId != null && 
                u.LastActivityAt <= cutoffTime)
    .ToListAsync();

foreach (var user in expiredUsers)
{
    activeAudit.Status = "SESSION_TIMEOUT";
    activeAudit.LogoutTime = now;
    user.ActiveSessionId = null;
}
```

### 2. ActivityTrackingMiddleware

**Responsibility:** Update LastActivityAt only

**How it works:**
- Runs on every authenticated request
- Updates `LastActivityAt` to current IST time
- Never touches audit status
- Uses EF tracking (not raw SQL)

**Key improvements:**
- Only updates if session exists
- Safe and clean
- No audit corruption
- Consistent timezone (IST)

```csharp
// Pseudo-code
if (user.IsAuthenticated && user.HasActiveSession)
{
    user.LastActivityAt = DateTimeHelper.Now;
    await context.SaveChangesAsync();
}
```

### 3. Login Endpoint

**Responsibility:** Create new session, handle old session detection

**How it works:**
1. Check if user has existing `ActiveSessionId`
2. If yes, calculate inactivity time
3. If inactivity >= timeout → mark old session as `SESSION_TIMEOUT`
4. If inactivity < timeout → mark old session as `FORCED_LOGOUT` (browser closed)
5. Create new `ACTIVE` session
6. Set `LastActivityAt = now` (CRITICAL!)

**Key improvements:**
- Proper FORCED_LOGOUT detection
- Clean session transition
- Initial activity time set correctly

```csharp
// Pseudo-code
if (user.HasActiveSession)
{
    var inactiveMinutes = (now - lastActivity).TotalMinutes;
    
    if (inactiveMinutes >= timeoutMinutes)
    {
        oldAudit.Status = "SESSION_TIMEOUT";
    }
    else
    {
        oldAudit.Status = "FORCED_LOGOUT"; // Browser closed
    }
}

// Create new session
user.ActiveSessionId = newSessionId;
user.SessionStartedAt = now;
user.LastActivityAt = now; // CRITICAL!

context.LoginAudits.Add(new LoginAudit {
    Status = "ACTIVE",
    LoginTime = now,
    ...
});
```

### 4. Logout Endpoint

**Responsibility:** Handle manual logout only

**How it works:**
1. Find `ACTIVE` audit record
2. Mark as `LOGGED_OUT`
3. Set `LogoutTime = now`
4. Clear session

**Key improvements:**
- Simple and clean
- Only handles explicit logout
- No complex logic

```csharp
// Pseudo-code
var activeAudit = await context.LoginAudits
    .Where(a => a.UserId == userId && a.Status == "ACTIVE")
    .FirstOrDefaultAsync();

if (activeAudit != null)
{
    activeAudit.Status = "LOGGED_OUT";
    activeAudit.LogoutTime = now;
}

user.ActiveSessionId = null;
```

## Complete Session Lifecycle

```
USER LOGS IN
    ↓
Status = ACTIVE
SessionStartedAt = now
LastActivityAt = now  ← CRITICAL!
    ↓
USER ACTIVE (sending requests)
    ↓
Middleware updates LastActivityAt = now
    ↓
Status still ACTIVE ✓
    ↓
┌─────────────────────────────────────┐
│ THREE POSSIBLE ENDINGS:             │
├─────────────────────────────────────┤
│ 1. User clicks logout               │
│    → Status = LOGGED_OUT            │
│                                     │
│ 2. User inactive >= 30 minutes     │
│    → Status = SESSION_TIMEOUT       │
│    (SessionCleanupService)          │
│                                     │
│ 3. User closes browser              │
│    → Later logs in again            │
│    → Old session = FORCED_LOGOUT    │
│    (Login endpoint detection)       │
└─────────────────────────────────────┘
```

## Timezone Consistency

**CRITICAL:** All timestamps use IST (Indian Standard Time)

- `DateTimeHelper.Now` returns IST time
- All components use `DateTimeHelper.Now`
- No mixing of UTC and IST
- Prevents 5.5-hour timezone bugs

## Configuration

Session timeout is configurable:
- Location: `SystemSettings` table
- Key: `SessionTimeoutMinutes`
- Category: `Security`
- Default: 30 minutes
- Editable via Settings page

## What Will NEVER Happen

❌ No 1-minute fake timeout  
❌ No 2-minute weird behavior  
❌ No random status changes  
❌ No forced logout due to inactivity  
❌ No audit corruption  
❌ No timezone mismatches  
❌ No concurrent sessions  

## What WILL Happen

✅ ACTIVE stays ACTIVE until real end  
✅ SESSION_TIMEOUT only after configured minutes  
✅ LOGGED_OUT only when user clicks logout  
✅ FORCED_LOGOUT only when browser closed  
✅ Clean audit trail  
✅ Consistent behavior  

## Design Patterns Used

1. **Hosted Service Pattern** - SessionCleanupService
2. **Middleware Pipeline Pattern** - ActivityTrackingMiddleware
3. **Token-Based Authentication** - JWT tokens
4. **Configurable Security Policy** - Database-driven timeout
5. **Audit Trail Pattern** - LoginAudit table
6. **Single Session Enforcement** - One session per user
7. **Clean State Machine** - Explicit state transitions

## Interview Answer

**Q: "How does your session management work?"**

**A:** "We enforce single-session login. A session remains ACTIVE until one of three events: explicit logout, inactivity exceeding a configurable timeout handled by a background cleanup service, or implicit termination detected during next login which is marked as FORCED_LOGOUT. Activity is tracked via middleware, and session expiration is handled centrally to maintain audit integrity. All timestamps use IST timezone consistently to prevent timezone-related bugs."

## Testing Scenarios

### Scenario 1: Normal Logout
1. User logs in → Status = ACTIVE
2. User clicks logout → Status = LOGGED_OUT
3. ✅ Expected: Audit shows LOGGED_OUT

### Scenario 2: Session Timeout
1. User logs in → Status = ACTIVE
2. User inactive for 31 minutes
3. SessionCleanupService runs → Status = SESSION_TIMEOUT
4. ✅ Expected: Audit shows SESSION_TIMEOUT

### Scenario 3: Browser Closed
1. User logs in → Status = ACTIVE
2. User closes browser (no logout)
3. User logs in again after 5 minutes
4. Login endpoint detects old session → Status = FORCED_LOGOUT
5. New session created → Status = ACTIVE
6. ✅ Expected: Old audit shows FORCED_LOGOUT, new audit shows ACTIVE

### Scenario 4: Active User
1. User logs in → Status = ACTIVE
2. User active for 2 hours (sending requests)
3. Middleware updates LastActivityAt continuously
4. ✅ Expected: Status remains ACTIVE (no timeout)

## Files Modified

- `Services/SessionCleanupService.cs` - Production-level cleanup logic
- `Middleware/ActivityTrackingMiddleware.cs` - Clean activity tracking
- `Controllers/AuthController.cs` - Proper login/logout logic
- `Services/UserService.cs` - Set LastActivityAt on session creation
- `Utilities/DateTimeHelper.cs` - IST timezone helper

## Date Implemented
February 20, 2026

## Architecture Level
Production-ready, enterprise-grade session management system.
