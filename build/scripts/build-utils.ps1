# Collection of powershell build utility functions that we use across our scripts.

set-strictmode -version 2.0
$ErrorActionPreference="Stop"

# Declare a number of useful variables for other scripts to use
[string]$repoDir = [IO.Path]::GetFullPath($(join-path $PSScriptRoot "..\.."))
[string]$binariesDir = join-path $repoDir "Binaries"
[string]$scriptDir = $PSScriptRoot

# The intent of this script is to locate and return the path to the MSBuild directory that
# we should use for bulid operations.  The preference order for MSBuild to use is as 
# follows
#
#   1. MSBuild from an active VS command prompt
#   2. MSBuild from a machine wide VS install
#   3. MSBuild from the xcopy toolset 
#
# This function will return two values: the kind of MSBuild chosen and the MSBuild directory.
function locate-msbuild-core() {
    param ([switch]$xcopy = $false) 

    if ($xcopy) { 
        write-output "xcopy"
        write-output (locate-msbuild-xcopy)
        return
    }

    # MSBuild from an active VS command prompt.  
    if (${env:VSINSTALLDIR} -ne $null) {

        # This line deliberately avoids using -ErrorAction.  Inside a VS command prompt
        # an MSBuild command should always be available.
        $p = (get-command msbuild).Source
        write-output "vscmd"
        write-output $p
        return
    }

    # Look for a valid VS installation
    try {
        # It's important to execute locate-vs using a new powershell process.  This script will 
        # use assemblies from our NuGet cache to locate VS.  This will cause them to be locked on
        # disk for the duration of the powershell session.  Using a separate process makes sure
        # they are not locked to this session which can be long lived.
        $p = & powershell -noprofile -executionPolicy RemoteSigned -file (join-path $PSScriptRoot "locate-vs.ps1")
        if ($? -and (test-path $p)) {
            $p = join-path $p "..\..\MSBuild\15.0\Bin"
            $p = [IO.Path]::GetFullPath($p)
            write-output "vsinstall"
            write-output $p
            return
        }
    }
    catch { 
        # Failures are expected here when 
    }

    write-output "xcopy"
    write-output (locate-msbuild-xcopy)
    return
}

function locate-msbuild() {
    param ([switch]$xcopy = $false) 
    $both = locate-msbuild-core -xcopy:$xcopy
    return $both[1]
}

# Locate the xcopy version of MSBuild
function locate-msbuild-xcopy() {
    $version = "0.1.5-alpha"
    $packagesDir = locate-packages
    $p = join-path $packagesDir "RoslynTools.XCopyMSBuild.$($version)\tools"
    if (-not (test-path $p)) { 
        $nuget = join-path $repoDir "NuGet.exe"
        if (-not (test-path $nuget)) { 
            & (join-path $PSScriptRoot "deploy-nuget.ps1")
        }

        $output = & $nuget install -OutputDirectory $packagesDir -Version $version RoslynTools.XCopyMSBuild
        if (-not $?) { 
            write-host $output
            throw $"Unable to restore xcopy MSBuild toolset"
        }
    }


    $p = [IO.Path]::GetFullPath($p)
    return $p
}

# Locate the xcopy version of the framework assemblies that are used by MSBuild
function locate-framework-xcopy() {
    $msbuildDir = locate-msbuild-xcopy
    return (join-path $msbuildDir "Framework")
}

function locate-reference-assemblies() {
    $both = locate-msbuild-core
    if ($both[0] -eq "xcopy") { 
        return (locate-framework-xcopy)
    }

    return $null
}

# This function will ensure that MSBuild is available on the PATH and that any other
# environment variables necessary for MSBuild to function are also set.
function ensure-msbuild() {
    param ([switch]$xcopy = $false) 
    $both = locate-msbuild-core $xcopy
    $msbuildDir = $both[1]
    switch ($both[0]) {
        "xcopy" {
            ${env:PATH}="$msbuildDir;${env:PATH}"
            ${env:TargetFrameworkRootPath}=(locate-framework-xcopy)
            break;
        }
        "vscmd" {
            # Nothing to do here as the VS Dev CMD should set all appropriate environment
            # variables.
            break;
        }
        "vsinstall" {
            ${env:PATH}="$msbuildDir;${env:PATH}"
            break;
        }
        default {
            throw "Unknown MSBuild installation type $($both[0])"
        }
    }
}

# This is a simple placeholder for now to make it easy to call from powershell.  Evetnually we 
# will mirgate the script to powershell and avoid the shell out to CMD here. 
function restore-packages() {
    param ([switch]$xcopy = $false) 

    $restore = join-path $PSScriptRoot "..\..\Restore.cmd"
    $msbuildDir = locate-msbuild $xcopy
    & $restore /msbuild $msbuildDir

    if (-not $?) {
        throw "Restore failed"
    }
}

# This function will return the directory where our NuGet packages are being 
# restored to.  Needs to be kept up to date with the logic in Versions.props
function locate-packages {
    $d = $null
    if ($env:NUGET_PACKAGES -ne $null) {
        $d = $env:NUGET_PACKAGES
    }
    else {
        $d = join-path $env:UserProfile ".nuget\packages\"
    }

    mkdir $d -ErrorAction SilentlyContinue | out-null
    return $d
}
