@ECHO off
PUSHD "%~dp0"

SET MSBuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
IF NOT EXIST "%MSBuild%" (
	ECHO Installation of .NET Framework 4.0 is required to build this project, including .NET v2.0 and v3.5 releases
	ECHO NOTE: Visual Studio is NOT needed to successfully build this project.
	ECHO http://www.microsoft.com/downloads/details.aspx?FamilyID=0a391abd-25c1-4fc0-919f-b21f31ab88b7
	START /d "~\iexplore.exe" http://www.microsoft.com/downloads/details.aspx?FamilyID=0a391abd-25c1-4fc0-919f-b21f31ab88b7
	EXIT /b 1
	GOTO END
)

IF NOT EXIST "keys\JsonFx_Key.pfx" (
	SET Configuration=Release
) ELSE (
	SET Configuration=Signed
)

ECHO.
ECHO Building unit test pass...
"%MSBuild%" JsonFx.sln /target:rebuild /property:TargetFrameworkVersion=v4.0;Configuration=Release;RunTests=True

ECHO.
ECHO Building specific releases for .NET Framework v2.0, v3.5 and v4.0...
FOR %%i IN (v2.0 v3.5 v4.0) DO "%MSBuild%" src/JsonFx/JsonFx.csproj /target:rebuild /property:TargetFrameworkVersion=%%i;Configuration=%Configuration%

ECHO.
ECHO Building specific release for Silverlight v4.0...
IF EXIST "%SystemRoot%\Program Files\MSBuild\Microsoft\Silverlight\v4.0\Microsoft.Silverlight.CSharp.targets" (
	"%MSBuild%" src/JsonFx/JsonFx.csproj /target:rebuild /property:TargetFrameworkVersion=v4.0;TargetFrameworkIdentifier=Silverlight;Configuration=%Configuration%
)
REM IF EXIST "%SystemRoot%\Program Files (x86)\MSBuild\Microsoft\Silverlight\v4.0\Microsoft.Silverlight.CSharp.targets" (
	"%MSBuild%" src/JsonFx/JsonFx.csproj /target:rebuild /property:TargetFrameworkVersion=v4.0;TargetFrameworkIdentifier=Silverlight;Configuration=%Configuration%
REM )

:END
POPD
