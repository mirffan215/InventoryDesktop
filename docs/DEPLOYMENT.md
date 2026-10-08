# Deployment guide
## Database
1. Create `AMGH_ITInventory` on SQL Server 2022. 2. Apply schema: `database/01_schema_ef.sql` (generated from the EF migration; regenerate with `dotnet ef migrations script -i`).
3. Review placeholders (passwords, `X:\` paths), then run `02_security_tde.sql`, `03_views_functions_procs.sql`, `04_agent_jobs_backup.sql`.
4. **Back up the TDE certificate and key before anything else.**
## Docker
`cp .env.example .env`, fill `SA_PASSWORD`, `JWT_SIGNING_KEY` (32+ chars), Entra IDs; `docker compose up -d --build`. API :8080, Web :8081 (put TLS termination in front).
Use a dedicated SQL login in `amgh_app` rather than `sa` for real deployments.
## IIS
Install the ASP.NET Core 8 Hosting Bundle, `dotnet publish src/AMGH.ITInventory.Web -c Release -o out`, then `deploy/iis/Deploy-AmghIIS.ps1 -PackagePath out -CertThumbprint <thumb>`.
## Entra ID
Register a web app (redirect `/signin-oidc`) and set `AzureAd:*`; create app roles Admin, ITManager, Technician, Auditor, Custodian, Approver and assign them. Enforce MFA via Conditional Access.
Graph app registration (for mail/users/Intune) needs application permissions `Mail.Send`, `User.Read.All`, `DeviceManagementManagedDevices.Read.All` with admin consent; set `Graph:*` via secret store.
## CI/CD
`.github/workflows/ci.yml` and `azure-pipelines.yml` build, test and publish.
