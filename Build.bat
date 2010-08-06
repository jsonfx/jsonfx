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

IF NOT EXIST "keys\JsonFx_Key.pfx" (
	SET Configuration=Release
) ELSE (
	SET Configuration=Signed
)

ECHO Building unit test pass...
"%MSBuild%" JsonFx.sln /target:rebuild /property:TargetFrameworkVersion=v4.0;Configuration=Release;RunTests=True

ECHO Building specific releases for v2.0, v3.5 and v4.0...
FOR %%i IN (v2.0 v3.5 v4.0) DO "%MSBuild%" src/JsonFx/JsonFx.csproj /target:rebuild /property:TargetFrameworkVersion=%%i;Configuration=%Configuration%

:END
POPD
