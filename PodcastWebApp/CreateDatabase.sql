USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'PodcastDB')
BEGIN
    ALTER DATABASE PodcastDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PodcastDB;
END
GO
CREATE DATABASE PodcastDB;
USE PodcastDB;

CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) PRIMARY KEY,
    UserName NVARCHAR(256),
    NormalizedUserName NVARCHAR(256),
    Email NVARCHAR(256),
    NormalizedEmail NVARCHAR(256),
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    ConcurrencyStamp NVARCHAR(MAX),
    PhoneNumber NVARCHAR(MAX),
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIMEOFFSET,
    LockoutEnabled BIT NOT NULL DEFAULT 0,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    FirstName NVARCHAR(255) NOT NULL,
    LastName NVARCHAR(255) NOT NULL,
    AvatarURL NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME NOT NULL
);

CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) PRIMARY KEY,
    Name NVARCHAR(256),
    NormalizedName NVARCHAR(256)
);

CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE Podcasts (
    PodcastID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    CoverImageURL NVARCHAR(MAX) NOT NULL,
    CreatorID NVARCHAR(450) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    FOREIGN KEY (CreatorID) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE Episodes (
    EpisodeID INT IDENTITY(1,1) PRIMARY KEY,
    PodcastID INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    AudioFileURL NVARCHAR(MAX) NOT NULL,
    ThumbnailURL NVARCHAR(MAX) NOT NULL,
    Duration INT NOT NULL,
    ReleaseDate DATETIME NOT NULL,
    Views INT NOT NULL DEFAULT 0,
    PlayCount INT NOT NULL DEFAULT 0,
    Host NVARCHAR(255) NOT NULL,
    Topic NVARCHAR(255) NOT NULL,
    IsApproved BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (PodcastID) REFERENCES Podcasts(PodcastID) ON DELETE CASCADE
);

INSERT INTO AspNetRoles (Id, Name, NormalizedName)
VALUES 
('1', 'Admin', 'ADMIN'),
('2', 'Podcaster', 'PODCASTER'),
('3', 'Listener', 'LISTENER');

INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName, AvatarURL, CreatedAt)
VALUES ('1', 'admin', 'ADMIN', 'admin@podcast.com', 'ADMIN@PODCAST.COM', 1, '', NEWID(), NEWID(), NULL, 0, 0, 0, 0, 'Admin', 'User', '', GETDATE());

SET IDENTITY_INSERT Podcasts ON;
INSERT INTO Podcasts (PodcastID, Title, Description, Category, CoverImageURL, CreatorID, CreatedDate)
VALUES (1, 'The Deep Dive', 'Everything and anything in depth', 'Education', '', '1', GETDATE());
SET IDENTITY_INSERT Podcasts OFF;

INSERT INTO Episodes (PodcastID, Title, Description, AudioFileURL, ThumbnailURL, Duration, ReleaseDate, Views, PlayCount, Host, Topic, IsApproved)
VALUES 
(1, 'From Complements Rule to the Central Limit Theorem', 'Statistics and probability theory discussion', 'https://podcast-media-files-rb-2025.s3.us-east-1.amazonaws.com/From_Complements_Rule_to_the_Central_Limit_Theorem__Mastering_S.mp3', '', 498, GETDATE(), 0, 0, 'Statistics Professor', 'Mathematics', 1),
(1, 'Stop Coding and Start Countering', 'Software development strategies and best practices', 'https://podcast-media-files-rb-2025.s3.us-east-1.amazonaws.com/Stop_Coding_and_Start_Countering__How_to_Use_Misuse_Cases_and_t.mp3', '', 498, GETDATE(), 0, 0, 'Tech Lead', 'Technology', 1),
(1, 'The Strategy of Communication', 'Effective communication techniques and strategies', 'https://podcast-media-files-rb-2025.s3.us-east-1.amazonaws.com/The_Strategy_of_Communication__Master_Audience_Analysis,_Cultur.mp3', '', 758, GETDATE(), 0, 0, 'Communication Expert', 'Business', 1);
