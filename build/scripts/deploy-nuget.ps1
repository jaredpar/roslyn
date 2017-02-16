
# This script will ensure that the MSBuild toolset of the specified 
# version is downloaded and deployed to Binaries\Toolset\MSBuild
#
# This is a no-op if it is already present at the appropriate version

param ()
set-strictmode -version 2.0
$ErrorActionPreference="Stop"

$version = "3.6.0-beta1"
$repoDir = [IO.Path]::GetFullPath((join-path $PSScriptroot "..\.."))
$toolsDir = join-path $repoDir "Binaries\Toolset"
$scratchDir = join-path $toolsDir "Scratch"

try {

    mkdir $toolsDir -ErrorAction silent | out-null
    mkdir $scratchDir -ErrorAction silent | out-null

    $scratchFile = join-path $scratchDir "nuget.txt"
    $scratchVersion = gc -raw $scratchFile -ErrorAction SilentlyContinue
    if ($scratchVersion -eq ($version + [Environment]::NewLine)) { 
        exit 0
    }

    $tempFileName = "nuget-$version.zip"
    $tempFile = join-path $scratchDir $tempFileName
    if (-not (test-path $tempFile)) { 
        $url = "https://dist.nuget.org/win-x86-commandline/v$version/NuGet.exe"
        write-host "Downloading $tempFileName"
        $webClient = New-Object -TypeName "System.Net.WebClient"
        $webClient.DownloadFile($url, $tempFile)
    }
    
    cp $tempFile (join-path $repoDir "NuGet.exe")

    $version | out-file $scratchFile 
    exit 0
}
catch [exception] {
    write-host $_.Exception
    exit 1
}
