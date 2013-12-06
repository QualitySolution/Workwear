;--------------------------------
!define PRODUCT_VERSION "1.0.0"
!define NETVersion "4.0"
!define NETInstaller "dotNetFx40_Full_setup.exe"
!define PRODUCT_NAME "QS: ���������� � ���������"

; The name of the installer
Name "${PRODUCT_NAME}"

; The file to write
OutFile "workwear-${PRODUCT_VERSION}.exe"

!include "MUI.nsh"

; The default installation directory
InstallDir $PROGRAMFILES\����������

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;--------------------------------
; Pages

!insertmacro MUI_PAGE_LICENSE "License.txt"
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
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Workwear" "DisplayName" "${PRODUCT_NAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Workwear" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Workwear" "DisplayIcon" '"$INSTDIR\workwear.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Workwear" "Publisher" "Quality Solution"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Workwear" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Workwear" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Workwear" "NoRepair" 1
  WriteUninstaller "uninstall.exe"

  ; Start Menu Shortcuts
  CreateDirectory "$SMPROGRAMS\���������� � ���������"
  CreateShortCut "$SMPROGRAMS\���������� � ���������\��������.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\���������� � ���������\���������� � ���������.lnk" "$INSTDIR\workwear.exe" "" "$INSTDIR\Bazar.exe" 0
  CreateShortCut "$SMPROGRAMS\���������� � ���������\������������.lnk" "$INSTDIR\UserGuide_ru.pdf"
  
SectionEnd

Section "MS .NET Framework v${NETVersion}" SecFramework
	SectionIn RO
	InitPluginsDir
	SetOutPath "$pluginsdir\Requires"

  IfFileExists "$WINDIR\Microsoft.NET\Framework\v${NETVersion}*" NETFrameworkInstalled 0
  File ${NETInstaller}
 
	MessageBox MB_OK "��� ������ ��������� ���������� ��������� .NET Framework ${NETVersion}. ����� ����� �������� ��������� ��������� ����� ��������, ���� ��� ��������� �� ��������� � ���������, ���������� ��������� �������."
  DetailPrint "Starting Microsoft .NET Framework v${NETVersion} Setup..."
  ExecWait "$pluginsdir\Requires\${NETInstaller}"
  Return
 
  NETFrameworkInstalled:
  DetailPrint "Microsoft .NET Framework is already installed!"
 
SectionEnd

Section "GTK# 2.12.22" SecGTK
	SectionIn RO
; Install 2.12.22
  File "gtk-sharp-2.12.22.msi"
  ExecWait '"msiexec" /i "$pluginsdir\Requires\gtk-sharp-2.12.22.msi"  /passive'
; Setup Gtk style
  ${ConfigWrite} "$PROGRAMFILES\GtkSharp\2.12\share\themes\MS-Windows\gtk-2.0\gtkrc" "gtk-button-images =" "1" $R0
SectionEnd

Section "����� �� ������� ����" SecDesktop

	SetOutPath $INSTDIR
	CreateShortCut "$DESKTOP\QS: ���������� � ���������.lnk" "$INSTDIR\workwear.exe" "" "$INSTDIR\workwear.exe" 0
 
SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecProgram ${LANG_Russian} "�������� ����� ���������"
  LangString DESC_SecFramework ${LANG_Russian} "��� ������ ��������� ���������� ��������� .NET Framework. ��� ������������� ����� ��������� ��������� ����� ��������."
  LangString DESC_SecGTK ${LANG_Russian} "���������� GTK#, ����������� ��� ������ ���������"
  LangString DESC_SecDesktop ${LANG_Russian} "��������� ����� ��������� �� ������� ����"

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
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Workwear"

  ; Remove files and uninstaller
  Delete $INSTDIR\*
  Delete $INSTDIR\uninstall.exe

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\���������� � ���������\*.*"
  Delete "$DESKTOP\QS: ���������� � ���������.lnk"

  ; Remove directories used
  RMDir "$SMPROGRAMS\���������� � ���������"
  RMDir "$INSTDIR"

  ; Remove GTK#
  MessageBox MB_YESNO "������� ���������� GTK#? ��� ���� ����������� ��� ${PRODUCT_NAME}, �� ����� �������������� ������� ������������." /SD IDYES IDNO endGTK
    ExecWait '"msiexec" /X{06AF6533-F201-47C0-8675-AAAE5CB81B41} /passive'
  endGTK:
SectionEnd


