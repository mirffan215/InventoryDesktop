# Administrator guide
- **Roles**: Admin (all), ITManager, Technician (manage assets, run discovery), Auditor (verify, reports), Custodian (verify), Approver (approve campaigns).
- **Audit trail**: every create/update/delete is written to `AuditLogs` (user, IP, old/new values). Deletes are soft. `amgh_app` cannot update/delete audit rows.
- **Retention**: `usp_PurgeOldAuditLogs` (default 7 years, minimum 1 year) - run under a maintenance login per policy.
- **Secrets**: JWT key, Graph secret and DB passwords belong in a secret store / environment variables, never in git.
- **Discovery**: scans are limited to /16 or smaller. Obtain network-team approval; scanning is active probing.
- **Backups/DR**: see DR-RUNBOOK.md. Test restores quarterly.
## ISO 27001 mapping (supporting evidence, not certification)
A.5.9 inventory of assets (register, owner/custodian fields) · A.5.12 classification (`SecurityClassification`) · A.8.15 logging (AuditLogs, SQL audit) · A.8.13 backup (Agent jobs) · A.8.24 cryptography (TDE, TLS, hashed licence keys).
