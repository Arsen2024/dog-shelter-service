/* =====================================================================
   AdoptionsDb — збережувані процедури (Проєкт №1)
   Запускати з sqlcmd -I.
   Коди помилок THROW:
     50001 Application not found          50002 Adopter not found
     50003 Dog not found (local copy)     50004 Invalid status transition
     50005 Mandatory requirements unmet   50006 Operation not allowed in current status
     50007 Dog is not in application      50008 Dog already in application
     50009 Use usp_SignAgreement          50010 Dog is not available
     50020 Adopter email already exists
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE AdoptionsDb;
GO

/* 1) CRUD (Create): новий заявник */
CREATE OR ALTER PROCEDURE dbo.usp_CreateAdopter
    @FullName  NVARCHAR(200),
    @Email     NVARCHAR(256),
    @Phone     NVARCHAR(30)  = NULL,
    @Address   NVARCHAR(400) = NULL,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.Adopters WHERE Email = @Email AND IsDeleted = 0)
        THROW 50020, 'Adopter with this email already exists', 1;

    DECLARE @Id UNIQUEIDENTIFIER = NEWID();

    INSERT dbo.Adopters (Id, FullName, Email, Phone, Address, CreatedBy)
    VALUES (@Id, @FullName, @Email, @Phone, @Address, @CreatedBy);

    SELECT @Id AS AdopterId;
END
GO

/* 2) Бізнес-операція: створення заявки з першою собакою (транзакція) */
CREATE OR ALTER PROCEDURE dbo.usp_CreateApplication
    @AdopterId UNIQUEIDENTIFIER,
    @DogId     UNIQUEIDENTIFIER,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM dbo.Adopters WHERE Id = @AdopterId AND IsDeleted = 0)
        THROW 50002, 'Adopter not found', 1;

    DECLARE @DogName NVARCHAR(100), @DogStatus NVARCHAR(30);
    SELECT @DogName = Name, @DogStatus = Status
    FROM dbo.Dogs WITH (UPDLOCK) WHERE Id = @DogId;

    IF @DogName IS NULL
        THROW 50003, 'Dog not found in local catalog copy', 1;
    IF @DogStatus <> 'Available'
        THROW 50010, 'Dog is not available for adoption', 1;

    DECLARE @AppId UNIQUEIDENTIFIER = NEWID();

    INSERT dbo.AdoptionApplications (Id, AdopterId, Status, CreatedBy)
    VALUES (@AppId, @AdopterId, 'Submitted', @CreatedBy);

    INSERT dbo.ApplicationDogs (ApplicationId, DogId, DogName)      -- DogName = знімок
    VALUES (@AppId, @DogId, @DogName);

    INSERT dbo.ApplicationStatusHistory (ApplicationId, OldStatus, NewStatus, ChangedBy, Comment)
    VALUES (@AppId, NULL, 'Submitted', @CreatedBy, N'Application created');

    INSERT dbo.ApplicationRequirements (ApplicationId, RequirementId)
    SELECT @AppId, Id FROM dbo.Requirements;

    COMMIT TRANSACTION;

    SELECT @AppId AS ApplicationId;
END
GO

/* 3) Додавання ще однієї собаки в заявку (лише поки заявка не схвалена) */
CREATE OR ALTER PROCEDURE dbo.usp_AddDogToApplication
    @ApplicationId UNIQUEIDENTIFIER,
    @DogId         UNIQUEIDENTIFIER,
    @Priority      INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Status NVARCHAR(30) =
        (SELECT Status FROM dbo.AdoptionApplications WITH (UPDLOCK, HOLDLOCK)
         WHERE Id = @ApplicationId AND IsDeleted = 0);

    IF @Status IS NULL
        THROW 50001, 'Application not found', 1;
    IF @Status NOT IN ('Submitted', 'UnderReview')
        THROW 50006, 'Dogs can be added only to Submitted or UnderReview applications', 1;

    IF EXISTS (SELECT 1 FROM dbo.ApplicationDogs WHERE ApplicationId = @ApplicationId AND DogId = @DogId)
        THROW 50008, 'Dog is already in this application', 1;

    DECLARE @DogName NVARCHAR(100), @DogStatus NVARCHAR(30);
    SELECT @DogName = Name, @DogStatus = Status FROM dbo.Dogs WHERE Id = @DogId;

    IF @DogName IS NULL
        THROW 50003, 'Dog not found in local catalog copy', 1;
    IF @DogStatus <> 'Available'
        THROW 50010, 'Dog is not available for adoption', 1;

    INSERT dbo.ApplicationDogs (ApplicationId, DogId, DogName, Priority)
    VALUES (@ApplicationId, @DogId, @DogName, @Priority);

    UPDATE dbo.AdoptionApplications SET UpdatedAt = SYSUTCDATETIME() WHERE Id = @ApplicationId;

    COMMIT TRANSACTION;
END
GO

/* 4) ГОЛОВНА: транзакційна зміна статусу з перевіркою допустимого переходу
      Submitted -> UnderReview -> Approved -> Completed (Completed лише через usp_SignAgreement)
      Rejected / Cancelled — з будь-якого статусу до Completed. */
CREATE OR ALTER PROCEDURE dbo.usp_ChangeApplicationStatus
    @ApplicationId UNIQUEIDENTIFIER,
    @NewStatus     NVARCHAR(30),
    @ChangedBy     NVARCHAR(100) = NULL,
    @Comment       NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;                                  -- будь-яка помилка відкочує транзакцію

    BEGIN TRANSACTION;

    DECLARE @OldStatus NVARCHAR(30) =
        (SELECT Status FROM dbo.AdoptionApplications WITH (UPDLOCK, HOLDLOCK)   -- блокуємо рядок до COMMIT
         WHERE Id = @ApplicationId AND IsDeleted = 0);

    IF @OldStatus IS NULL
        THROW 50001, 'Application not found', 1;

    IF @NewStatus = 'Completed'
        THROW 50009, 'Status Completed is set only by usp_SignAgreement', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM (VALUES ('Submitted',   'UnderReview'),
                     ('Submitted',   'Rejected'),
                     ('Submitted',   'Cancelled'),
                     ('UnderReview', 'Approved'),
                     ('UnderReview', 'Rejected'),
                     ('UnderReview', 'Cancelled'),
                     ('Approved',    'Cancelled'),
                     ('Approved',    'Rejected')) AS t (FromStatus, ToStatus)
        WHERE t.FromStatus = @OldStatus AND t.ToStatus = @NewStatus)
    BEGIN
        DECLARE @Msg NVARCHAR(200) = CONCAT(N'Invalid transition: ', @OldStatus, N' -> ', @NewStatus);
        THROW 50004, @Msg, 1;
    END

    IF @NewStatus = 'Approved' AND EXISTS (
        SELECT 1
        FROM dbo.ApplicationRequirements ar
        JOIN dbo.Requirements r ON r.Id = ar.RequirementId
        WHERE ar.ApplicationId = @ApplicationId AND r.IsMandatory = 1 AND ar.IsFulfilled = 0)
        THROW 50005, 'All mandatory requirements must be fulfilled before approval', 1;

    UPDATE dbo.AdoptionApplications
    SET Status = @NewStatus,
        DecisionNote = CASE WHEN @NewStatus IN ('Approved','Rejected','Cancelled') THEN @Comment ELSE DecisionNote END,
        UpdatedAt = SYSUTCDATETIME(),
        UpdatedBy = @ChangedBy
    WHERE Id = @ApplicationId;

    INSERT dbo.ApplicationStatusHistory (ApplicationId, OldStatus, NewStatus, ChangedBy, Comment)
    VALUES (@ApplicationId, @OldStatus, @NewStatus, @ChangedBy, @Comment);

    COMMIT TRANSACTION;
END
GO

/* 5) Підписання договору: створює AdoptionAgreement (1:1) і закриває заявку (Completed).
      Статус собаки в Каталозі (Adopted) змінює DogsService за подією — тут його НЕ чіпаємо (немає dual write). */
CREATE OR ALTER PROCEDURE dbo.usp_SignAgreement
    @ApplicationId   UNIQUEIDENTIFIER,
    @DogId           UNIQUEIDENTIFIER,
    @AgreementNumber NVARCHAR(50),
    @SignedBy        NVARCHAR(200),
    @Terms           NVARCHAR(2000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @OldStatus NVARCHAR(30) =
        (SELECT Status FROM dbo.AdoptionApplications WITH (UPDLOCK, HOLDLOCK)
         WHERE Id = @ApplicationId AND IsDeleted = 0);

    IF @OldStatus IS NULL
        THROW 50001, 'Application not found', 1;
    IF @OldStatus <> 'Approved'
        THROW 50006, 'Agreement can be signed only for an Approved application', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.ApplicationDogs WHERE ApplicationId = @ApplicationId AND DogId = @DogId)
        THROW 50007, 'Dog is not part of this application', 1;

    INSERT dbo.AdoptionAgreements (ApplicationId, DogId, AgreementNumber, SignedBy, Terms)
    VALUES (@ApplicationId, @DogId, @AgreementNumber, @SignedBy, @Terms);

    UPDATE dbo.AdoptionApplications
    SET Status = 'Completed', UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @SignedBy
    WHERE Id = @ApplicationId;

    INSERT dbo.ApplicationStatusHistory (ApplicationId, OldStatus, NewStatus, ChangedBy, Comment)
    VALUES (@ApplicationId, @OldStatus, 'Completed', @SignedBy, CONCAT(N'Agreement ', @AgreementNumber, N' signed'));

    COMMIT TRANSACTION;
END
GO

/* 6) Читання: деталі заявки кількома result set-ами (для Dapper QueryMultiple) */
CREATE OR ALTER PROCEDURE dbo.usp_GetApplicationDetails
    @ApplicationId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- 1) заявка + заявник + договір (1:1, може бути NULL)
    SELECT a.Id, a.Status, a.SubmittedAt, a.DecisionNote, a.RowVersion,
           ad.Id AS AdopterId, ad.FullName, ad.Email,
           ag.AgreementNumber, ag.SignedAt
    FROM dbo.AdoptionApplications a
    JOIN dbo.Adopters ad ON ad.Id = a.AdopterId
    LEFT JOIN dbo.AdoptionAgreements ag ON ag.ApplicationId = a.Id
    WHERE a.Id = @ApplicationId AND a.IsDeleted = 0;

    -- 2) собаки (знімок імені + поточний статус локальної копії)
    SELECT d.DogId, d.DogName AS DogNameAtSubmission, ld.Name AS CurrentName, ld.Status AS CurrentStatus, d.Priority
    FROM dbo.ApplicationDogs d
    JOIN dbo.Dogs ld ON ld.Id = d.DogId
    WHERE d.ApplicationId = @ApplicationId
    ORDER BY d.Priority;

    -- 3) вимоги (M:N)
    SELECT r.Code, r.Title, r.IsMandatory, ar.IsFulfilled, ar.CheckedAt, ar.CheckedBy
    FROM dbo.ApplicationRequirements ar
    JOIN dbo.Requirements r ON r.Id = ar.RequirementId
    WHERE ar.ApplicationId = @ApplicationId
    ORDER BY r.Code;

    -- 4) історія статусів
    SELECT OldStatus, NewStatus, ChangedAt, ChangedBy, Comment
    FROM dbo.ApplicationStatusHistory
    WHERE ApplicationId = @ApplicationId
    ORDER BY ChangedAt, Id;
END
GO
