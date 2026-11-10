[Setup]
AppId={{B0B5D2A7-4A30-4BB2-9D89-CA4F85F5B3B8}
AppName=AZUL NEX
AppVersion=0.1.0-alpha
AppPublisher=Azuliciado
DefaultDirName={autopf}\AZUL NEX
DefaultGroupName=AZUL NEX
OutputDir=..\artifacts\installer
OutputBaseFilename=AZUL-NEX-Setup
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\AZUL-NEX.exe

[Files]
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\AZUL NEX"; Filename: "{app}\AZUL-NEX.exe"
Name: "{autodesktop}\AZUL NEX"; Filename: "{app}\AZUL-NEX.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"