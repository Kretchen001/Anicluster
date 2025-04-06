@echo off
setlocal enabledelayedexpansion

:: Setze den relativen Pfad zur version.txt-Datei
set versionFile=..\Anicluster\bin\Publish\version.txt

:: Existiert die Datei?
if not exist %versionFile% (
    echo Fehler: Die Datei version.txt wurde nicht gefunden!
    exit /b 1
)

:: Lese die Version aus der version.txt
for /f "delims=" %%i in (%versionFile%) do (
    set version=%%i
)

:: Version gelesen?
if not defined version (
    echo Fehler: Keine Version in der Datei gefunden!
    exit /b 1
)

:: Umbenennen der erzeugten Setup-Datei mit der echten Versionsnummer
ren .\setup_Anicluster_v_XXX.exe setup_Anicluster_v_%version%.exe

:: Ende
endlocal

