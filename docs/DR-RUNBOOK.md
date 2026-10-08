# Database Disaster Recovery Runbook

Targets (adjust to AMGH policy): **RPO 15 min** (log backups every 15 min), **RTO 4 h**.

## Prerequisites (verify quarterly)
- TDE certificate **and private key** are backed up off-server and the password is escrowed. Without them, backups are unrecoverable.
- Backup files are copied off-site / to immutable storage after each job.
- A restore test is performed to a scratch server at least quarterly; record the result for ISO 27001 A.8.13.

## Restore procedure
1. Provision SQL Server 2022 and restore the TDE certificate into `master` (`CREATE CERTIFICATE ... FROM FILE ... WITH PRIVATE KEY`).
2. `RESTORE DATABASE AMGH_ITInventory FROM DISK='...AMGH_FULL.bak' WITH NORECOVERY, REPLACE;`
3. `RESTORE DATABASE ... FROM DISK='...AMGH_DIFF.bak' WITH NORECOVERY;`
4. Restore each log backup in order `WITH NORECOVERY`; the last `WITH RECOVERY` (use `STOPAT` for point-in-time).
5. Run `database/02_security_tde.sql` role section; re-map the application login to `amgh_app`.
6. Start API/Web; check `/health`; confirm the latest `AuditLogs.TimestampUtc` matches the expected recovery point.
7. Mobile/desktop clients keep offline queues and re-sync automatically (server de-duplicates by ClientId).
