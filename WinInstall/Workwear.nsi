Unicode true
;--------------------------------
!define PRODUCT_VERSION "2.8"
!define NET_VERSION "4.6.1"
!define EXE_NAME "workwear"
!define PRODUCT_NAME "QS: Спецпошив Аутсорсинг"
!define SHORTCUT_NAME "Спецпошив Аутсорсинг"
!define MENU_DIR_NAME "Спецпошив Аутсорсинг"
!define APP_DIR_NAME "Спецпошив Аутсорсинг"
!define UNINSTAL_KEY "workwear-sposhiv"
!define SETUP_POSTFIX "-sposhiv"

!ifdef BETA
	!define /redef PRODUCT_NAME "QS: Спецодежда и имущество БЕТА"
	!define /redef SHORTCUT_NAME "QS Спецодежда и имущество БЕТА"
	!define /redef MENU_DIR_NAME "Спецодежда и имущество БЕТА"
	!define /redef APP_DIR_NAME "Спецодежда и имущество beta"
	!define /redef UNINSTAL_KEY "workwear-beta"
	!define /redef SETUP_POSTFIX "-beta"
!endif

; The name of the installer
Name "${PRODUCT_NAME}"

; The file to write
OutFile "${EXE_NAME}-${PRODUCT_VERSION}${SETUP_POSTFIX}.exe"

!include "MUI.nsh"
!include "x64.nsh"

!addplugindir "NsisDotNetChecker\bin"
!include "NsisDotNetChecker\nsis\DotNetChecker_ru.nsh"

; The default installation directory
InstallDir "$PROGRAMFILES\${APP_DIR_NAME}"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;--------------------------------
; Pages

;!insertmacro MUI_PAGE_LICENSE "License.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
;Languages
 
!insertmacro MUI_LANGUAGE "Russian"

;--------------------------------
; Functions

Function ConfigWrite
	!define ConfigWrite `!insertmacro ConfigWriteCall`
 
	!macro ConfigWriteCall _FILE _ENTRY _VALUE _RESULT
		Push `${_FILE}`
		Push `${_ENTRY}`
		Push `${_VALUE}`
		Call ConfigWrite
		Pop ${_RESULT}
	!macroend
 
	Exch $2
	Exch
	Exch $1
	Exch
	Exch 2
	Exch $0
	Exch 2
	Push $3
	Push $4
	Push $5
	Push $6
	ClearErrors
 
	IfFileExists $0 0 error
	FileOpen $3 $0 a
	IfErrors error
 
	StrLen $0 $1
	StrCmp $0 0 0 readnext
	StrCpy $0 ''
	goto close
 
	readnext:
	FileRead $3 $4
	IfErrors add
	StrCpy $5 $4 $0
	StrCmp $5 $1 0 readnext
 
	StrCpy $5 0
	IntOp $5 $5 - 1
	StrCpy $6 $4 1 $5
	StrCmp $6 '$\r' -2
	StrCmp $6 '$\n' -3
	StrCpy $6 $4
	StrCmp $5 -1 +3
	IntOp $5 $5 + 1
	StrCpy $6 $4 $5
 
	StrCmp $2 '' change
	StrCmp $6 '$1$2' 0 change
	StrCpy $0 SAME
	goto close
 
	change:
	FileSeek $3 0 CUR $5
	StrLen $4 $4
	IntOp $4 $5 - $4
	FileSeek $3 0 END $6
	IntOp $6 $6 - $5
 
	System::Alloc /NOUNLOAD $6
	Pop $0
	FileSeek $3 $5 SET
	System::Call /NOUNLOAD 'kernel32::ReadFile(i r3, i r0, i $6, t.,)'
	FileSeek $3 $4 SET
	StrCmp $2 '' +2
	FileWrite $3 '$1$2$\r$\n'
	System::Call /NOUNLOAD 'kernel32::WriteFile(i r3, i r0, i $6, t.,)'
	System::Call /NOUNLOAD 'kernel32::SetEndOfFile(i r3)'
	System::Free $0
	StrCmp $2 '' +3
	StrCpy $0 CHANGED
	goto close
	StrCpy $0 DELETED
	goto close
 
	add:
	StrCmp $2 '' 0 +3
	StrCpy $0 SAME
	goto close
	FileSeek $3 -1 END
	FileRead $3 $4
	IfErrors +4
	StrCmp $4 '$\r' +3
	StrCmp $4 '$\n' +2
	FileWrite $3 '$\r$\n'
	FileWrite $3 '$1$2$\r$\n'
	StrCpy $0 ADDED
 
	close:
	FileClose $3
	goto end
 
	error:
	SetErrors
	StrCpy $0 ''
 
	end:
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0
FunctionEnd

;--------------------------------
; The stuff to install
Section "${PRODUCT_NAME}" SecProgram

  SectionIn RO

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File /r "Files\*.*"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTAL_KEY}" "DisplayName" "${PRODUCT_NAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTAL_KEY}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTAL_KEY}" "DisplayIcon" '"$INSTDIR\${EXE_NAME}.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTAL_KEY}" "Publisher" "Quality Solution"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTAL_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTAL_KEY}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTAL_KEY}" "NoRepair" 1
  WriteUninstaller "uninstall.exe"

  ; Start Menu Shortcuts
  SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\${MENU_DIR_NAME}"
  CreateShortCut "$SMPROGRAMS\${MENU_DIR_NAME}\Удаление.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\${MENU_DIR_NAME}\${SHORTCUT_NAME}.lnk" "$INSTDIR\${EXE_NAME}.exe" "" "$INSTDIR\${EXE_NAME}.exe" 0
  CreateShortCut "$SMPROGRAMS\${MENU_DIR_NAME}\Руководство пользователя.lnk" "$INSTDIR\user-guide.pdf"
  CreateShortCut "$SMPROGRAMS\${MENU_DIR_NAME}\Руководство администратора.lnk" "$INSTDIR\admin-guide.pdf"
  
  ; Удяляем файлы ненужные после версии 1.2.4.2
  Delete $INSTDIR\gtk-databind-lib.dll

  ; Удаляем файлы используемые до версии 2.0
  Delete $INSTDIR\EncryptionProvider.dll
  Delete $INSTDIR\Newtonsoft.Json.dll
  Delete $INSTDIR\RdlReader.exe
  Delete $INSTDIR\RdlViewer.dll
  Delete $INSTDIR\zxing.dll
  Delete $INSTDIR\ru-RU\RdlReader.resources.dll
  Delete $INSTDIR\ru-RU\RdlViewer.resources.dll
  
  ; Удаляем файлы используемые до версии 2.3.2
  Delete $INSTDIR\workwear_ru.pdf
  
  ; Удаляем файлы используемые до версии 2.5.1
   Delete "$SMPROGRAMS\${MENU_DIR_NAME}\Документация.lnk"

SectionEnd

Section "MS .NET Framework ${NET_VERSION}" SecFramework
  SectionIn RO

  !insertmacro CheckNetFramework 461
 
SectionEnd

Section "GTK# 2.12.21" SecGTK
  ;SectionIn RO

  ; Delete 2.12.21 if installed
  ;System::Call "msi::MsiQueryProductStateW(t '{71109D19-D8C1-437D-A6DA-03B94F5187FB}') i.r0"
  ;DetailPrint "msi::MsiQueryProductStateW 21: $0"
  ;StrCmp $0 "5" 0 GTKInstall
  ;DetailPrint "GTK# 2.12.21 установлен, удаляем"
  ;ExecWait '"msiexec" /X{71109D19-D8C1-437D-A6DA-03B94F5187FB} /passive'

  GTKInstall:
  ; Test 2.12.45
  System::Call "msi::MsiQueryProductStateW(t '{0D038544-52B1-4F30-BAE1-46509B4A91A7}') i.r0"
  StrCmp $0 "5" GTKDone
  DetailPrint "GTK# 2.12.45 не установлен"

  ; Test 2.12.38
  System::Call "msi::MsiQueryProductStateW(t '{C7A0CF1E-A936-426A-9694-035636DCD356}') i.r0"
  StrCmp $0 "5" GTKDone
  DetailPrint "GTK# 2.12.38 не установлен"

  ; Test 2.12.30
  System::Call "msi::MsiQueryProductStateW(t '{CA8017BD-8271-4C93-A409-186375C5A5CA}') i.r0"
  StrCmp $0 "5" GTKDone
  DetailPrint "GTK# 2.12.30 не установлен"

  ; Test 2.12.26
  System::Call "msi::MsiQueryProductStateW(t '{BC25B808-A11C-4C9F-9C0A-6682E47AAB83}') i.r0"
  StrCmp $0 "5" GTKDone
  DetailPrint "GTK# 2.12.26 не установлен"

  ; Test 2.12.25
  System::Call "msi::MsiQueryProductStateW(t '{889E7D77-2A98-4020-83B1-0296FA1BDE8A}') i.r0"
  StrCmp $0 "5" GTKDone
  DetailPrint "GTK# 2.12.25 не установлен"

  ; Test 2.12.21
  System::Call "msi::MsiQueryProductStateW(t '{71109D19-D8C1-437D-A6DA-03B94F5187FB}') i.r0"
  StrCmp $0 "5" GTKDone
  DetailPrint "GTK# 2.12.21 не установлен"

; Install 2.12.21
  DetailPrint "Запуск установщика GTK# 2.12.21"

  InitPluginsDir
  SetOutPath "$pluginsdir\Requires"
  File "gtk-sharp-2.12.21.msi"
  ExecWait '"msiexec" /i "$pluginsdir\Requires\gtk-sharp-2.12.21.msi"  /passive'

; Setup Gtk style
  ${ConfigWrite} "$PROGRAMFILES\GtkSharp\2.12\share\themes\MS-Windows\gtk-2.0\gtkrc" "gtk-button-images =" "1" $R0

; Fix Localication
  SetOutPath "$PROGRAMFILES\GtkSharp\2.12\share\locale\ru\LC_MESSAGES"
  File "LC_MESSAGES\*"
  GTKDone:
SectionEnd

Section "Ярлык на рабочий стол" SecDesktop

  SetShellVarContext all

  SetOutPath $INSTDIR
  CreateShortCut "$DESKTOP\${SHORTCUT_NAME}.lnk" "$INSTDIR\${EXE_NAME}.exe" "" "$INSTDIR\${EXE_NAME}.exe" 0
 
SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecProgram ${LANG_Russian} "Основные файлы программы"
  LangString DESC_SecFramework ${LANG_Russian} "Для работы программы необходима платформа .NET Framework. При необходимости будет выполнена установка через интернет."
  LangString DESC_SecGTK ${LANG_Russian} "Библиотеки GTK#, необходимые для работы программы"
  LangString DESC_SecDesktop ${LANG_Russian} "Установит ярлык программы на рабочий стол"

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecProgram} $(DESC_SecProgram)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecFramework} $(DESC_SecFramework)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecGTK} $(DESC_SecGTK)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDesktop} $(DESC_SecDesktop)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
; Uninstaller

Section "Uninstall"
  
  SetShellVarContext all
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${UNINSTAL_KEY}"

  ; Remove files and uninstaller
  Delete $INSTDIR\*
  Delete $INSTDIR\uninstall.exe

  Delete $INSTDIR\Reports\Employee\*
  RMDir $INSTDIR\Reports\Employee
  Delete $INSTDIR\Reports\Statements\*
  RMDir $INSTDIR\Reports\Statements
  Delete $INSTDIR\Reports\Stock\*
  RMDir $INSTDIR\Reports\Stock

  Delete $INSTDIR\Reports\*
  RMDir $INSTDIR\Reports

  Delete $INSTDIR\ru-RU\*
  RMDir $INSTDIR\ru-RU

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\${MENU_DIR_NAME}\*.*"
  Delete "$DESKTOP\${SHORTCUT_NAME}.lnk"

  ; Remove directories used
  RMDir "$SMPROGRAMS\${MENU_DIR_NAME}"
  RMDir "$INSTDIR"

  ; Remove GTK#
  MessageBox MB_YESNO "Удалить библиотеки GTK#? Они были установлены для ${PRODUCT_NAME}, но могут использоваться другими приложениями." /SD IDYES IDNO endGTK
    ExecWait '"msiexec" /X{71109D19-D8C1-437D-A6DA-03B94F5187FB} /passive'
  endGTK:
SectionEnd
