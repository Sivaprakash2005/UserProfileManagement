-- =============================================
-- User Profile Management
-- Database Script
-- =============================================

CREATE DATABASE UserProfileDB;
GO

USE UserProfileDB;
GO

-- =============================================
-- Create UserProfiles Table
-- =============================================

CREATE TABLE UserProfiles
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,

    FullName NVARCHAR(100) NOT NULL,

    Email NVARCHAR(150) NOT NULL,

    PhoneNumber NVARCHAR(15) NULL,

    DateOfBirth DATE NULL,

    Address NVARCHAR(250) NULL,

    ProfilePicture NVARCHAR(255) NULL,

    CreatedAt DATETIME2 NOT NULL
        DEFAULT GETDATE(),

    UpdatedAt DATETIME2 NOT NULL
        DEFAULT GETDATE()
);
GO

-- =============================================
-- Unique Index for Email
-- =============================================

CREATE UNIQUE INDEX IX_UserProfiles_Email
ON UserProfiles(Email);
GO

-- =============================================
-- Sample User Data
-- =============================================

INSERT INTO UserProfiles
(
    FullName,
    Email,
    PhoneNumber,
    DateOfBirth,
    Address,
    ProfilePicture
)
VALUES
(
    'Siva Prakash',
    'siva@example.com',
    '9876543210',
    '2003-05-10',
    'Madurai',
    NULL
);
GO

-- =============================================
-- Display User Profiles
-- =============================================

SELECT * FROM UserProfiles;
GO