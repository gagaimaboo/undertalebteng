; Windows installer for the C# port. Built on Linux with makensis (apt install nsis):
;   cd port/installer && makensis UTBattle.nsi
; Needs port/UTBattle/publish/win-x64/UTBattle.exe (see port/README.md). Installs per user, so no
; admin prompt, into %LOCALAPPDATA%\Programs\UTBattle with Start menu and desktop shortcuts.
Unicode true
!include "MUI2.nsh"

!define APP "Undertale Battle Engine (C#)"
!define REG "Software\Microsoft\Windows\CurrentVersion\Uninstall\UTBattleCSharp"

Name "${APP}"
OutFile "UTBattle-CSharp-Setup.exe"
InstallDir "$LOCALAPPDATA\Programs\UTBattle"
InstallDirRegKey HKCU "${REG}" "InstallLocation"
RequestExecutionLevel user
SetCompressor /SOLID lzma
ManifestDPIAware true

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
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  CreateDirectory "$SMPROGRAMS\${APP}"
  CreateShortcut "$SMPROGRAMS\${APP}\${APP}.lnk" "$INSTDIR\UTBattle.exe"
  CreateShortcut "$SMPROGRAMS\${APP}\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
  CreateShortcut "$DESKTOP\${APP}.lnk" "$INSTDIR\UTBattle.exe"
  WriteRegStr HKCU "${REG}" "DisplayName" "${APP}"
  WriteRegStr HKCU "${REG}" "DisplayIcon" "$INSTDIR\UTBattle.exe"
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
