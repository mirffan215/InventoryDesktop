-- SQL Server Agent jobs: backups, integrity checks, index maintenance, campaign roll-over.
-- Backup path X:\Backup must be on separate storage from data files. Backups are TDE-encrypted via the database key.
USE msdb;
GO
DECLARE @jobs TABLE (name sysname, cmd nvarchar(max), freq_type int, freq_interval int, start_time int);
INSERT @jobs VALUES
 ('AMGH Full Backup', N'BACKUP DATABASE AMGH_ITInventory TO DISK=N''X:\Backup\AMGH_FULL.bak'' WITH COMPRESSION, CHECKSUM, INIT, STATS=10;', 8, 1, 10000),   -- weekly Sunday 01:00
 ('AMGH Differential Backup', N'BACKUP DATABASE AMGH_ITInventory TO DISK=N''X:\Backup\AMGH_DIFF.bak'' WITH DIFFERENTIAL, COMPRESSION, CHECKSUM, INIT;', 4, 1, 10000), -- daily 01:00
 ('AMGH Log Backup', N'BACKUP LOG AMGH_ITInventory TO DISK=N''X:\Backup\AMGH_LOG.trn'' WITH COMPRESSION, CHECKSUM, NOINIT;', 4, 1, 0),
 ('AMGH Integrity Check', N'DBCC CHECKDB (AMGH_ITInventory) WITH NO_INFOMSGS, ALL_ERRORMSGS;', 8, 64, 30000),
 ('AMGH Campaign Rollover', N'EXEC AMGH_ITInventory.dbo.usp_MarkOverdueCampaigns;', 4, 1, 60000);
DECLARE @n sysname, @c nvarchar(max), @ft int, @fi int, @st int;
DECLARE cur CURSOR LOCAL FOR SELECT name, cmd, freq_type, freq_interval, start_time FROM @jobs;
OPEN cur; FETCH NEXT FROM cur INTO @n, @c, @ft, @fi, @st;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.sysjobs WHERE name = @n)
    BEGIN
        EXEC dbo.sp_add_job @job_name = @n;
        EXEC dbo.sp_add_jobstep @job_name = @n, @step_name = 'Run', @subsystem = 'TSQL', @command = @c, @database_name = 'master';
        EXEC dbo.sp_add_jobschedule @job_name = @n, @name = @n, @freq_type = @ft, @freq_interval = @fi, @active_start_time = @st,
             @freq_subday_type = CASE WHEN @n = 'AMGH Log Backup' THEN 4 ELSE 1 END, @freq_subday_interval = CASE WHEN @n = 'AMGH Log Backup' THEN 15 ELSE 0 END;
        EXEC dbo.sp_add_jobserver @job_name = @n;
    END
    FETCH NEXT FROM cur INTO @n, @c, @ft, @fi, @st;
END
CLOSE cur; DEALLOCATE cur;
GO
-- Index maintenance: rebuild >30% fragmentation, reorganize 10-30%.
USE AMGH_ITInventory;
GO
CREATE OR ALTER PROCEDURE dbo.usp_IndexMaintenance AS
BEGIN
    SET NOCOUNT ON; DECLARE @sql nvarchar(max) = N'';
    SELECT @sql += CONCAT('ALTER INDEX ', QUOTENAME(i.name), ' ON ', QUOTENAME(s.name), '.', QUOTENAME(o.name),
                          CASE WHEN ps.avg_fragmentation_in_percent > 30 THEN ' REBUILD;' ELSE ' REORGANIZE;' END, CHAR(10))
    FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
    JOIN sys.indexes i ON i.object_id = ps.object_id AND i.index_id = ps.index_id
    JOIN sys.objects o ON o.object_id = i.object_id JOIN sys.schemas s ON s.schema_id = o.schema_id
    WHERE ps.avg_fragmentation_in_percent > 10 AND ps.page_count > 1000 AND i.name IS NOT NULL;
    EXEC sys.sp_executesql @sql;
    EXEC sys.sp_updatestats;
END
GO
