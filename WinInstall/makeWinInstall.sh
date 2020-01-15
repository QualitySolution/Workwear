#!/bin/bash
set -e

cd "$(dirname "$0")"

ProjectName="Workwear"
BinDir=../$ProjectName/bin/ReleaseWin

# Сборка релиза
msbuild /p:Configuration=ReleaseWin /p:Platform=x86 ../workwear.sln

# Очистка бин от лишний файлов

rm -v -f ${BinDir}/*.mdb
rm -v -f ${BinDir}/*.pdb
rm -v -f -R ./Files/*

mkdir -p Files
cp -r -v ${BinDir}/* ./Files

if [ ! -f "gtk-sharp-2.12.21.msi" ]; then
    wget https://xamarin.azureedge.net/GTKforWindows/Windows/gtk-sharp-2.12.21.msi
fi

wine ~/.wine/drive_c/Program\ Files\ \(x86\)/NSIS/makensis.exe /INPUTCHARSET UTF8 ${ProjectName}.nsi
