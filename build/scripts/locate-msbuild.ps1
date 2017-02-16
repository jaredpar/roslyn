
param ([switch]$xcopy = $false) 

set-strictmode -version 2.0
$ErrorActionPreference="Stop"

try {
    . (join-path $PSScriptRoot "build-utils.ps1")
    $p = locate-msbuild $xcopy
    write-host $p
}
catch {
  Write-Error $_.Exception.Message

  # Add a blank line for anyone that wants to just grep the output of the 
  # program without checking for an error code
  write-host ""
  exit 1
}
