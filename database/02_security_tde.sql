-- AMGH IT Inventory: encryption & database security. Run as sysadmin. Review passwords/paths before use.
USE master;
IF NOT EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##')
    CREATE MASTER KEY ENCRYPTION BY PASSWORD = N'<<REPLACE-WITH-STRONG-SECRET>>';
IF NOT EXISTS (SELECT 1 FROM sys.certificates WHERE name = 'AMGH_TDE_Cert')
    CREATE CERTIFICATE AMGH_TDE_Cert WITH SUBJECT = 'AMGH TDE Certificate';
GO
-- BACK UP THE CERTIFICATE AND PRIVATE KEY IMMEDIATELY; without them backups cannot be restored (see DR runbook).
-- BACKUP CERTIFICATE AMGH_TDE_Cert TO FILE = 'X:\Keys\AMGH_TDE_Cert.cer'
--   WITH PRIVATE KEY (FILE = 'X:\Keys\AMGH_TDE_Cert.pvk', ENCRYPTION BY PASSWORD = N'<<REPLACE>>');
USE AMGH_ITInventory;
IF NOT EXISTS (SELECT 1 FROM sys.dm_database_encryption_keys WHERE database_id = DB_ID())
BEGIN
    CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_256 ENCRYPTION BY SERVER CERTIFICATE AMGH_TDE_Cert;
    ALTER DATABASE AMGH_ITInventory SET ENCRYPTION ON;
END
GO
-- Roles (least privilege). The application connects as a contained/service identity that is a member of amgh_app only.
IF DATABASE_PRINCIPAL_ID('amgh_app') IS NULL CREATE ROLE amgh_app;
IF DATABASE_PRINCIPAL_ID('amgh_readonly') IS NULL CREATE ROLE amgh_readonly;
IF DATABASE_PRINCIPAL_ID('amgh_auditor') IS NULL CREATE ROLE amgh_auditor;
GRANT SELECT, INSERT, UPDATE ON SCHEMA::dbo TO amgh_app;
DENY DELETE ON SCHEMA::dbo TO amgh_app;                 -- soft delete only
DENY UPDATE, DELETE ON dbo.AuditLogs TO amgh_app;        -- audit trail is append-only
GRANT SELECT, INSERT ON dbo.AuditLogs TO amgh_app;
GRANT EXECUTE ON SCHEMA::dbo TO amgh_app;
GRANT SELECT ON SCHEMA::dbo TO amgh_readonly;
GRANT SELECT ON dbo.AuditLogs TO amgh_auditor;
GO
-- Server-level audit of schema/permission changes (ISO 27001 A.8.15/A.8.16).
USE master;
IF NOT EXISTS (SELECT 1 FROM sys.server_audits WHERE name = 'AMGH_Audit')
BEGIN
    CREATE SERVER AUDIT AMGH_Audit TO FILE (FILEPATH = N'X:\SQLAudit\', MAXSIZE = 100 MB, MAX_ROLLOVER_FILES = 50);
    ALTER SERVER AUDIT AMGH_Audit WITH (STATE = ON);
END
USE AMGH_ITInventory;
IF NOT EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = 'AMGH_DbAudit')
BEGIN
    CREATE DATABASE AUDIT SPECIFICATION AMGH_DbAudit FOR SERVER AUDIT AMGH_Audit
        ADD (SCHEMA_OBJECT_CHANGE_GROUP), ADD (DATABASE_PERMISSION_CHANGE_GROUP), ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP)
        WITH (STATE = ON);
END
GO
