#Requires -RunAsAdministrator
param(
  [Parameter(Mandatory)] [string] $PackagePath,        # folder from `dotnet publish`
  [string] $SiteName = 'AMGH-ITInventory-Web',
  [string] $Root = 'C:\inetpub\amgh-itinventory',
  [int] $HttpsPort = 443,
  [Parameter(Mandatory)] [string] $CertThumbprint
)
$ErrorActionPreference = 'Stop'
Import-Module WebAdministration
# Prereq: ASP.NET Core 8 Hosting Bundle installed.
New-Item -ItemType Directory -Force $Root | Out-Null
if (Test-Path "$Root\app_offline.htm") { Remove-Item "$Root\app_offline.htm" }
Copy-Item "$PackagePath\*" $Root -Recurse -Force
if (-not (Test-Path "IIS:\AppPools\$SiteName")) { New-WebAppPool $SiteName | Out-Null }
Set-ItemProperty "IIS:\AppPools\$SiteName" managedRuntimeVersion ''
if (-not (Get-Website $SiteName -ErrorAction SilentlyContinue)) {
  New-Website -Name $SiteName -PhysicalPath $Root -ApplicationPool $SiteName -Port $HttpsPort -Ssl | Out-Null
  (Get-WebBinding -Name $SiteName).AddSslCertificate($CertThumbprint, 'My')
}
Restart-WebAppPool $SiteName
Write-Host "Deployed. Verify: https://localhost:$HttpsPort/health"
