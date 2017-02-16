
# This script will ensure that the MSBuild toolset of the specified 
# version is downloaded and deployed to Binaries\Toolset\MSBuild
#
# This is a no-op if it is already present at the appropriate version

param ()
set-strictmode -version 2.0
$ErrorActionPreference="Stop"

$version = "0.1.7-alpha"
$repoDir = [IO.Path]::GetFullPath((join-path $PSScriptroot "..\.."))
$toolsDir = join-path $repoDir "Binaries\Toolset"
$scratchDir = join-path $toolsDir "Scratch"

try {

    mkdir $toolsDir -ErrorAction silent | out-null
    mkdir $scratchDir -ErrorAction silent | out-null
    pushd $toolsDir

    $scratchFile = join-path $scratchDir "msbuild.txt"
    $scratchVersion = gc -raw $scratchFile -ErrorAction SilentlyContinue
    if ($scratchVersion -eq ($version + [Environment]::NewLine)) { 
        exit 0
    }

    $tempFileName = "msbuild-$version.zip"
    $tempFile = join-path $scratchDir $tempFileName
    if (-not (test-path $tempFile)) { 
        $url = "https://jdashstorage.blob.core.windows.net/msbuild/msbuild-$version.zip" 
        write-host "Downloading $tempFileName"
        $webClient = New-Object -TypeName "System.Net.WebClient"
        $webClient.DownloadFile($url, $tempFile)
    }
    
    write-host "Unpacking msbuild.zip"
    rm -re (join-path $toolsDir "msbuild") -ErrorAction SilentlyContinue
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::ExtractToDirectory($tempFile, $toolsDir)

    $version | out-file $scratchFile 
    exit 0
}
catch [exception] {
    write-host $_.Exception
    exit 1
}
finally {
    popd
}
