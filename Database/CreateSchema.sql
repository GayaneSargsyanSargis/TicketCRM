-- Drop existing tables if they exist (for clean migration)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Comments')
    DROP TABLE Comments;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tickets')
    DROP TABLE Tickets;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AuditEntries')
    DROP TABLE AuditEntries;

-- Create Tickets table
CREATE TABLE Tickets (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Subject NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    CustomerEmail NVARCHAR(255) NOT NULL,
    Priority INT NOT NULL,
    Status INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    ResolvedAt DATETIME2 NULL
);

-- Create Comments table
CREATE TABLE Comments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TicketId INT NOT NULL,
    Author NVARCHAR(255) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    Internal BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    CONSTRAINT FK_Comments_Tickets FOREIGN KEY (TicketId) REFERENCES Tickets(Id) ON DELETE CASCADE
);

-- Create AuditEntries table
CREATE TABLE AuditEntries (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Actor NVARCHAR(255) NOT NULL,
    Action INT NOT NULL,
    TicketId INT NULL,
    At DATETIME2 NOT NULL,
    Details NVARCHAR(MAX) NOT NULL
);

-- Create indexes for better query performance
CREATE INDEX IX_Tickets_Status ON Tickets(Status);
CREATE INDEX IX_Tickets_Priority ON Tickets(Priority);
CREATE INDEX IX_Tickets_CreatedAt ON Tickets(CreatedAt);
CREATE INDEX IX_Comments_TicketId ON Comments(TicketId);
CREATE INDEX IX_AuditEntries_TicketId ON AuditEntries(TicketId);
CREATE INDEX IX_AuditEntries_At ON AuditEntries(At);

-- Connection string example (to be used in application settings, not in SQL file)
-- "DefaultConnection": "Server=YOUR_SERVER;Database=TicketCRM;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=true;"