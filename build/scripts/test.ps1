
set-strictmode -version 2.0
$ErrorActionPreference="Stop"

try {
    . (join-path $PSScriptRoot "build-utils.ps1")
    write-host "Calling locate-msbuild-core"
    locate-msbuild-core
    write-host "Calling locate-msbuild"
    locate-msbuild
    write-host "Calling locate-msbuild -xcopy"
    locate-msbuild -xcopy

    write-host "Calling locate-vs"
    & (join-path $PSScriptRoot "locate-vs.ps1")
}
catch {
  Write-Error $_.Exception.Message

  # Add a blank line for anyone that wants to just grep the output of the 
  # program without checking for an error code
  write-host ""
  exit 1
}
