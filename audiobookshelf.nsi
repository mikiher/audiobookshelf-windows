!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "StrFunc.nsh"
!include "x64.nsh"
!include "WinVer.nsh"
!include "WordFunc.nsh"
;${Using:StrFunc} StrRep 

;--------------------------------
Name "Audiobookshelf"
OutFile "AudiobookshelfInstaller.exe"
Unicode True

;Default installation folder
InstallDir "$PROGRAMFILES64\Audiobookshelf"

;Get installation folder from registry if available
InstallDirRegKey HKCU "Software\Audiobookshelf" "InstallDir"

;Request application privileges
RequestExecutionLevel admin

Var DataDir

;--------------------------------
;Macros


;--------------------------------
;Interface Settings

!define MUI_ABORTWARNING
!define MUI_ICON "AppIcon.ico"

;--------------------------------
;Pages

!insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\Modern UI\License.txt"
;!define MUI_PAGE_CUSTOMFUNCTION_PRE comp_pre
;!insertmacro MUI_PAGE_COMPONENTS
!define MUI_PAGE_HEADER_SUBTEXT "Choose a Folder in which to store Audiobookshelf program files."
!insertmacro MUI_PAGE_DIRECTORY
!define MUI_DIRECTORYPAGE_VARIABLE $DataDir
!define MUI_DIRECTORYPAGE_TEXT_DESTINATION "Data Folder"
!define MUI_DIRECTORYPAGE_TEXT_TOP "Audiobookshelf will use the following folder to store data and configuration files. To use a different folder, click Browse and select another folder. Click Next to continue"
!define MUI_PAGE_HEADER_TEXT "Choose Data Folder."
!define MUI_PAGE_HEADER_SUBTEXT "Choose a Folder in which to store Audiobookshelf configuration and data."
!insertmacro MUI_PAGE_DIRECTORY 
!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
!insertmacro MUI_LANGUAGE "English"

;--------------------------------


;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  File "WinApp\audiobookshelf\bin\Debug\net462\*"
  
  ;Store installation folder
  WriteRegStr HKCU "Software\Audiobookshelf" "InstallDir" $INSTDIR
  WriteRegStr HKCU "Software\Audiobookshelf" "DataDir" $DataDir
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ;${If} $FFMpegPath != ""
  ;  DetailPrint "FFMpeg found at $FFMpegPath"
  ;${Else}
  ;  DetailPrint "FFMpeg not found. Installing..."
  ;${EndIf}
  
  SectionIn RO

SectionEnd


;--------------------------------
;Descriptions

  ;Language strings
  ;LangString DESC_SecDummy ${LANG_ENGLISH} "A test section."
  ;LangString DESC_ffmpeg ${LANG_ENGLISH} "FFMpeg"

  ;Assign language strings to sections
  ;!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  ;  !insertmacro MUI_DESCRIPTION_TEXT ${SecDummy} $(DESC_SecDummy)
  ;  !insertmacro MUI_DESCRIPTION_TEXT ${FFMpeg} $(DESC_ffmpeg)
  ;!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...
  Delete "$INSTDIR\*"

  RMDir "$INSTDIR"

  DeleteRegKey /ifempty HKCU "Software\Audiobookshelf"

SectionEnd

;Function comp_pre
;
;  nsExec::ExecToStack 'where ffmpeg.exe'
;  Pop $0
;  Pop $1
;  ${If} $0 == 0
;    !insertmacro UnselectSection ${FFMpeg}
;  ${EndIf}
;  ${StrRep} $FFMpegPath $1 "\ffmpeg.exe" ""
;  MessageBox MB_OK "$FFMpegPath"
;
;Functionend

Function .onInit
  ReadRegStr $DataDir HKCU "Software\Audiobookshelf" "DataDir"
  ${If} $DataDir == ""
    StrCpy $DataDir "$LOCALAPPDATA\Audiobookshelf"
  ${EndIf}

  ; Check Windows version
  ${IfNot} ${AtLeastWin10}
  ${AndIfNot} ${AtLeastBuild} 14393 ; Anniversary Update (1607) or higher
  ${AndIfNot} ${RunningX64}
    MessageBox MB_OK "This application requires 64-bit Windows 10, Build 14393 (v1607), or higher."
    Abort
  ${EndIf}

  ; Find if audiobookshelf tray app is running
  FindWindow $0 "" "Audiobookshelf Tray"
  ${If} $0 != 0
    MessageBox MB_OKCANCEL "Audiobookshelf is already running.$\nOK to close it, Cancel to abort installation." IDOK close_tray
    Abort
    close_tray:
    SendMessage $0 0x10 0 0 ; WM_CLOSE
  ${EndIf}
FunctionEnd

