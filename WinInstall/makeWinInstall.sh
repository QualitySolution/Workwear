#!/bin/bash
set -e

ProjectName="workwear"
BinDir=../$ProjectName/bin/ReleaseWin

# Сборка релиза
msbuild /p:Configuration=ReleaseWin /p:Platform=x86 ../workwear.sln

# Очистка бин от лишний файлов

rm -v ${BinDir}/*.mdb
rm -v ${BinDir}/*.pdb
rm -v -R ./Files/*

cp -r -v ${BinDir}/* ./Files

wine ~/.wine/drive_c/Program\ Files\ \(x86\)/NSIS/makensis.exe  ${ProjectName}.nsi
