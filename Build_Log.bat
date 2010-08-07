@ECHO off

ECHO Building...

SET LOG=%~dp0build/Build.log

CALL "%~dp0Build.bat" %* > "%LOG%"
START notepad.exe "%LOG%"