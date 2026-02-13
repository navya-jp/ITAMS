-- Migration: Distribute users across projects with location restrictions
-- Date: 2026-02-13
-- Description: Assign users to different projects with hierarchical location restrictions

-- Get project IDs
DECLARE @DefaultProjectId INT = (SELECT Id FROM Projects WHERE Code = 'DEFAULT');
DECLARE @GujaratProject1 INT = (SELECT Id FROM Projects WHERE Code = 'SH_GU_74');
DECLARE @GujaratProject2 INT = (SELECT Id FROM Projects WHERE Code = 'N_GUMA_65');
DECLARE @HaryanaProject INT = (SELECT Id FROM Projects WHERE Code = 'H_HA_76');
DECLARE @AssamProject INT = (SELECT Id FROM Projects WHERE Code = 'P_AS_57');

-- SuperAdmin stays in DEFAULT project with no restrictions (access to all)
UPDATE Users SET ProjectId = @DefaultProjectId, 
    RestrictedRegion = NULL, RestrictedState = NULL, RestrictedPlaza = NULL, RestrictedOffice = NULL
WHERE RoleId = 1; -- SuperAdmin

-- Distribute other users across projects
-- Gujarat Project 1 (SH_GU_74) - 5 users
UPDATE Users SET ProjectId = @GujaratProject1, RestrictedRegion = NULL, RestrictedState = 'Gujarat', RestrictedPlaza = NULL, RestrictedOffice = NULL
WHERE Username IN ('kriya.morabia', 'navya.pradeepkumar', 'john.doe');

UPDATE Users SET ProjectId = @GujaratProject1, RestrictedRegion = NULL, RestrictedState = 'Gujarat', RestrictedPlaza = 'Ahmedabad Plaza', RestrictedOffice = NULL
WHERE Username IN ('nsk.mahe', 'shaku.mehta');

-- Gujarat/Maharashtra Project (N_GUMA_65) - 5 users
UPDATE Users SET ProjectId = @GujaratProject2, RestrictedRegion = NULL, RestrictedState = 'Gujarat', RestrictedPlaza = NULL, RestrictedOffice = NULL
WHERE Username IN ('one.two', 'newuser123');

UPDATE Users SET ProjectId = @GujaratProject2, RestrictedRegion = NULL, RestrictedState = 'Maharashtra', RestrictedPlaza = NULL, RestrictedOffice = NULL
WHERE Username IN ('test.user', 'demo.user');

UPDATE Users SET ProjectId = @GujaratProject2, RestrictedRegion = NULL, RestrictedState = NULL, RestrictedPlaza = NULL, RestrictedOffice = NULL
WHERE Username = 'admin.user'; -- Full access within project

-- Haryana Project (H_HA_76) - 4 users
UPDATE Users SET ProjectId = @HaryanaProject, RestrictedRegion = NULL, RestrictedState = 'Haryana', RestrictedPlaza = NULL, RestrictedOffice = NULL
WHERE Username IN ('haryana.user1', 'haryana.user2', 'haryana.admin');

UPDATE Users SET ProjectId = @HaryanaProject, RestrictedRegion = NULL, RestrictedState = 'Haryana', RestrictedPlaza = 'Gurgaon Plaza', RestrictedOffice = 'Office A'
WHERE Username = 'haryana.restricted';

-- Assam Project (P_AS_57) - Remaining users
UPDATE Users SET ProjectId = @AssamProject, RestrictedRegion = NULL, RestrictedState = 'Assam', RestrictedPlaza = NULL, RestrictedOffice = NULL
WHERE ProjectId = @DefaultProjectId AND RoleId != 1; -- All remaining non-superadmin users

-- Show distribution
SELECT 
    p.Code as ProjectCode,
    p.Name as ProjectName,
    p.States,
    COUNT(u.Id) as UserCount,
    STRING_AGG(u.Username, ', ') as Users
FROM Projects p
LEFT JOIN Users u ON p.Id = u.ProjectId
GROUP BY p.Id, p.Code, p.Name, p.States
ORDER BY p.Code;

-- Show users with location restrictions
SELECT 
    u.Username,
    p.Code as ProjectCode,
    r.Name as RoleName,
    u.RestrictedState,
    u.RestrictedPlaza,
    u.RestrictedOffice,
    CASE 
        WHEN u.RestrictedOffice IS NOT NULL THEN 'Office Level'
        WHEN u.RestrictedPlaza IS NOT NULL THEN 'Plaza Level'
        WHEN u.RestrictedState IS NOT NULL THEN 'State Level'
        WHEN u.RestrictedRegion IS NOT NULL THEN 'Region Level'
        ELSE 'Full Project Access'
    END as AccessLevel
FROM Users u
INNER JOIN Projects p ON u.ProjectId = p.Id
INNER JOIN Roles r ON u.RoleId = r.Id
ORDER BY p.Code, u.Username;
