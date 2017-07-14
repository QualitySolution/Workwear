#!/bin/bash

ProjectName="workwear"
BinDir=../$ProjectName/bin/Release

# Очистка бин от лишний файлов

rm -v ${BinDir}/*.mdb
rm -v ${BinDir}/*.pdb

cp -r -v ${BinDir}/* ./Files

wine ~/.wine/drive_c/Program\ Files\ \(x86\)/NSIS/makensis.exe  ${ProjectName}.nsi
