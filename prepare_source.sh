#!/bin/bash
echo "Что делаем?"
echo "1) git pull"
echo "2) nuget restore"
echo "3) cleanup packages directories"
echo "4) cleanup bin and obj directories"
echo "5) rebuild dependent dotnet libraries"
echo "Можно вызывать вместе, например git+nuget=12"
read case;

cd "$(dirname "$0")"

case $case in
    *4*)
rm -v -f -R ./Workwear/bin/*
rm -v -f -R ../QSProjects/*/bin/*
rm -v -f -R ../My-FyiReporting/*/bin/*
rm -v -f -R ./Workwear/obj/*
rm -v -f -R ../QSProjects/*/obj/*
rm -v -f -R ../My-FyiReporting/*/obj/*
;;&
    *3*)
rm -v -f -R ./packages/*
rm -v -f -R ../QSProjects/packages/*
rm -v -f -R ../My-FyiReporting/packages/*
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
nuget restore ../QSProjects/QSProjectsLib.sln;
nuget restore ../My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln
;;&
    *5*)
dotnet publish /p:Platform=x86 ../QSProjects/QS.Cloud.Client/QS.Cloud.Client.csproj
dotnet publish /p:Platform=x86 ./QS.Cloud.WearLk.Client/QS.Cloud.WearLk.Client.csproj  
;;&
esac


read -p "Press enter to exit"
