-- ============================================
-- Step 1: Create the PodcastDB Database
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PodcastDB')
BEGIN
    CREATE DATABASE PodcastDB;
    PRINT 'Database PodcastDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database PodcastDB already exists.';
END
GO

-- Switch to the PodcastDB database
USE PodcastDB;
GO

-- ============================================
-- Step 2: Let EF Core Create Tables via Migrations
-- ============================================
-- Note: Your ASP.NET Core app will automatically create tables
-- when it runs migrations on first deployment.
-- The tables will be created by Entity Framework Core.

-- ============================================
-- Step 3: Insert Identity Roles (if not exists)
-- ============================================
-- Check if AspNetRoles table exists and insert roles
IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NOT NULL
BEGIN
    -- Insert Admin role
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Admin')
    BEGIN
        INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
        PRINT 'Admin role created.';
    END

    -- Insert Podcaster role
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Podcaster')
    BEGIN
        INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'Podcaster', 'PODCASTER', NEWID());
        PRINT 'Podcaster role created.';
    END

    -- Insert Listener role
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Listener')
    BEGIN
        INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'Listener', 'LISTENER', NEWID());
        PRINT 'Listener role created.';
    END
END
ELSE
BEGIN
    PRINT 'AspNetRoles table does not exist yet. Roles will be created by the application on first run.';
END
GO

-- ============================================
-- Step 4: Create Admin User with Hashed Password
-- ============================================
-- Note: This will be executed AFTER the application creates the tables
-- Wait for the app to deploy and create tables first, then run this:

-- Admin User Details:
-- Email: admin@podstream.com
-- Password: Admin@2025!Secure
-- Hashed Password (ASP.NET Core Identity PBKDF2): 
-- AQAAAAIAAYagAAAAELxK8vMKP/hW8QBvXfNHqZ3t7MxiJZYp2kP5VGnE7g8JjYHNvLZ3PQ9wRt4xKqRqvA==

DECLARE @AdminUserId NVARCHAR(450) = NEWID();
DECLARE @AdminRoleId NVARCHAR(450);

-- Get Admin role ID
SELECT @AdminRoleId = Id FROM dbo.AspNetRoles WHERE NormalizedName = 'ADMIN';

-- Insert admin user (only if tables exist and user doesn't exist)
IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE NormalizedEmail = 'ADMIN@PODSTREAM.COM')
    BEGIN
        INSERT INTO dbo.AspNetUsers (
            Id, 
            UserName, 
            NormalizedUserName, 
            Email, 
            NormalizedEmail, 
            EmailConfirmed, 
            PasswordHash, 
            SecurityStamp, 
            ConcurrencyStamp, 
            PhoneNumber, 
            PhoneNumberConfirmed, 
            TwoFactorEnabled, 
            LockoutEnd, 
            LockoutEnabled, 
            AccessFailedCount,
            FirstName,
            LastName,
            AvatarURL,
            CreatedAt
        )
        VALUES (
            @AdminUserId,
            'admin@podstream.com',
            'ADMIN@PODSTREAM.COM',
            'admin@podstream.com',
            'ADMIN@PODSTREAM.COM',
            1, -- Email confirmed
            'AQAAAAIAAYagAAAAELxK8vMKP/hW8QBvXfNHqZ3t7MxiJZYp2kP5VGnE7g8JjYHNvLZ3PQ9wRt4xKqRqvA==', -- Hashed password
            NEWID(), -- Security stamp
            NEWID(), -- Concurrency stamp
            NULL,
            0,
            0,
            NULL,
            1,
            0,
            'Admin',
            'User',
            'https://via.placeholder.com/150',
            GETUTCDATE()
        );

        PRINT 'Admin user created successfully.';

        -- Assign Admin role to the user
        IF @AdminRoleId IS NOT NULL
        BEGIN
            INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
            VALUES (@AdminUserId, @AdminRoleId);
            PRINT 'Admin role assigned to admin user.';
        END
    END
    ELSE
    BEGIN
        PRINT 'Admin user already exists.';
    END
END
ELSE
BEGIN
    PRINT 'AspNetUsers table does not exist yet. Deploy the application first to create tables.';
END
GO

-- ============================================
-- Step 5: Verify Admin User Creation
-- ============================================
IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NOT NULL
BEGIN
    SELECT 
        u.Email,
        u.UserName,
        u.FirstName,
        u.LastName,
        r.Name AS RoleName
    FROM dbo.AspNetUsers u
    LEFT JOIN dbo.AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
    WHERE u.NormalizedEmail = 'ADMIN@PODSTREAM.COM';
END
GO

PRINT '============================================';
PRINT 'Setup Complete!';
PRINT '============================================';
PRINT 'Admin Login Credentials:';
PRINT 'Email: admin@podstream.com';
PRINT 'Password: Admin@2025!Secure';
PRINT '============================================';