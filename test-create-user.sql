-- First, check what projects exist
SELECT Id, Code, Name FROM Projects WHERE IsActive = 1;

-- Check what roles exist
SELECT Id, Name FROM Roles WHERE IsActive = 1;

-- Create a test user with ProjectId = 2 (N_GUMA_65)
INSERT INTO Users (
    UserId,
    Username,
    Email,
    FirstName,
    LastName,
    RoleId,
    PasswordHash,
    IsActive,
    MustChangePassword,
    FailedLoginAttempts,
    CreatedAt,
    ProjectId
)
VALUES (
    'USR99999',
    'testuser123',
    'test@example.com',
    'Test',
    'User',
    4, -- Auditor role
    '$2a$11$abcdefghijklmnopqrstuv', -- Dummy hash
    1, -- IsActive
    1, -- MustChangePassword
    0, -- FailedLoginAttempts
    GETUTCDATE(),
    2 -- ProjectId (N_GUMA_65)
);

-- Verify the user was created
SELECT Id, UserId, Username, Email, FirstName, LastName, RoleId, ProjectId, IsActive 
FROM Users 
WHERE Username = 'testuser123';
