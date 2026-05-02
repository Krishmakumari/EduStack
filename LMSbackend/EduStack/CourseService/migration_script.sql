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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406062645_InitialMigration'
)
BEGIN
    CREATE TABLE [Courses] (
        [CourseId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        [ThumbnailUrl] nvarchar(2048) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Level] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Language] nvarchar(50) NOT NULL,
        [InstructorId] uniqueidentifier NOT NULL,
        [InstructorName] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([CourseId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406062645_InitialMigration'
)
BEGIN
    CREATE TABLE [Sections] (
        [SectionId] uniqueidentifier NOT NULL,
        [CourseId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Order] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Sections] PRIMARY KEY ([SectionId]),
        CONSTRAINT [FK_Sections_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406062645_InitialMigration'
)
BEGIN
    CREATE TABLE [Lessons] (
        [LessonId] uniqueidentifier NOT NULL,
        [SectionId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [VideoUrl] nvarchar(2048) NULL,
        [Content] nvarchar(max) NULL,
        [DurationInSeconds] int NOT NULL,
        [Order] int NOT NULL,
        [IsFreePreview] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Lessons] PRIMARY KEY ([LessonId]),
        CONSTRAINT [FK_Lessons_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([SectionId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406062645_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Lessons_SectionId] ON [Lessons] ([SectionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406062645_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Sections_CourseId] ON [Sections] ([CourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406062645_InitialMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406062645_InitialMigration', N'8.0.0');
END;
GO

COMMIT;
GO

