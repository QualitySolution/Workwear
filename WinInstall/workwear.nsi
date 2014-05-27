;--------------------------------
!define PRODUCT_VERSION "1.0"
!define MIN_NET_MAJOR "4"
!define MIN_NET_MINOR "0"
!define MIN_NET_BUILD "*"
!define NETInstaller "dotNetFx40_Full_setup.exe"
!define PRODUCT_NAME "QS: ���������� � ���������"
!define MENU_DIR_NAME "���������� � ���������"
!define EXE_NAME "workwear"

var NETInstalled

; The name of the installer
Name "${PRODUCT_NAME}"

; The file to write
OutFile "${EXE_NAME}-${PRODUCT_VERSION}.exe"

!include "MUI.nsh"

; The default installation directory
InstallDir "$PROGRAMFILES\${MENU_DIR_NAME}"

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

Function TestNetFramework
 
  ;Save the variables in case something else is using them
  Push $0
  Push $1
  Push $2
  Push $3
  Push $4
  Push $R1
  Push $R2
  Push $R3
  Push $R4
  Push $R5
  Push $R6
  Push $R7
  Push $R8
 
  StrCpy $R5 "0"
  StrCpy $R6 "0"
  StrCpy $R7 "0"
  StrCpy $R8 "0.0.0"
  StrCpy $0 0
 
  loop:
 
  ;Get each sub key under "SOFTWARE\Microsoft\NET Framework Setup\NDP"
  EnumRegKey $1 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP" $0
  StrCmp $1 "" done ;jump to end if no more registry keys
  IntOp $0 $0 + 1
  StrCpy $2 $1 1 ;Cut off the first character
  StrCpy $3 $1 "" 1 ;Remainder of string
 
  ;Loop if first character is not a 'v'
  StrCmpS $2 "v" start_parse loop
 
  ;Parse the string
  start_parse:
  StrCpy $R1 ""
  StrCpy $R2 ""
  StrCpy $R3 ""
  StrCpy $R4 $3
 
  StrCpy $4 1
 
  parse:
  StrCmp $3 "" parse_done ;If string is empty, we are finished
  StrCpy $2 $3 1 ;Cut off the first character
  StrCpy $3 $3 "" 1 ;Remainder of string
  StrCmp $2 "." is_dot not_dot ;Move to next part if it's a dot
 
  is_dot:
  IntOp $4 $4 + 1 ; Move to the next section
  goto parse ;Carry on parsing
 
  not_dot:
  IntCmp $4 1 major_ver
  IntCmp $4 2 minor_ver
  IntCmp $4 3 build_ver
  IntCmp $4 4 parse_done
 
  major_ver:
  StrCpy $R1 $R1$2
  goto parse ;Carry on parsing
 
  minor_ver:
  StrCpy $R2 $R2$2
  goto parse ;Carry on parsing
 
  build_ver:
  StrCpy $R3 $R3$2
  goto parse ;Carry on parsing
 
  parse_done:
 
  IntCmp $R1 $R5 this_major_same loop this_major_more
  this_major_more:
  StrCpy $R5 $R1
  StrCpy $R6 $R2
  StrCpy $R7 $R3
  StrCpy $R8 $R4
 
  goto loop
 
  this_major_same:
  IntCmp $R2 $R6 this_minor_same loop this_minor_more
  this_minor_more:
  StrCpy $R6 $R2
  StrCpy $R7 R3
  StrCpy $R8 $R4
  goto loop
 
  this_minor_same:
  IntCmp R3 $R7 loop loop this_build_more
  this_build_more:
  StrCpy $R7 $R3
  StrCpy $R8 $R4
  goto loop
 
  done:
  
  StrCpy $NETInstalled "yes"
  ;Have we got the framework we need?
  IntCmp $R5 ${MIN_NET_MAJOR} max_major_same fail end
  max_major_same:
  IntCmp $R6 ${MIN_NET_MINOR} max_minor_same fail end
  max_minor_same:
  IntCmp $R7 ${MIN_NET_BUILD} end fail end
 
  fail:
  StrCmp $R8 "0.0.0" no_framework
  goto wrong_framework
 
  no_framework:
  wrong_framework:
  StrCpy $NETInstalled "no"
 
  end:
 
  ;Pop the variables we pushed earlier
  Pop $R8
  Pop $R7
  Pop $R6
  Pop $R5
  Pop $R4
  Pop $R3
  Pop $R2
  Pop $R1
  Pop $4
  Pop $3
  Pop $2
  Pop $1
  Pop $0
 
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
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${EXE_NAME}" "DisplayName" "${PRODUCT_NAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${EXE_NAME}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${EXE_NAME}" "DisplayIcon" '"$INSTDIR\${EXE_NAME}.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${EXE_NAME}" "Publisher" "Quality Solution"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${EXE_NAME}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${EXE_NAME}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${EXE_NAME}" "NoRepair" 1
  WriteUninstaller "uninstall.exe"

  ; Start Menu Shortcuts
  CreateDirectory "$SMPROGRAMS\${MENU_DIR_NAME}"
  CreateShortCut "$SMPROGRAMS\${MENU_DIR_NAME}\��������.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\${MENU_DIR_NAME}\${PRODUCT_NAME}.lnk" "$INSTDIR\${EXE_NAME}.exe" "" "$INSTDIR\${EXE_NAME}.exe" 0
  CreateShortCut "$SMPROGRAMS\${MENU_DIR_NAME}\������������.lnk" "$INSTDIR\workwear_ru.pdf"
  
SectionEnd

Section "MS .NET Framework ${MIN_NET_MAJOR}.${MIN_NET_MINOR}" SecFramework
	SectionIn RO
	InitPluginsDir
	SetOutPath "$pluginsdir\Requires"

  call TestNetFramework
  StrCmp $NETInstalled "yes" NETFrameworkInstalled
  File ${NETInstaller}
 
	MessageBox MB_OK "��� ������ ��������� ���������� ��������� .NET Framework ${MIN_NET_MAJOR}.${MIN_NET_MINOR}. ����� ����� �������� ��������� ��������� ����� ��������, ���� ��� ��������� �� ��������� � ���������, ���������� ��������� �������."
  DetailPrint "Starting Microsoft .NET Framework v${MIN_NET_MAJOR}.${MIN_NET_MINOR} Setup..."
  ExecWait "$pluginsdir\Requires\${NETInstaller}"
  Return
 
  NETFrameworkInstalled:
  DetailPrint "Microsoft .NET Framework is already installed!"
 
SectionEnd

Section "GTK# 2.12.21" SecGTK
	SectionIn RO
; Install 2.12.21
  File "gtk-sharp-2.12.21.msi"
  ExecWait '"msiexec" /i "$pluginsdir\Requires\gtk-sharp-2.12.21.msi"  /passive'
; Setup Gtk style
  ${ConfigWrite} "$PROGRAMFILES\GtkSharp\2.12\share\themes\MS-Windows\gtk-2.0\gtkrc" "gtk-button-images =" "1" $R0
SectionEnd

Section "����� �� ������� ����" SecDesktop

	SetOutPath $INSTDIR
	CreateShortCut "$DESKTOP\${PRODUCT_NAME}.lnk" "$INSTDIR\${EXE_NAME}.exe" "" "$INSTDIR\${EXE_NAME}.exe" 0
 
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
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${EXE_NAME}"

  ; Remove files and uninstaller
  Delete $INSTDIR\*
  Delete $INSTDIR\uninstall.exe

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\${MENU_DIR_NAME}\*.*"
  Delete "$DESKTOP\${PRODUCT_NAME}.lnk"

  ; Remove directories used
  RMDir "$SMPROGRAMS\${MENU_DIR_NAME}"
  RMDir "$INSTDIR"

  ; Remove GTK#
  MessageBox MB_YESNO "������� ���������� GTK#? ��� ���� ����������� ��� ${PRODUCT_NAME}, �� ����� �������������� ������� ������������." /SD IDYES IDNO endGTK
    ExecWait '"msiexec" /X{06AF6533-F201-47C0-8675-AAAE5CB81B41} /passive'
  endGTK:
SectionEnd
