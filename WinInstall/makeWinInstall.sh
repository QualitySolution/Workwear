#!/bin/bash
set -e

cd "$(dirname "$0")"

ProjectName="workwear"
BinDir=../$ProjectName/bin/ReleaseWin

# Сборка релиза
msbuild /p:Configuration=ReleaseWin /p:Platform=x86 ../workwear.sln

# Очистка бин от лишний файлов

rm -v -f ${BinDir}/*.mdb
rm -v -f ${BinDir}/*.pdb
rm -v -f -R ./Files/*

cp -r -v ${BinDir}/* ./Files

wine ~/.wine/drive_c/Program\ Files\ \(x86\)/NSIS/makensis.exe /INPUTCHARSET UTF8 ${ProjectName}.nsi
