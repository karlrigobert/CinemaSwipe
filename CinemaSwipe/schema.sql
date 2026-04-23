-- Run this manually OR let EF Core auto-migrate on startup (Program.cs already calls db.Database.Migrate())
-- This is provided as reference for the DB schema

CREATE TABLE [AspNetUsers] (
    [Id]                   NVARCHAR(450)  NOT NULL PRIMARY KEY,
    [DisplayName]          NVARCHAR(256)  NOT NULL DEFAULT '',
    [AvatarUrl]            NVARCHAR(1024) NULL,
    [UserName]             NVARCHAR(256)  NULL,
    [NormalizedUserName]   NVARCHAR(256)  NULL,
    [Email]                NVARCHAR(256)  NULL,
    [NormalizedEmail]      NVARCHAR(256)  NULL,
    [EmailConfirmed]       BIT            NOT NULL DEFAULT 0,
    [PasswordHash]         NVARCHAR(MAX)  NULL,
    [SecurityStamp]        NVARCHAR(MAX)  NULL,
    [ConcurrencyStamp]     NVARCHAR(MAX)  NULL,
    [PhoneNumber]          NVARCHAR(MAX)  NULL,
    [PhoneNumberConfirmed] BIT            NOT NULL DEFAULT 0,
    [TwoFactorEnabled]     BIT            NOT NULL DEFAULT 0,
    [LockoutEnd]           DATETIMEOFFSET NULL,
    [LockoutEnabled]       BIT            NOT NULL DEFAULT 0,
    [AccessFailedCount]    INT            NOT NULL DEFAULT 0
);

CREATE TABLE [Rooms] (
    [Id]           INT            NOT NULL PRIMARY KEY IDENTITY,
    [Code]         NVARCHAR(6)    NOT NULL,
    [HostId]       NVARCHAR(450)  NOT NULL,
    [ContentType]  NVARCHAR(10)   NOT NULL DEFAULT 'movie',
    [Genre]        NVARCHAR(64)   NOT NULL DEFAULT '',
    [Status]       INT            NOT NULL DEFAULT 0,
    [WinnerTmdbId] INT            NULL,
    [WinnerTitle]  NVARCHAR(256)  NULL,
    [WinnerPoster] NVARCHAR(256)  NULL,
    [CreatedAt]    DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Rooms_Host] FOREIGN KEY ([HostId]) REFERENCES [AspNetUsers]([Id])
);
CREATE UNIQUE INDEX [IX_Rooms_Code] ON [Rooms]([Code]);

CREATE TABLE [RoomMembers] (
    [Id]              INT           NOT NULL PRIMARY KEY IDENTITY,
    [RoomId]          INT           NOT NULL,
    [UserId]          NVARCHAR(450) NOT NULL,
    [FinishedSwiping] BIT           NOT NULL DEFAULT 0,
    CONSTRAINT [FK_RoomMembers_Room] FOREIGN KEY ([RoomId]) REFERENCES [Rooms]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RoomMembers_User] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id])
);
CREATE UNIQUE INDEX [IX_RoomMembers_RoomUser] ON [RoomMembers]([RoomId], [UserId]);

CREATE TABLE [RoomFilms] (
    [Id]          INT            NOT NULL PRIMARY KEY IDENTITY,
    [RoomId]      INT            NOT NULL,
    [TmdbId]      INT            NOT NULL,
    [Title]       NVARCHAR(256)  NOT NULL,
    [PosterPath]  NVARCHAR(256)  NOT NULL DEFAULT '',
    [Overview]    NVARCHAR(2048) NOT NULL DEFAULT '',
    [Rating]      FLOAT          NOT NULL DEFAULT 0,
    [Position]    INT            NOT NULL,
    CONSTRAINT [FK_RoomFilms_Room] FOREIGN KEY ([RoomId]) REFERENCES [Rooms]([Id]) ON DELETE CASCADE
);

CREATE TABLE [Votes] (
    [Id]         INT           NOT NULL PRIMARY KEY IDENTITY,
    [RoomFilmId] INT           NOT NULL,
    [UserId]     NVARCHAR(450) NOT NULL,
    [IsLike]     BIT           NOT NULL,
    CONSTRAINT [FK_Votes_RoomFilm] FOREIGN KEY ([RoomFilmId]) REFERENCES [RoomFilms]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Votes_User]     FOREIGN KEY ([UserId])     REFERENCES [AspNetUsers]([Id])
);
CREATE UNIQUE INDEX [IX_Votes_FilmUser] ON [Votes]([RoomFilmId], [UserId]);
