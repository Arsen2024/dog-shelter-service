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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE TABLE [Breeds] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_Breeds] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE TABLE [Features] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_Features] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE TABLE [Kennels] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        [Capacity] int NOT NULL,
        CONSTRAINT [PK_Kennels] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Kennels_Capacity] CHECK ([Capacity] > 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE TABLE [Dogs] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Color] nvarchar(100) NOT NULL,
        [TailLength] int NOT NULL,
        [Weight] decimal(6,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL DEFAULT N'Available',
        [BreedId] uniqueidentifier NOT NULL,
        [KennelId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [CreatedBy] nvarchar(100) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeletedAt] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Dogs] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Dogs_Status] CHECK ([Status] IN ('Available', 'Reserved', 'Adopted', 'UnderTreatment')),
        CONSTRAINT [CK_Dogs_TailLength] CHECK ([TailLength] >= 0),
        CONSTRAINT [CK_Dogs_Weight] CHECK ([Weight] > 0),
        CONSTRAINT [FK_Dogs_Breeds_BreedId] FOREIGN KEY ([BreedId]) REFERENCES [Breeds] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Dogs_Kennels_KennelId] FOREIGN KEY ([KennelId]) REFERENCES [Kennels] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE TABLE [DogFeatures] (
        [DogId] uniqueidentifier NOT NULL,
        [FeatureId] uniqueidentifier NOT NULL,
        [AssignedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_DogFeatures] PRIMARY KEY ([DogId], [FeatureId]),
        CONSTRAINT [FK_DogFeatures_Dogs_DogId] FOREIGN KEY ([DogId]) REFERENCES [Dogs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DogFeatures_Features_FeatureId] FOREIGN KEY ([FeatureId]) REFERENCES [Features] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE TABLE [DogProfiles] (
        [DogId] uniqueidentifier NOT NULL,
        [BirthDate] datetime2 NULL,
        [Gender] nvarchar(20) NULL,
        [Description] nvarchar(1000) NULL,
        [Temperament] nvarchar(500) NULL,
        CONSTRAINT [PK_DogProfiles] PRIMARY KEY ([DogId]),
        CONSTRAINT [CK_DogProfiles_Gender] CHECK ([Gender] IS NULL OR [Gender] IN ('Male', 'Female', 'Unknown')),
        CONSTRAINT [FK_DogProfiles_Dogs_DogId] FOREIGN KEY ([DogId]) REFERENCES [Dogs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Breeds]'))
        SET IDENTITY_INSERT [Breeds] ON;
    EXEC(N'INSERT INTO [Breeds] ([Id], [Description], [Name])
    VALUES (''11111111-1111-1111-1111-111111111111'', N''Friendly and intelligent dog breed.'', N''Labrador Retriever''),
    (''22222222-2222-2222-2222-222222222222'', N''Intelligent and versatile working dog breed.'', N''German Shepherd''),
    (''33333333-3333-3333-3333-333333333333'', N''Friendly and curious hunting dog breed.'', N''Beagle''),
    (''44444444-4444-4444-4444-444444444444'', N''Dog with mixed ancestry.'', N''Mixed Breed'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Breeds]'))
        SET IDENTITY_INSERT [Breeds] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Features]'))
        SET IDENTITY_INSERT [Features] ON;
    EXEC(N'INSERT INTO [Features] ([Id], [Description], [Name])
    VALUES (''12345678-1234-1234-1234-123456789012'', N''Has a high activity level and enjoys exercise.'', N''Active''),
    (''dddddddd-dddd-dddd-dddd-dddddddddddd'', N''Friendly towards people and other animals.'', N''Friendly''),
    (''eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'', N''Has completed basic obedience training.'', N''Trained''),
    (''ffffffff-ffff-ffff-ffff-ffffffffffff'', N''Comfortable interacting with children.'', N''Good with children'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Features]'))
        SET IDENTITY_INSERT [Features] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Capacity', N'Location', N'Name') AND [object_id] = OBJECT_ID(N'[Kennels]'))
        SET IDENTITY_INSERT [Kennels] ON;
    EXEC(N'INSERT INTO [Kennels] ([Id], [Capacity], [Location], [Name])
    VALUES (''aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'', 10, N''Shelter Building 1'', N''Kennel A''),
    (''bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'', 15, N''Shelter Building 2'', N''Kennel B''),
    (''cccccccc-cccc-cccc-cccc-cccccccccccc'', 8, N''Shelter Building 3'', N''Kennel C'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Capacity', N'Location', N'Name') AND [object_id] = OBJECT_ID(N'[Kennels]'))
        SET IDENTITY_INSERT [Kennels] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Breeds_Name] ON [Breeds] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DogFeatures_FeatureId_AssignedAt] ON [DogFeatures] ([FeatureId], [AssignedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Dogs_BreedId_Status_Active] ON [Dogs] ([BreedId], [Status]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Dogs_KennelId_Active] ON [Dogs] ([KennelId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Dogs_Name_Active] ON [Dogs] ([Name]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Dogs_Status_Active] ON [Dogs] ([Status]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Features_Name] ON [Features] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Kennels_Name] ON [Kennels] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124095954_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261124095954_InitialCreate', N'9.0.1');
END;

COMMIT;
GO

