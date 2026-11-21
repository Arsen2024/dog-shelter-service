/* =====================================================================
   AdoptionsDb — тестові дані. ІДЕМПОТЕНТНО: повторний запуск не створює дублікатів.
   Id собак збігаються з seed-даними DogsService (SeedIds.cs).
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE AdoptionsDb;
GO

-- Локальна копія собак (Name/Status — копія для читання, власник DogsService)
MERGE dbo.Dogs AS t
USING (VALUES
  ('d0000000-0000-0000-0000-000000000001', N'Rex',     'Available'),
  ('d0000000-0000-0000-0000-000000000002', N'Bella',   'Available'),
  ('d0000000-0000-0000-0000-000000000003', N'Max',     'Reserved'),
  ('d0000000-0000-0000-0000-000000000004', N'Luna',    'UnderTreatment'),
  ('d0000000-0000-0000-0000-000000000005', N'Charlie', 'Adopted')
) AS s (Id, Name, Status)
ON t.Id = TRY_CAST(s.Id AS UNIQUEIDENTIFIER)
WHEN NOT MATCHED THEN INSERT (Id, Name, Status) VALUES (TRY_CAST(s.Id AS UNIQUEIDENTIFIER), s.Name, s.Status);
GO

-- Довідник вимог
INSERT dbo.Requirements (Id, Code, Title, Description, IsMandatory)
SELECT v.Id, v.Code, v.Title, v.Description, v.IsMandatory
FROM (VALUES
  (CAST('a0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER), 'ID_DOCUMENT',   N'Документ, що посвідчує особу', N'Паспорт або ID-картка', 1),
  (CAST('a0000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER), 'HOME_VISIT',    N'Візит до оселі',              N'Перевірка умов утримання', 1),
  (CAST('a0000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER), 'VET_CONTACT',   N'Контакт ветеринара',          N'Ветеринар, який спостерігатиме собаку', 0),
  (CAST('a0000000-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER), 'FENCED_YARD',   N'Огороджене подвір''я',        N'Для великих порід', 0)
) AS v (Id, Code, Title, Description, IsMandatory)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Requirements r WHERE r.Code = v.Code);
GO

-- Заявники
INSERT dbo.Adopters (Id, FullName, Email, Phone, Address, CreatedBy)
SELECT v.Id, v.FullName, v.Email, v.Phone, v.Address, 'seed'
FROM (VALUES
  (CAST('b0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER), N'Олена Коваленко', 'olena@example.com',  '+380501112233', N'м. Чернівці, вул. Головна 1'),
  (CAST('b0000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER), N'Андрій Мельник',  'andrii@example.com', '+380672223344', N'м. Чернівці, вул. Руська 10'),
  (CAST('b0000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER), N'Ірина Шевчук',    'iryna@example.com',  NULL,            NULL)
) AS v (Id, FullName, Email, Phone, Address)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Adopters a WHERE a.Email = v.Email AND a.IsDeleted = 0);
GO

-- Заявка 1: Олена -> Rex, статус UnderReview
DECLARE @A1 UNIQUEIDENTIFIER = 'b0000000-0000-0000-0000-000000000001';
IF NOT EXISTS (SELECT 1 FROM dbo.AdoptionApplications WHERE AdopterId = @A1)
BEGIN
    EXEC dbo.usp_CreateApplication @AdopterId = @A1, @DogId = 'd0000000-0000-0000-0000-000000000001', @CreatedBy = 'seed';
    DECLARE @App1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.AdoptionApplications WHERE AdopterId = @A1);
    EXEC dbo.usp_ChangeApplicationStatus @ApplicationId = @App1, @NewStatus = 'UnderReview', @ChangedBy = 'seed', @Comment = N'Документи отримано';
END
GO

-- Заявка 2: Андрій -> Bella (+ Rex), усі обов'язкові вимоги виконано, статус Approved
DECLARE @A2 UNIQUEIDENTIFIER = 'b0000000-0000-0000-0000-000000000002';
IF NOT EXISTS (SELECT 1 FROM dbo.AdoptionApplications WHERE AdopterId = @A2)
BEGIN
    EXEC dbo.usp_CreateApplication @AdopterId = @A2, @DogId = 'd0000000-0000-0000-0000-000000000002', @CreatedBy = 'seed';
    DECLARE @App2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.AdoptionApplications WHERE AdopterId = @A2);
    EXEC dbo.usp_AddDogToApplication @ApplicationId = @App2, @DogId = 'd0000000-0000-0000-0000-000000000001', @Priority = 2;
    EXEC dbo.usp_ChangeApplicationStatus @ApplicationId = @App2, @NewStatus = 'UnderReview', @ChangedBy = 'seed';

    UPDATE ar SET IsFulfilled = 1, CheckedAt = SYSUTCDATETIME(), CheckedBy = 'seed'
    FROM dbo.ApplicationRequirements ar
    JOIN dbo.Requirements r ON r.Id = ar.RequirementId
    WHERE ar.ApplicationId = @App2 AND r.IsMandatory = 1;

    EXEC dbo.usp_ChangeApplicationStatus @ApplicationId = @App2, @NewStatus = 'Approved', @ChangedBy = 'seed', @Comment = N'Все гаразд';
END
GO
