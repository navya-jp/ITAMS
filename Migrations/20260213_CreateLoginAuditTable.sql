-- Migration: Create LoginAudit table for tamper-proof audit logging
-- Date: 2026-02-13
-- Description: Create table to store login audit information including IP, browser, OS, and session details

CREATE TABLE LoginAudit (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    LoginTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IpAddress NVARCHAR(50) NULL,
    BrowserType NVARCHAR(200) NULL,
    OperatingSystem NVARCHAR(200) NULL,
    SessionId NVARCHAR(500) NOT NULL,
    LogoutTime DATETIME2 NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    
    CONSTRAINT FK_LoginAudit_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

GO

-- Create indexes for better query performance
CREATE INDEX IX_LoginAudit_UserId ON LoginAudit(UserId);
CREATE INDEX IX_LoginAudit_SessionId ON LoginAudit(SessionId);
CREATE INDEX IX_LoginAudit_LoginTime ON LoginAudit(LoginTime DESC);
CREATE INDEX IX_LoginAudit_Status ON LoginAudit(Status);

GO

PRINT 'LoginAudit table created successfully';
