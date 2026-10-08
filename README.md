# AMGH IT Asset Inventory Management System

Hospital IT asset management: asset register, **asset verification campaigns** (web, WPF desktop, Android), QR/barcode labels,
network discovery, reporting, and Microsoft 365 (Entra ID / Graph / Intune / Teams) integration. Built on .NET 8 and SQL Server 2022.

## Implementation status (read this first)

| Area | State |
|---|---|
| Domain, Application, Persistence (EF Core, audit trail, soft delete, migration) | **Built, compiled, tested** |
| REST API (JWT, versioning, Swagger, health checks, Serilog, OpenTelemetry): auth, assets, verification + offline sync, discovery | **Built, compiled** |
| Web MVC (dashboard, assets, verification campaigns, discovery, Entra ID sign-in) | **Built, compiled** (not browser-tested) |
| QR/barcode (QRCoder), reports (QuestPDF, ClosedXML), notifications + Intune/Entra sync (Graph) | **Built, compiled** (Graph calls not run against a tenant) |
| Shared API client + offline sync engine | **Built, compiled, unit-tested** |
| SQL package (TDE, roles, audit, views, functions, procs, Agent jobs, DR runbook) | **Written, NOT executed** (no SQL Server instance was available) |
| Docker / compose, GitHub Actions, Azure Pipelines, IIS script | **Written, NOT executed** |
| WPF desktop app | **Source written, NOT compiled** (needs Windows) |
| .NET MAUI Android app | **Source written, NOT compiled** (needs the `maui-android` workload) |
| Not yet implemented | Procurement, budget, warranty/maintenance/licence screens + API, approval workflow UI, evidence upload endpoint, certificates endpoint, push notifications, Power BI embedding, user/permission APIs, bulk label printing UI, import wizard, integration/performance/UAT suites |

The domain model already contains the procurement, budget, maintenance, licence and approval entities (and the migration creates their tables);
the services/controllers/screens for them remain to be built.

## Layout
```
src/AMGH.ITInventory.Domain          entities, business rules
src/AMGH.ITInventory.Application     abstractions, VerificationService
src/AMGH.ITInventory.Persistence     EF Core DbContext, configs, migrations, seed
src/AMGH.ITInventory.Infrastructure  DI wiring, network discovery (ping/port/SNMP)
src/AMGH.ITInventory.Security        roles, policies, JWT
src/AMGH.ITInventory.QRCode          QR payloads/PNG
src/AMGH.ITInventory.Reporting       PDF/Excel/certificate
src/AMGH.ITInventory.Notifications   Graph mail, Teams, Entra/Intune import
src/AMGH.ITInventory.Shared          DTOs, enums, API client, SyncEngine
src/AMGH.ITInventory.API / .Web      hosts
src/AMGH.ITInventory.Desktop         WPF (Windows)      src/AMGH.ITInventory.Mobile   MAUI (Android)
database/                            SQL scripts        deploy/ docs/ Dockerfile docker-compose.yml
```

## Build and test
```
dotnet build AMGH.ITInventory.Server.slnf      # everything except WPF/MAUI (works on Linux/macOS/Windows)
dotnet test  AMGH.ITInventory.Server.slnf
```
Open `AMGH.ITInventory.sln` in Visual Studio 2022 (with .NET desktop + .NET MAUI workloads) for the Desktop and Mobile projects.

## Run locally
API: set `ConnectionStrings:Default`, run in Development (auto-migrates + seeds, dev user `admin` from `appsettings.Development.json`; change that password).
Production never exposes the dev token endpoint; authenticate with Entra ID.
Docs: [Deployment](docs/DEPLOYMENT.md) · [Admin guide](docs/ADMIN-GUIDE.md) · [User guide](docs/USER-GUIDE.md) · [DR runbook](docs/DR-RUNBOOK.md)
