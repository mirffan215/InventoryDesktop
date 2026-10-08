; Inno Setup script. Expects the self-contained publish output in ..\artifacts\desktop
#define AppVersion GetEnv("APP_VERSION")
#if AppVersion == ""
  #define AppVersion "1.0.0"
#endif
[Setup]
AppName=AMGH IT Inventory
AppVersion={#AppVersion}
AppPublisher=AMGH IT
DefaultDirName={autopf}\AMGH IT Inventory
DefaultGroupName=AMGH IT Inventory
OutputDir=..\artifacts\installer
OutputBaseFilename=AMGH-ITInventory-Desktop-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
[Files]
Source: "..\artifacts\desktop\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
[Icons]
Name: "{group}\AMGH IT Inventory"; Filename: "{app}\AMGH.ITInventory.Desktop.exe"
Name: "{commondesktop}\AMGH IT Inventory"; Filename: "{app}\AMGH.ITInventory.Desktop.exe"
[Run]
Filename: "{app}\AMGH.ITInventory.Desktop.exe"; Description: "Launch"; Flags: nowait postinstall skipifsilent
