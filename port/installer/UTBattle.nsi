; Windows installer for the C# port. Built on Linux with makensis (apt install nsis):
;   cd port/installer && makensis UTBattle.nsi
; Needs port/UTBattle/publish/win-x64/UTBattle.exe (see port/README.md). Installs per user, so no
; admin prompt, into %LOCALAPPDATA%\Programs\UTBattle with Start menu and desktop shortcuts.
Unicode true
!include "MUI2.nsh"

!define APP "Innertale Engine (C#)"
!define VER "1.4.0"
!define REG "Software\Microsoft\Windows\CurrentVersion\Uninstall\UTBattleCSharp"

Name "${APP}"
OutFile "UTBattle-CSharp-Setup.exe"
InstallDir "$LOCALAPPDATA\Programs\UTBattle"
InstallDirRegKey HKCU "${REG}" "InstallLocation"
RequestExecutionLevel user
SetCompressor /SOLID lzma
ManifestDPIAware true
; the installer's own version info (unsigned installers without any are flagged more often)
VIProductVersion "${VER}.0"
VIAddVersionKey "ProductName" "${APP}"
VIAddVersionKey "CompanyName" "Innertale Engine"
VIAddVersionKey "FileDescription" "${APP} setup"
VIAddVersionKey "FileVersion" "${VER}"
VIAddVersionKey "ProductVersion" "${VER}"
VIAddVersionKey "LegalCopyright" "Fan project; UNDERTALE and DELTARUNE by Toby Fox"

!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_RUN "$INSTDIR\UTBattle.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Play now"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Section "Install"
  SetOutPath "$INSTDIR"
  File "../UTBattle/publish/win-x64/UTBattle.exe"
  File "../UTBattle/publish/win-x64/raylib.dll"
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  CreateDirectory "$SMPROGRAMS\${APP}"
  CreateShortcut "$SMPROGRAMS\${APP}\${APP}.lnk" "$INSTDIR\UTBattle.exe"
  CreateShortcut "$SMPROGRAMS\${APP}\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
  CreateShortcut "$DESKTOP\${APP}.lnk" "$INSTDIR\UTBattle.exe"
  WriteRegStr HKCU "${REG}" "DisplayName" "${APP}"
  WriteRegStr HKCU "${REG}" "DisplayIcon" "$INSTDIR\UTBattle.exe"
  WriteRegStr HKCU "${REG}" "Publisher" "Innertale Engine"
  WriteRegStr HKCU "${REG}" "DisplayVersion" "${VER}"
  WriteRegStr HKCU "${REG}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKCU "${REG}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegDWORD HKCU "${REG}" "NoModify" 1
  WriteRegDWORD HKCU "${REG}" "NoRepair" 1
SectionEnd

Section "Uninstall"
  Delete "$DESKTOP\${APP}.lnk"
  RMDir /r "$SMPROGRAMS\${APP}"
  RMDir /r "$INSTDIR"
  DeleteRegKey HKCU "${REG}"
SectionEnd
