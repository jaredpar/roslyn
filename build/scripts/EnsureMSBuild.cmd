@echo on

REM This is a wrapper around locate-msbuild which ensures the result is available on the PATH
REM variable.

call %~dp0LocateMSBuild.cmd %* || goto :Failed
set PATH=%RoslynMSBuildDir%;%PATH%
exit /b 0

:FAILED
echo Unable to locate MSBuild
exit /b 1

