#!/bin/bash
set -e

cd "$(dirname "$0")"

ProjectName="Workwear"
BinDir=../$ProjectName/bin/ReleaseWin
Configuration="ReleaseWin"
NsisOptions=""

# Параметры

while [ "$1" != "" ]; do
    case $1 in
        -b | --beta ) 
		NsisOptions+=" /DBETA"
		BinDir=../$ProjectName/bin/Debug
		Configuration="Debug"
                ;;
    esac
    shift
done

# Сборка релиза
msbuild /p:Configuration=${Configuration} /p:Platform=x86 ../Workwear.sln

# Очистка бин от лишний файлов

rm -v -f ${BinDir}/*.mdb
rm -v -f ${BinDir}/*.pdb
rm -v -f -R ./Files/*

mkdir -p Files
cp -r -v ${BinDir}/* ./Files

if [ ! -f "gtk-sharp-2.12.21.msi" ]; then
    wget https://files.qsolution.ru/Common/gtk-sharp-2.12.21.msi
fi

# Сборка документации
if command -v asciidoctor-pdf.ruby3.2 2>/dev/null; then
        BuildDoc=asciidoctor-pdf.ruby3.2 
if command -v asciidoctor-pdf.ruby3.1 2>/dev/null; then
        BuildDoc=asciidoctor-pdf.ruby3.1 
elif command -v asciidoctor-pdf.ruby2.7 2>/dev/null; then
        BuildDoc=asciidoctor-pdf.ruby2.7 
elif command -v asciidoctor-pdf.ruby2.6 2>/dev/null; then
        BuildDoc=asciidoctor-pdf.ruby2.6
elif command -v asciidoctor-pdf.ruby2.5 2>/dev/null; then
	BuildDoc=asciidoctor-pdf.ruby2.5
else
	echo "asciidoctor-pdf не установлен."
	exit 1
fi

${BuildDoc} ../docs/modules/ROOT/pages/user-guide.adoc
cp -v ../docs/modules/ROOT/pages/user-guide.pdf ./Files

${BuildDoc} ../docs/modules/ROOT/pages/admin-guide.adoc
cp -v ../docs/modules/ROOT/pages/admin-guide.pdf ./Files

wine ~/.wine/drive_c/Program\ Files\ \(x86\)/NSIS/makensis.exe /INPUTCHARSET UTF8 ${NsisOptions} ${ProjectName}.nsi
