
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
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Workwear" "Publisher" "Quality solution"
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
