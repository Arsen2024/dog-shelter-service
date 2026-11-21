/* =====================================================================
   AdoptionsService  |  Проєкт №1  |  SQL Server + ADO.NET/Dapper
   Схема БД AdoptionsDb. Скрипт ідемпотентний (можна запускати повторно).
   ВАЖЛИВО: запускати з sqlcmd -I (QUOTED_IDENTIFIER ON), інакше
   фільтровані індекси не створяться.
   Жодних FOREIGN KEY на інші бази (DogsDb, HealthDb) — лише логічні посилання.
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
IF DB_ID(N'AdoptionsDb') IS NULL CREATE DATABASE AdoptionsDb;
GO
USE AdoptionsDb;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- Dogs: ЛОКАЛЬНА КОПІЯ довідника з DogsService ----------
   Власник даних: DogsDb.Dogs. Id той самий, що в БД Каталогу (без IDENTITY).
   Name/Status оновлюються подіями від DogsService (eventual consistency). */
IF OBJECT_ID(N'dbo.Dogs', N'U') IS NULL
CREATE TABLE dbo.Dogs (
    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Dogs PRIMARY KEY,
    Name      NVARCHAR(100)    NOT NULL,
    Status    NVARCHAR(30)     NOT NULL CONSTRAINT DF_Dogs_Status DEFAULT 'Available'
              CONSTRAINT CK_Dogs_Status CHECK (Status IN ('Available','Reserved','Adopted','UnderTreatment')),
    SyncedAt  DATETIME2        NOT NULL CONSTRAINT DF_Dogs_SyncedAt DEFAULT SYSUTCDATETIME()
);
GO

/* ---------- Adopters ---------- */
IF OBJECT_ID(N'dbo.Adopters', N'U') IS NULL
CREATE TABLE dbo.Adopters (
    Id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Adopters PRIMARY KEY
               CONSTRAINT DF_Adopters_Id DEFAULT NEWSEQUENTIALID(),
    FullName   NVARCHAR(200) NOT NULL,
    Email      NVARCHAR(256) NOT NULL,
    Phone      NVARCHAR(30)  NULL,
    Address    NVARCHAR(400) NULL,
    CreatedAt  DATETIME2     NOT NULL CONSTRAINT DF_Adopters_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy  NVARCHAR(100) NULL,
    UpdatedAt  DATETIME2     NULL,
    UpdatedBy  NVARCHAR(100) NULL,
    IsDeleted  BIT           NOT NULL CONSTRAINT DF_Adopters_IsDeleted DEFAULT 0,
    RowVersion ROWVERSION    NOT NULL,
    CONSTRAINT CK_Adopters_Email CHECK (Email LIKE '%_@_%._%')
);
GO

/* ---------- Requirements (довідник вимог до власника) ---------- */
IF OBJECT_ID(N'dbo.Requirements', N'U') IS NULL
CREATE TABLE dbo.Requirements (
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Requirements PRIMARY KEY
                CONSTRAINT DF_Requirements_Id DEFAULT NEWSEQUENTIALID(),
    Code        NVARCHAR(50)   NOT NULL CONSTRAINT UQ_Requirements_Code UNIQUE,
    Title       NVARCHAR(200)  NOT NULL,
    Description NVARCHAR(1000) NULL,
    IsMandatory BIT            NOT NULL CONSTRAINT DF_Requirements_IsMandatory DEFAULT 1
);
GO

/* ---------- AdoptionApplications  (1:N Adopters -> Applications) ---------- */
IF OBJECT_ID(N'dbo.AdoptionApplications', N'U') IS NULL
CREATE TABLE dbo.AdoptionApplications (
    Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AdoptionApplications PRIMARY KEY
                 CONSTRAINT DF_Applications_Id DEFAULT NEWSEQUENTIALID(),
    AdopterId    UNIQUEIDENTIFIER NOT NULL
                 CONSTRAINT FK_Applications_Adopters REFERENCES dbo.Adopters(Id),   -- без CASCADE: історія заявників зберігається
    Status       NVARCHAR(30)  NOT NULL CONSTRAINT DF_Applications_Status DEFAULT 'Submitted'
                 CONSTRAINT CK_Applications_Status CHECK (Status IN
                   ('Submitted','UnderReview','Approved','Completed','Rejected','Cancelled')),
    SubmittedAt  DATETIME2     NOT NULL CONSTRAINT DF_Applications_SubmittedAt DEFAULT SYSUTCDATETIME(),
    DecisionNote NVARCHAR(1000) NULL,
    CreatedAt    DATETIME2     NOT NULL CONSTRAINT DF_Applications_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy    NVARCHAR(100) NULL,
    UpdatedAt    DATETIME2     NULL,
    UpdatedBy    NVARCHAR(100) NULL,
    IsDeleted    BIT           NOT NULL CONSTRAINT DF_Applications_IsDeleted DEFAULT 0,
    RowVersion   ROWVERSION    NOT NULL                                              -- оптимістична конкурентність
);
GO

/* ---------- ApplicationDogs  (1:N Application -> Dogs у заявці) ----------
   DogName — ЗНІМОК на момент подання заявки, не оновлюється. */
IF OBJECT_ID(N'dbo.ApplicationDogs', N'U') IS NULL
CREATE TABLE dbo.ApplicationDogs (
    ApplicationId UNIQUEIDENTIFIER NOT NULL
                  CONSTRAINT FK_ApplicationDogs_Applications REFERENCES dbo.AdoptionApplications(Id) ON DELETE CASCADE,
    DogId         UNIQUEIDENTIFIER NOT NULL
                  CONSTRAINT FK_ApplicationDogs_Dogs REFERENCES dbo.Dogs(Id),      -- FK на ЛОКАЛЬНУ копію, не на DogsDb
    DogName       NVARCHAR(100) NOT NULL,
    Priority      INT           NOT NULL CONSTRAINT DF_ApplicationDogs_Priority DEFAULT 1
                  CONSTRAINT CK_ApplicationDogs_Priority CHECK (Priority > 0),
    AddedAt       DATETIME2     NOT NULL CONSTRAINT DF_ApplicationDogs_AddedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ApplicationDogs PRIMARY KEY (ApplicationId, DogId)
);
GO

/* ---------- ApplicationStatusHistory (1:N) ---------- */
IF OBJECT_ID(N'dbo.ApplicationStatusHistory', N'U') IS NULL
CREATE TABLE dbo.ApplicationStatusHistory (
    Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationStatusHistory PRIMARY KEY, -- один писач, послідовна вставка
    ApplicationId UNIQUEIDENTIFIER NOT NULL
                  CONSTRAINT FK_StatusHistory_Applications REFERENCES dbo.AdoptionApplications(Id) ON DELETE CASCADE,
    OldStatus     NVARCHAR(30)  NULL,
    NewStatus     NVARCHAR(30)  NOT NULL
                  CONSTRAINT CK_StatusHistory_NewStatus CHECK (NewStatus IN
                   ('Submitted','UnderReview','Approved','Completed','Rejected','Cancelled')),
    ChangedAt     DATETIME2     NOT NULL CONSTRAINT DF_StatusHistory_ChangedAt DEFAULT SYSUTCDATETIME(),
    ChangedBy     NVARCHAR(100) NULL,
    Comment       NVARCHAR(500) NULL
);
GO

/* ---------- AdoptionAgreements  (1:1 Application <-> Agreement) ----------
   FK є одночасно PK залежної таблиці. */
IF OBJECT_ID(N'dbo.AdoptionAgreements', N'U') IS NULL
CREATE TABLE dbo.AdoptionAgreements (
    ApplicationId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AdoptionAgreements PRIMARY KEY
                    CONSTRAINT FK_Agreements_Applications REFERENCES dbo.AdoptionApplications(Id) ON DELETE CASCADE,
    DogId           UNIQUEIDENTIFIER NOT NULL
                    CONSTRAINT FK_Agreements_Dogs REFERENCES dbo.Dogs(Id),
    AgreementNumber NVARCHAR(50)  NOT NULL CONSTRAINT UQ_Agreements_Number UNIQUE,
    SignedAt        DATETIME2     NOT NULL CONSTRAINT DF_Agreements_SignedAt DEFAULT SYSUTCDATETIME(),
    SignedBy        NVARCHAR(200) NOT NULL,
    Terms           NVARCHAR(2000) NULL
);
GO

/* ---------- ApplicationRequirements  (M:N Application <-> Requirement, з власними полями) ---------- */
IF OBJECT_ID(N'dbo.ApplicationRequirements', N'U') IS NULL
CREATE TABLE dbo.ApplicationRequirements (
    ApplicationId UNIQUEIDENTIFIER NOT NULL
                  CONSTRAINT FK_AppReq_Applications REFERENCES dbo.AdoptionApplications(Id) ON DELETE CASCADE,
    RequirementId UNIQUEIDENTIFIER NOT NULL
                  CONSTRAINT FK_AppReq_Requirements REFERENCES dbo.Requirements(Id),
    IsFulfilled   BIT           NOT NULL CONSTRAINT DF_AppReq_IsFulfilled DEFAULT 0,
    CheckedAt     DATETIME2     NULL,
    CheckedBy     NVARCHAR(100) NULL,
    Notes         NVARCHAR(500) NULL,
    CONSTRAINT PK_ApplicationRequirements PRIMARY KEY (ApplicationId, RequirementId),
    CONSTRAINT CK_AppReq_CheckedAt CHECK (IsFulfilled = 0 OR CheckedAt IS NOT NULL)
);
GO

/* ======================= ІНДЕКСИ (кожен — під конкретний запит) ======================= */

-- Запит: вхід/реєстрація заявника за email; гарантує унікальність серед "живих"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Adopters_Email_Active' AND object_id = OBJECT_ID('dbo.Adopters'))
    CREATE UNIQUE INDEX UX_Adopters_Email_Active ON dbo.Adopters (Email) WHERE IsDeleted = 0;
GO
-- Запит: "заявки заявника з певним статусом" (покриваючий, лише живі)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Applications_AdopterId_Status' AND object_id = OBJECT_ID('dbo.AdoptionApplications'))
    CREATE INDEX IX_Applications_AdopterId_Status ON dbo.AdoptionApplications (AdopterId, Status)
        INCLUDE (SubmittedAt) WHERE IsDeleted = 0;
GO
-- Запит: "черга розгляду": заявки в статусі X, від найстаріших (пагінація)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Applications_Status_SubmittedAt' AND object_id = OBJECT_ID('dbo.AdoptionApplications'))
    CREATE INDEX IX_Applications_Status_SubmittedAt ON dbo.AdoptionApplications (Status, SubmittedAt) WHERE IsDeleted = 0;
GO
-- Запит: "у яких заявках фігурує собака Y"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApplicationDogs_DogId' AND object_id = OBJECT_ID('dbo.ApplicationDogs'))
    CREATE INDEX IX_ApplicationDogs_DogId ON dbo.ApplicationDogs (DogId);
GO
-- Запит: "історія статусів заявки за часом"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StatusHistory_ApplicationId_ChangedAt' AND object_id = OBJECT_ID('dbo.ApplicationStatusHistory'))
    CREATE INDEX IX_StatusHistory_ApplicationId_ChangedAt ON dbo.ApplicationStatusHistory (ApplicationId, ChangedAt);
GO
-- Запит: "невиконані вимоги по заявках" (фільтрований — лише IsFulfilled = 0)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AppReq_Unfulfilled' AND object_id = OBJECT_ID('dbo.ApplicationRequirements'))
    CREATE INDEX IX_AppReq_Unfulfilled ON dbo.ApplicationRequirements (RequirementId, ApplicationId) WHERE IsFulfilled = 0;
GO
-- Запит: "які собаки доступні для прилаштування" (локальна копія)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Dogs_Status' AND object_id = OBJECT_ID('dbo.Dogs'))
    CREATE INDEX IX_Dogs_Status ON dbo.Dogs (Status) INCLUDE (Name);
GO
