@ECHO off
PUSHD "%~dp0"

SET MSBuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
IF NOT EXIST "%MSBuild%" (
	ECHO Installation of .NET Framework 4.0 is required to build this project, including .NET v2.0 and v3.5 releases
	ECHO http://www.microsoft.com/downloads/details.aspx?FamilyID=0a391abd-25c1-4fc0-919f-b21f31ab88b7
	START /d "~\iexplore.exe" http://www.microsoft.com/downloads/details.aspx?FamilyID=0a391abd-25c1-4fc0-919f-b21f31ab88b7
	EXIT /b 1
	GOTO END
)

REM Unit Tests ------------------------------------------------------

ECHO.
ECHO Building unit test pass...
ECHO.

"%MSBuild%" JsonFx.sln /target:rebuild /property:TargetFrameworkVersion=v4.0;Configuration=Release;RunTests=True

REM Standard CLR ----------------------------------------------------

IF NOT EXIST "keys\JsonFx_Key.pfx" (
	SET Configuration=Release
) ELSE (
	SET Configuration=Signed
)

SET FrameworkVer=v2.0 v3.5 v4.0

ECHO.
ECHO Building specific releases for .NET Framework (%FrameworkVer%)...
ECHO.

FOR %%i IN (%FrameworkVer%) DO "%MSBuild%" src/JsonFx/JsonFx.csproj /target:rebuild /property:TargetFrameworkVersion=%%i;Configuration=%Configuration%

REM Silverlight -----------------------------------------------------

IF EXIST "%ProgramFiles%\MSBuild\Microsoft\Silverlight\v3.0\Microsoft.Silverlight.CSharp.targets" (
	SET SilverlightVer=%SilverlightVer% v3.5
)
IF EXIST "%ProgramFiles(x86)%\MSBuild\Microsoft\Silverlight\v3.0\Microsoft.Silverlight.CSharp.targets" (
	SET SilverlightVer=%SilverlightVer% v3.5
)
IF EXIST "%ProgramFiles%\MSBuild\Microsoft\Silverlight\v4.0\Microsoft.Silverlight.CSharp.targets" (
	SET SilverlightVer=%SilverlightVer% v4.0
)
IF EXIST "%ProgramFiles(x86)%\MSBuild\Microsoft\Silverlight\v4.0\Microsoft.Silverlight.CSharp.targets" (
	SET SilverlightVer=%SilverlightVer% v4.0
)

ECHO.
ECHO Building specific releases for Silverlight (%SilverlightVer%)...
ECHO.

FOR %%i IN (%SilverlightVer%) DO "%MSBuild%" src/JsonFx/JsonFx.csproj /target:rebuild /property:TargetFrameworkVersion=%%i;TargetFrameworkIdentifier=Silverlight;Configuration=%Configuration%

REM FOR %%i IN (%SilverlightVer%) DO "%MSBuild%" src/JsonFx/JsonFx.csproj /target:rebuild /property:TargetFrameworkVersion=%%i;TargetFrameworkIdentifier=Silverlight;Configuration=Debug

REM Windows Phone ---------------------------------------------------

IF EXIST "%ProgramFiles%\MSBuild\Microsoft\Silverlight for Phone\v4.0\Microsoft.Silverlight.CSharp.targets" (
	SET WindowsPhoneVer=%WindowsPhoneVer% v4.0
)
IF EXIST "%ProgramFiles(x86)%\MSBuild\Microsoft\Silverlight for Phone\v4.0\Microsoft.Silverlight.CSharp.targets" (
	SET WindowsPhoneVer=%WindowsPhoneVer% v4.0
)

ECHO.
ECHO Building specific releases for Windows Phone (%WindowsPhoneVer%)...
ECHO.

FOR %%i IN (%WindowsPhoneVer%) DO "%MSBuild%" src/JsonFx/JsonFx.csproj /target:rebuild /property:TargetFrameworkVersion=%%i;TargetFrameworkIdentifier=Silverlight;TargetFrameworkProfile=WindowsPhone;Configuration=%Configuration%

REM FOR %%i IN (%WindowsPhoneVer%) DO "%MSBuild%" src/JsonFx/JsonFx.csproj /target:rebuild /property:TargetFrameworkVersion=%%i;TargetFrameworkIdentifier=Silverlight;TargetFrameworkProfile=WindowsPhone;Configuration=Debug

:END
POPD
