[CmdletBinding(PositionalBinding=$false)]
param ( [string]$bootstrapDir = "",
        [switch]$debugDeterminism = $false)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

### Variables available to the entire script.

# List of binary names that should be skipped because they have a known issue that
# makes them non-deterministic.  
$script:skipList = @()

# Holds the determinism data checked on every build.
$script:dataMap = @{}

# Location that deterministic error information should be written to. 
[string]$script:errorDir = ""
[string]$script:errorDirLeft = ""
[string]$script:errorDirRight = ""

function Run-Build([string]$rootDir, [string]$pathMapBuildOption, [switch]$restore = $false) {
    $sln = Join-Path $rootDir "Roslyn.sln"
    $debugDir = Join-Path $rootDir "Binaries\Debug"
    $objDir = Join-Path $rootDir "Binaries\Obj"

    # Create directories that may or may not exist to make the script execution below 
    # clean in either case.
    Create-Directory $debugDir
    Create-Directory $objDir

    Push-Location $rootDir
    try {

        # Clean out the previous run
        Write-Host "Cleaning the Binaries"
        Exec-Command $msbuild "/nologo /v:m /nodeReuse:false /t:clean $sln"

        if ($restore) {
            Write-Host "Restoring the packages"
            Restore-Project -fileName $sln -nuget (Ensure-NuGet) -msbuildDir (Split-Path -parent $msbuild)
        }

        Write-Host "Building the Solution"
        Exec-Command $msbuild "/nologo /v:m /nodeReuse:false /m /p:DebugDeterminism=true /p:BootstrapBuildPath=$script:bootstrapDir /p:Features=`"debug-determinism`" /p:UseRoslynAnalyzers=false $pathMapBuildOption $sln"
    }
    finally {
        Pop-Location
    }
}

# Return all of the files that need to be processed for determinism under the given
# directory.
function Get-FilesToProcess([string]$dir) {
    foreach ($item in Get-ChildItem -re -in *.dll,*.exe $dir) {
        $fileFullName = $item.FullName 
        $fileName = Split-Path -leaf $fileFullName
        $keyFullName = $fileFullName + ".key"
        $keyName = Split-Path -leaf $keyFullName

        # Do not process binaries that have been explicitly skipped or do not have a key
        # file.  The lack of a key file means it's a binary that wasn't specifically 
        # built for that directory (dependency).  Only need to check the binaries we are
        # building. 
        if ($script:skipList.Contains($fileName) -or -not (test-path $keyFullName)) {
            continue;
        }

        Write-Output $fileFullName
        $pdbFullName = [IO.Path]::ChangeExtension($fileFullName, ".pdb")
        if (Test-Path $pdbFullName) { 
            Write-Output $pdbFullName
        }
    }
}

function Run-Analysis([string]$rootDir, [bool]$buildMap, [string]$pathMapBuildOption, [switch]$restore = $false) {
            
    $debugDir = Join-Path $rootDir "Binaries\Debug"
    $errorList = @()
    $allGood = $true

    Run-Build $rootDir $pathMapBuildOption -restore:$restore

    Push-Location $debugDir

    Write-Host "Testing the binaries"
    foreach ($fileFullName in Get-FilesToProcess $debugDir) {
        $fileId = $fileFullName.Substring($debugDir.Length)
        $fileName = Split-Path -leaf $fileFullName
        $fileHash = (get-filehash $fileFullName -algorithm MD5).Hash
        $keyFullName = $fileFullName + ".key"
        $keyName = Split-Path -leaf $keyFullName

        if ($buildMap) {
            Write-Host "`tRecording $fileName = $fileHash"
            $data = @{}
            $data["Hash"] = $fileHash
            $data["Content"] = [IO.File]::ReadAllBytes($fileFullName)

            $keyBytes = ""
            if (Test-Path $keyFullName) { 
                $keyBytes = [IO.File]::ReadAllBytes($keyFullName)
            }

            $data["Key"] = $keyBytes
            $script:dataMap[$fileId] = $data
        }
        elseif (-not $script:dataMap.Contains($fileId)) {
            Write-Host "Missing entry in map $fileId->$fileFullName"
            $allGood = $false
        }
        else {
            $data = $script:dataMap[$fileId]
            $oldHash = $data.Hash
            if ($oldHash -eq $fileHash) {
                Write-Host "`tVerified $fileName"
            }
            else {
                Write-Host "`tERROR! $fileName"
                $allGood = $false
                $errorList += $fileName

                # Save out the original and baseline so Jenkins will archive them for investigation
                [IO.File]::WriteAllBytes((Join-Path $script:errorDirLeft $fileName), $data.Content)
                [IO.File]::WriteAllBytes((Join-Path $script:errorDirLeft $keyName), $data.Key)
                cp $fileFullName (Join-Path $script:errorDirRight $fileName)
                cp $keyFullName (Join-Path $script:errorDirRight $keyName)
            }
        }
    }

    Pop-Location

    # Sanity check to ensure we didn't return a false positive because we failed
    # to examine any binaries.
    if ($script:dataMap.Count -lt 10) {
        Write-Host "Found no binaries to process"
        $allGood = $false
    }

    if (-not $allGood) {
        Write-Host "Determinism failed for the following binaries:"
        foreach ($name in $errorList) {
            Write-Host "`t$name"
        }

        Write-Host "Archiving failure information"
        $zipFile = Join-Path $rootDir "Binaries\determinism.zip"
        Add-Type -Assembly "System.IO.Compression.FileSystem";
        [System.IO.Compression.ZipFile]::CreateFromDirectory($script:errorDir, $zipFile, "Fastest", $true);

        Write-Host "Please send $zipFile to compiler team for analysis"
        exit 1
    }
}

function Run-Test() {
    $origRootDir = Resolve-Path (Split-Path -parent (Split-Path -parent $PSScriptRoot))

    # Ensure the error directory is written for all analysis to use.
    $script:errorDir = Join-Path $origRootDir "Binaries\Determinism"
    $script:errorDirLeft = Join-Path $script:errorDir "Left"
    $script:errorDirRight = Join-Path $script:errorDir "Right"
    Create-Directory $script:errorDir
    Create-Directory $script:errorDirLeft
    Create-Directory $script:errorDirRight

    # Run initial build to populate all of the expected data.
    Run-Analysis -rootDir $origRootDir -buildMap $true

    # Run another build in same place and verify the build is identical.
    Run-Analysis -rootDir $origRootDir -buildMap $false

    # Run another build in a different source location and verify that path mapping 
    # allows the build to be identical.  To do this we'll copy the entire source 
    # tree under the Binaries\q directory and run a build from there.
    $origBinDir = Join-Path $origRootDir "Binaries"
    $altRootDir = Join-Path $origBinDir "q"
    & robocopy $origRootDir $altRootDir /E /XD $origBinDir /XD ".git" /njh /njs /ndl /nc /ns /np /nfl
    $pathMapBuildOption = "/p:PathMap=`"$altRootDir=$origRootDir`""
    Run-Analysis -rootDir $altRootDir -buildMap $false -pathMapBuildOption $pathMapBuildOption -restore
    Remove-Item -re -fo $altRootDir
}

try {
    . (Join-Path $PSScriptRoot "build-utils.ps1")

    $msbuild = Ensure-MSBuild
    if (($bootstrapDir -eq "") -or (-not ([IO.Path]::IsPathRooted($script:bootstrapDir)))) {
        Write-Host "The bootstrap build path must be absolute"
        exit 1
    }

    Run-Test
    exit 0
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
finally {
    Write-Host "Stopping VBCSCompiler"
    Get-Process VBCSCompiler -ErrorAction SilentlyContinue | Stop-Process
    Write-Host "Stopped VBCSCompiler"
}

