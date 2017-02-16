set-strictmode -version 2.0
$ErrorActionPreference="Stop"

pushd $PSScriptRoot
try {

    . (join-path $PSScriptRoot "..\..\..\build\scripts\build-utils.ps1")

    # Ensure we are in a known clean state before running the build
    & (join-path $scriptDir "deploy-nuget.ps1")
    $nuget = join-path $repoDir "NuGet.exe"
    & $nuget locals all -clear 
    if (-not $?) { 
        throw "Error clearing locals"
    }

    # Get MSBuild and restore setup
    $msbuildDir = ensure-msbuild -xcopy
    restore-packages -xcopy 

    & msbuild /nodereuse:false /p:Configuration=Release /p:SkipTest=true Build.proj 
    if (-not $?) { 
        throw "Error during build"
    }

    exit 0
}
catch [exception] {
    write-host $_.Exception
    exit 1
}
finally {
    popd
}

