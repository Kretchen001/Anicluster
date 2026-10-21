; Inno Setup 6.x – Installer für Anicluster
; Voraussetzung: Vorher mit "dotnet publish" (bzw. dem Visual-Studio-Profil) nach Anicluster\bin\Publish veröffentlichen.
; Die Versionsnummer stammt aus der version.txt, die beim Publish erzeugt wird (<Version> in Anicluster.csproj).

#define MyAppName "Anicluster"
#define MyAppPublisher "Kretchen001"
#define MyAppExeName "Anicluster.exe"
#define PublishDir "..\Anicluster\bin\Publish"

; Version aus der version.txt lesen, ohne diese Datei lässt sich der Installer nicht bauen
#define VersionHandle FileOpen(PublishDir + "\version.txt")
#define MyAppVersion Trim(FileRead(VersionHandle))
#expr FileClose(VersionHandle)

[Setup]
AppId={{4DACC09A-0C9C-40CE-A0B4-0A240F902802}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
VersionInfoVersion={#MyAppVersion}
VersionInfoProductName={#MyAppName}
VersionInfoDescription={#MyAppName} Setup

; Installation für alle Benutzer nach "Programme" (C:\Program Files\Anicluster), dafür sind Administratorrechte nötig.
; Die Anwendung selbst läuft später ohne Administratorrechte.
PrivilegesRequired=admin
; Anicluster ist 64-Bit, ohne diese Zeilen ginge die Installation nach "Programme (x86)" (ab Inno Setup 6.3).
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
AllowNoIcons=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

; Symbol des Installers (das Programmsymbol stammt aus der Anwendung selbst)
SetupIconFile=..\Anicluster\Resources\Icon\Anicluster.ico

; Moderner Assistent, Changelog vor der Installation anzeigen
WizardStyle=modern
WizardSizePercent=110
InfoBeforeFile={#SourcePath}\ReadMe.txt

; Laufende Anwendung beim Update schließen lassen und danach nicht neu starten
CloseApplications=yes
RestartApplications=no

; Ausgabe: setup_Anicluster_v_<Version>.exe neben dem Skript
OutputDir={#SourcePath}
OutputBaseFilename=setup_{#MyAppName}_v_{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes

; Das Setup ist passwortgeschützt. Für einen frei verteilbaren Installer beide Zeilen entfernen.
Password=Anicluster
Encryption=yes

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Das komplette Publish-Verzeichnis, außer Versionsdatei sowie Daten und Logs eines lokalen Testlaufs
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "version.txt,db\*,logs\*"; Flags: ignoreversion recursesubdirs createallsubdirs

[Dirs]
; Datenbank und Logs liegen neben der Anwendung. "Programme" ist für normale Benutzer schreibgeschützt, deshalb bekommen
; beide Ordner Schreibrechte für die Gruppe "Benutzer". Die Datenbank wird damit von allen Benutzern des Rechners geteilt.
; uninsneveruninstall: Die Daten bleiben beim Deinstallieren zunächst erhalten, siehe [Code].
Name: "{app}\db"; Permissions: users-modify; Flags: uninsneveruninstall
Name: "{app}\logs"; Permissions: users-modify; Flags: uninsneveruninstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
{ Beim Deinstallieren fragen, ob auch die Daten (Datenbank, Logs, Einstellungen) gelöscht werden sollen. Standard: behalten.
  Die Einstellungen liegen im AppData-Ordner des Kontos, unter dem die Deinstallation läuft, die anderer Benutzer bleiben bestehen. }
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if MsgBox('Sollen auch die Datenbank, die Log-Dateien und die Einstellungen dieses Benutzerkontos gelöscht werden?' + #13#10 +
              'Diese Daten gehen unwiderruflich verloren.', mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
    begin
      DelTree(ExpandConstant('{app}\db'), True, True, True);
      DelTree(ExpandConstant('{app}\logs'), True, True, True);
      DelTree(ExpandConstant('{userappdata}\{#MyAppName}'), True, True, True);
      RemoveDir(ExpandConstant('{app}'));
    end;
  end;
end;
