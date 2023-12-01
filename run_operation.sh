#!/bin/bash
set -e

echo "Что делаем?"
echo "0) Собрать и запустить"
echo "1) git pull"
echo "2) nuget restore"
echo "3) cleanup packages directories"
echo "4) cleanup bin and obj directories"
echo "5) run dotnet tests"
echo "6) run SQL tests"
echo "7) run Net4.x tests"
echo "Можно вызывать вместе, например git+nuget=12"
read case;

cd "$(dirname "$0")"

case $case in
    *4*)
rm -v -f -R ./Workwear/bin/*
rm -v -f -R ./QSProjects/*/bin/
rm -v -f -R ./QSProjects/*/*/bin/
rm -v -f -R ./My-FyiReporting/*/bin/
rm -v -f -R ./My-FyiReporting/*/*/bin/
rm -v -f -R ./Gtk.DataBindings/System.Data.Bindings/bin
rm -v -f -R ./Workwear/obj/*
rm -v -f -R ./QSProjects/*/obj
rm -v -f -R ./QSProjects/*/*/obj
rm -v -f -R ./My-FyiReporting/*/obj/
rm -v -f -R ./My-FyiReporting/*/*/obj/
rm -v -f -R ./Gtk.DataBindings/System.Data.Bindings/obj
;;&
    *3*)
rm -v -f -R ./packages/*
rm -v -f -R ./QSProjects/packages/*
rm -v -f -R ./My-FyiReporting/packages/*
;;&
    *1*)
git pull --autostash
cd ../QSProjects
git pull --autostash
cd ../Gtk.DataBindings
git pull --autostash
cd ../GammaBinding
git pull --autostash
cd ../My-FyiReporting
git pull --autostash
cd ../Workwear
;;&
    *2*)
nuget restore Workwear.sln;
nuget restore ./QSProjects/QSProjectsLib.sln;
nuget restore ./My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln
;;&
    *5*)
dotnet test Workwear.Test/Workwear.Test.csproj
;;&
    *6*)
dotnet test Workwear.Test.Sql/Workwear.Test.Sql.csproj
;;&
    *7*)
msbuild /p:Configuration=Debug /p:Platform=x86 Workwear.sln    
cd WorkwearTest/bin/Debug/
cp -r ~/.nuget/packages/nunit.consolerunner/3.16.3/tools/* .
mono nunit3-console.exe WorkwearTest.dll --framework=mono-4.0
cd "$(dirname "$0")"
;;&
    *0*)
msbuild /p:Configuration=Debug /p:Platform=x86 Workwear.sln
cd Workwear/bin/Debug/
mono workwear.exe
esac

read -p "Press enter to exit"
