/* =====================================================================
   AdoptionsDb — запити перевірки: CRUD, пошук, пагінація, транзакційні сценарії
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
GO
USE AdoptionsDb;
GO

-- ---------- CRUD (прямі SQL, як у Dapper) ----------
-- Read: пошук заявників за частиною імені
SELECT Id, FullName, Email FROM dbo.Adopters WHERE IsDeleted = 0 AND FullName LIKE N'%Олена%';

-- Update з оптимістичною конкурентністю (RowVersion)
DECLARE @id UNIQUEIDENTIFIER = 'b0000000-0000-0000-0000-000000000003';
DECLARE @rv VARBINARY(8) = (SELECT RowVersion FROM dbo.Adopters WHERE Id = @id);
UPDATE dbo.Adopters SET Phone = '+380931112233', UpdatedAt = SYSUTCDATETIME(), UpdatedBy = 'demo'
WHERE Id = @id AND RowVersion = @rv;
SELECT @@ROWCOUNT AS RowsUpdated;      -- 0 => хтось змінив запис раніше (конфлікт)

-- Delete (soft)
-- UPDATE dbo.Adopters SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME() WHERE Id = @id;

-- ---------- Пагінація: "черга розгляду" (використовує IX_Applications_Status_SubmittedAt) ----------
DECLARE @PageNumber INT = 1, @PageSize INT = 10;
SELECT a.Id, a.Status, a.SubmittedAt, ad.FullName
FROM dbo.AdoptionApplications a
JOIN dbo.Adopters ad ON ad.Id = a.AdopterId
WHERE a.IsDeleted = 0 AND a.Status = 'UnderReview'
ORDER BY a.SubmittedAt, a.Id
OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

-- ---------- Деталі заявки ----------
DECLARE @App UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.AdoptionApplications WHERE Status = 'Approved');
EXEC dbo.usp_GetApplicationDetails @ApplicationId = @App;

-- ---------- Транзакційні сценарії ----------
-- 1) Недопустимий перехід має відмовити керованою помилкою (50004)
BEGIN TRY
    DECLARE @NewApp TABLE (ApplicationId UNIQUEIDENTIFIER);
    INSERT @NewApp EXEC dbo.usp_CreateApplication
         @AdopterId = 'b0000000-0000-0000-0000-000000000003',
         @DogId = 'd0000000-0000-0000-0000-000000000002', @CreatedBy = 'demo';
    DECLARE @Bad UNIQUEIDENTIFIER = (SELECT TOP 1 ApplicationId FROM @NewApp);
    -- Submitted -> Approved пропускає UnderReview
    EXEC dbo.usp_ChangeApplicationStatus @ApplicationId = @Bad, @NewStatus = 'Approved';
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrNo, ERROR_MESSAGE() AS ErrMsg;   -- 50004 Invalid transition
END CATCH;

-- 2) Підписання договору для схваленої заявки
-- EXEC dbo.usp_SignAgreement @ApplicationId = @App, @DogId = 'd0000000-0000-0000-0000-000000000002',
--      @AgreementNumber = 'AGR-2026-0001', @SignedBy = N'Андрій Мельник';

-- 3) Спроба скасувати вже завершену заявку -> 50004 (після кроку 2)

-- ---------- Перевірка індексів і обмежень ----------
SELECT t.name AS TableName, i.name AS IndexName, i.is_unique, i.filter_definition
FROM sys.indexes i JOIN sys.tables t ON t.object_id = i.object_id
WHERE i.name IS NOT NULL AND t.is_ms_shipped = 0 ORDER BY t.name, i.name;

SELECT OBJECT_NAME(parent_object_id) AS TableName, name, definition FROM sys.check_constraints ORDER BY 1, 2;
SELECT OBJECT_NAME(parent_object_id) AS TableName, name, OBJECT_NAME(referenced_object_id) AS RefTable FROM sys.foreign_keys ORDER BY 1, 2;
