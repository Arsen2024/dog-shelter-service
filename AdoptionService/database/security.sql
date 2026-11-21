/* Права найменших привілеїв: окремий логін застосунку лише з доступом до AdoptionsDb.
   ЗМІНІТЬ ПАРОЛЬ перед використанням. */
USE master;
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'adoptions_app')
    CREATE LOGIN adoptions_app WITH PASSWORD = 'ChangeMe_Str0ng!Pass1', CHECK_POLICY = ON;
GO
USE AdoptionsDb;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'adoptions_app')
    CREATE USER adoptions_app FOR LOGIN adoptions_app;
GO
GRANT EXECUTE ON SCHEMA::dbo TO adoptions_app;
GRANT SELECT  ON SCHEMA::dbo TO adoptions_app;
GRANT INSERT, UPDATE ON dbo.Dogs TO adoptions_app;      -- синхронізація локальної копії з подій Каталогу
GRANT UPDATE ON dbo.ApplicationRequirements TO adoptions_app;
GRANT UPDATE ON dbo.Adopters TO adoptions_app;
GO
