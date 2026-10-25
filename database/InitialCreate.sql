IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(320) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [CreatedById] int NULL,
    [UpdatedById] int NULL,
    [DeletedById] int NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [MentorSkills] (
    [Id] int NOT NULL IDENTITY,
    [MentorId] int NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [CreatedById] int NULL,
    [UpdatedById] int NULL,
    [DeletedById] int NULL,
    CONSTRAINT [PK_MentorSkills] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MentorSkills_Users_MentorId] FOREIGN KEY ([MentorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [MentorshipRequests] (
    [Id] int NOT NULL IDENTITY,
    [MentorId] int NOT NULL,
    [MenteeId] int NOT NULL,
    [SkillId] int NOT NULL,
    [RequestedStartAt] datetime2 NOT NULL,
    [DurationInMinutes] int NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [CreatedById] int NULL,
    [UpdatedById] int NULL,
    [DeletedById] int NULL,
    CONSTRAINT [PK_MentorshipRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MentorshipRequests_MentorSkills_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [MentorSkills] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MentorshipRequests_Users_MenteeId] FOREIGN KEY ([MenteeId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MentorshipRequests_Users_MentorId] FOREIGN KEY ([MentorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_MentorshipRequests_MenteeId] ON [MentorshipRequests] ([MenteeId]);

CREATE INDEX [IX_MentorshipRequests_MentorId] ON [MentorshipRequests] ([MentorId]);

CREATE INDEX [IX_MentorshipRequests_SkillId] ON [MentorshipRequests] ([SkillId]);

CREATE UNIQUE INDEX [IX_MentorSkills_MentorId_Name] ON [MentorSkills] ([MentorId], [Name]) WHERE [DeletedAt] IS NULL;

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [DeletedAt] IS NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261025125645_InitialCreate', N'10.0.13');

COMMIT;
GO

