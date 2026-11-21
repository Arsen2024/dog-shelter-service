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
    WHERE [MigrationId] = N'20251030171236_InitialCreate'
)
BEGIN
    CREATE TABLE [Dogs] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Color] nvarchar(max) NOT NULL,
        [TailLength] int NOT NULL,
        [Weight] int NOT NULL,
        CONSTRAINT [PK_Dogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251030171236_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251030171236_InitialCreate', N'9.0.1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Dogs]') AND [c].[name] = N'Weight');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Dogs] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Dogs] ALTER COLUMN [Weight] decimal(6,2) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Dogs]') AND [c].[name] = N'Name');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Dogs] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Dogs] ALTER COLUMN [Name] nvarchar(100) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Dogs]') AND [c].[name] = N'Color');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Dogs] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Dogs] ALTER COLUMN [Color] nvarchar(100) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [BreedId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [CreatedBy] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [KennelId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [RowVersion] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [Status] nvarchar(30) NOT NULL DEFAULT N'Available';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD [UpdatedBy] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
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
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Dogs]') AND [c].[name] = N'BreedId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Dogs] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Dogs] ALTER COLUMN [BreedId] uniqueidentifier NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE TABLE [DogProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [DogId] uniqueidentifier NOT NULL,
        [BirthDate] datetime2 NULL,
        [Gender] nvarchar(20) NULL,
        [Description] nvarchar(1000) NULL,
        [Temperament] nvarchar(500) NULL,
        CONSTRAINT [PK_DogProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DogProfiles_Dogs_DogId] FOREIGN KEY ([DogId]) REFERENCES [Dogs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
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
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
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
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
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
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE INDEX [IX_Dogs_BreedId] ON [Dogs] ([BreedId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE INDEX [IX_Dogs_IsDeleted] ON [Dogs] ([IsDeleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE INDEX [IX_Dogs_KennelId] ON [Dogs] ([KennelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE INDEX [IX_Dogs_Status] ON [Dogs] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    EXEC(N'ALTER TABLE [Dogs] ADD CONSTRAINT [CK_Dogs_Status] CHECK ([Status] IN (''Available'', ''Reserved'', ''Adopted'', ''UnderTreatment''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    EXEC(N'ALTER TABLE [Dogs] ADD CONSTRAINT [CK_Dogs_TailLength] CHECK ([TailLength] >= 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    EXEC(N'ALTER TABLE [Dogs] ADD CONSTRAINT [CK_Dogs_Weight] CHECK ([Weight] > 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Breeds_Name] ON [Breeds] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE INDEX [IX_DogFeatures_FeatureId] ON [DogFeatures] ([FeatureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DogProfiles_DogId] ON [DogProfiles] ([DogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Features_Name] ON [Features] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Kennels_Name] ON [Kennels] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD CONSTRAINT [FK_Dogs_Breeds_BreedId] FOREIGN KEY ([BreedId]) REFERENCES [Breeds] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    ALTER TABLE [Dogs] ADD CONSTRAINT [FK_Dogs_Kennels_KennelId] FOREIGN KEY ([KennelId]) REFERENCES [Kennels] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105205652_ExpandDogModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105205652_ExpandDogModel', N'9.0.1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261105212655_AddReferenceSeedData'
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
    WHERE [MigrationId] = N'20261105212655_AddReferenceSeedData'
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
    WHERE [MigrationId] = N'20261105212655_AddReferenceSeedData'
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
    WHERE [MigrationId] = N'20261105212655_AddReferenceSeedData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261105212655_AddReferenceSeedData', N'9.0.1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    DROP INDEX [IX_Dogs_BreedId] ON [Dogs];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    DROP INDEX [IX_Dogs_IsDeleted] ON [Dogs];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    DROP INDEX [IX_Dogs_KennelId] ON [Dogs];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    DROP INDEX [IX_Dogs_Status] ON [Dogs];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    ALTER TABLE [DogProfiles] DROP CONSTRAINT [PK_DogProfiles];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    DROP INDEX [IX_DogProfiles_DogId] ON [DogProfiles];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DogProfiles]') AND [c].[name] = N'Id');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [DogProfiles] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [DogProfiles] DROP COLUMN [Id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    ALTER TABLE [DogProfiles] ADD CONSTRAINT [PK_DogProfiles] PRIMARY KEY ([DogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BreedId', N'Color', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'KennelId', N'Name', N'Status', N'TailLength', N'UpdatedAt', N'UpdatedBy', N'Weight') AND [object_id] = OBJECT_ID(N'[Dogs]'))
        SET IDENTITY_INSERT [Dogs] ON;
    EXEC(N'INSERT INTO [Dogs] ([Id], [BreedId], [Color], [CreatedAt], [CreatedBy], [DeletedAt], [KennelId], [Name], [Status], [TailLength], [UpdatedAt], [UpdatedBy], [Weight])
    VALUES (''d0000000-0000-0000-0000-000000000001'', ''11111111-1111-1111-1111-111111111111'', N''golden'', ''2026-01-01T00:00:00.0000000Z'', N''seed'', NULL, ''aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'', N''Rex'', N''Available'', 35, NULL, NULL, 29.5),
    (''d0000000-0000-0000-0000-000000000002'', ''22222222-2222-2222-2222-222222222222'', N''black & tan'', ''2026-01-01T00:00:00.0000000Z'', N''seed'', NULL, ''bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'', N''Bella'', N''Available'', 40, NULL, NULL, 27.0),
    (''d0000000-0000-0000-0000-000000000003'', ''33333333-3333-3333-3333-333333333333'', N''brown & white'', ''2026-01-01T00:00:00.0000000Z'', N''seed'', NULL, ''aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'', N''Max'', N''Reserved'', 25, NULL, NULL, 12.3),
    (''d0000000-0000-0000-0000-000000000004'', ''44444444-4444-4444-4444-444444444444'', N''grey'', ''2026-01-01T00:00:00.0000000Z'', N''seed'', NULL, ''cccccccc-cccc-cccc-cccc-cccccccccccc'', N''Luna'', N''UnderTreatment'', 20, NULL, NULL, 9.8),
    (''d0000000-0000-0000-0000-000000000005'', ''11111111-1111-1111-1111-111111111111'', N''black'', ''2026-01-01T00:00:00.0000000Z'', N''seed'', NULL, NULL, N''Charlie'', N''Adopted'', 33, NULL, NULL, 31.2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BreedId', N'Color', N'CreatedAt', N'CreatedBy', N'DeletedAt', N'KennelId', N'Name', N'Status', N'TailLength', N'UpdatedAt', N'UpdatedBy', N'Weight') AND [object_id] = OBJECT_ID(N'[Dogs]'))
        SET IDENTITY_INSERT [Dogs] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DogId', N'FeatureId', N'AssignedAt', N'Notes') AND [object_id] = OBJECT_ID(N'[DogFeatures]'))
        SET IDENTITY_INSERT [DogFeatures] ON;
    EXEC(N'INSERT INTO [DogFeatures] ([DogId], [FeatureId], [AssignedAt], [Notes])
    VALUES (''d0000000-0000-0000-0000-000000000001'', ''dddddddd-dddd-dddd-dddd-dddddddddddd'', ''2026-01-01T00:00:00.0000000Z'', N''Greets everyone''),
    (''d0000000-0000-0000-0000-000000000001'', ''eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'', ''2026-01-01T00:00:00.0000000Z'', N''Basic commands''),
    (''d0000000-0000-0000-0000-000000000002'', ''12345678-1234-1234-1234-123456789012'', ''2026-01-01T00:00:00.0000000Z'', N''Needs long walks''),
    (''d0000000-0000-0000-0000-000000000002'', ''eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'', ''2026-01-01T00:00:00.0000000Z'', NULL),
    (''d0000000-0000-0000-0000-000000000003'', ''dddddddd-dddd-dddd-dddd-dddddddddddd'', ''2026-01-01T00:00:00.0000000Z'', NULL),
    (''d0000000-0000-0000-0000-000000000003'', ''ffffffff-ffff-ffff-ffff-ffffffffffff'', ''2026-01-01T00:00:00.0000000Z'', N''Tested with kids''),
    (''d0000000-0000-0000-0000-000000000004'', ''dddddddd-dddd-dddd-dddd-dddddddddddd'', ''2026-01-01T00:00:00.0000000Z'', NULL),
    (''d0000000-0000-0000-0000-000000000005'', ''ffffffff-ffff-ffff-ffff-ffffffffffff'', ''2026-01-01T00:00:00.0000000Z'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DogId', N'FeatureId', N'AssignedAt', N'Notes') AND [object_id] = OBJECT_ID(N'[DogFeatures]'))
        SET IDENTITY_INSERT [DogFeatures] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DogId', N'BirthDate', N'Description', N'Gender', N'Temperament') AND [object_id] = OBJECT_ID(N'[DogProfiles]'))
        SET IDENTITY_INSERT [DogProfiles] ON;
    EXEC(N'INSERT INTO [DogProfiles] ([DogId], [BirthDate], [Description], [Gender], [Temperament])
    VALUES (''d0000000-0000-0000-0000-000000000001'', ''2022-03-14T00:00:00.0000000'', N''Energetic retriever, loves water.'', N''Male'', N''Playful, friendly''),
    (''d0000000-0000-0000-0000-000000000002'', ''2021-07-02T00:00:00.0000000'', N''Calm and attentive shepherd.'', N''Female'', N''Loyal, calm''),
    (''d0000000-0000-0000-0000-000000000003'', ''2023-01-20T00:00:00.0000000'', N''Curious beagle with a great nose.'', N''Male'', N''Curious, social''),
    (''d0000000-0000-0000-0000-000000000004'', ''2020-11-05T00:00:00.0000000'', N''Shy mixed-breed, recovering after surgery.'', N''Female'', N''Shy, gentle''),
    (''d0000000-0000-0000-0000-000000000005'', ''2019-05-30T00:00:00.0000000'', N''Adopted family dog.'', N''Male'', N''Calm, affectionate'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DogId', N'BirthDate', N'Description', N'Gender', N'Temperament') AND [object_id] = OBJECT_ID(N'[DogProfiles]'))
        SET IDENTITY_INSERT [DogProfiles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Dogs_BreedId_Status_Active] ON [Dogs] ([BreedId], [Status]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Dogs_KennelId_Active] ON [Dogs] ([KennelId]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Dogs_Name_Active] ON [Dogs] ([Name]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Dogs_Status_Active] ON [Dogs] ([Status]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    EXEC(N'ALTER TABLE [DogProfiles] ADD CONSTRAINT [CK_DogProfiles_Gender] CHECK ([Gender] IS NULL OR [Gender] IN (''Male'', ''Female'', ''Unknown''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112223536_CatalogFixes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112223536_CatalogFixes', N'9.0.1');
END;

COMMIT;
GO

