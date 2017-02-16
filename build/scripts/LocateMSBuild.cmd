@echo off

REM Batch wrapper for locate-msbuild.ps1.  Will set RoslynMSBuildDir to the preferred
REM MSBuild directory on successful execution

for /f "usebackq delims=" %%v in (`powershell -noprofile -executionPolicy Bypass -file "%~dp0locate-msbuild.ps1" "%*"`) do (
    set RoslynMSBuildDir=%%v
    echo %%v
)

if NOT EXIST "%RoslynMSBuildDir%" (
    echo Unable to find MSBuild
    exit /b 1
)
