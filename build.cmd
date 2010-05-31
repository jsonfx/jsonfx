@ECHO off
PUSHD "%~dp0"

SET MSBuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
IF NOT EXIST "%MSBuild%" (
    ECHO .NET Framework 4.0 is used to produce the various builds including for v2.0 and v3.5
    ECHO Lauching...
    ECHO http://www.microsoft.com/downloads/details.aspx?FamilyID=0a391abd-25c1-4fc0-919f-b21f31ab88b7
    start /d "~\iexplore.exe" http://www.microsoft.com/downloads/details.aspx?FamilyID=0a391abd-25c1-4fc0-919f-b21f31ab88b7
    EXIT /b 1
    GOTO ERROR
)

FOR %%i IN (v2.0 v3.5 v4.0) DO IF EXIST "%MSBuild%" "%MSBuild%" JsonFx2.Json.sln /property:TargetFrameworkVersion=%%i;Configuration=Release

:ERROR
POPD