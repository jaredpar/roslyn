Param(
  [string] $locateVsApiVersion = "0.2.4-beta"
)

set-strictmode -version 2.0
$ErrorActionPreference="Stop"

function Locate-LocateVsApi {
    $packagesPath = locate-packages
    $locateVsApi = Join-Path -path $packagesPath -ChildPath "RoslynTools.Microsoft.LocateVS\$locateVsApiVersion\tools\LocateVS.dll"

    if (!(Test-Path -path $locateVsApi)) {
        & (join-path $scriptDir "deploy-nuget.ps1")
        $jsonFile = join-path $repoDir "build\ToolsetPackages\project.json"
        $nuget = join-path $repoDir "NuGet.exe" 
        & $nuget restore $jsonFile -PackagesDirectory $packagesPath | out-null

        if (-not $?) { 
            throw "Unable to install the LocateVS API at version $locateVsApiVersion"
        }
    }

    return [IO.Path]::GetFullPath($locateVsApi)
}

try
{
    . (join-path $PSScriptRoot "build-utils.ps1")
    $locateVsApi = Locate-LocateVsApi
    $requiredPackageIds = @()

    $requiredPackageIds += "Microsoft.Component.MSBuild"
    $requiredPackageIds += "Microsoft.Net.Component.4.6.TargetingPack"
    $requiredPackageIds += "Microsoft.VisualStudio.Component.PortableLibrary"
    $requiredPackageIds += "Microsoft.VisualStudio.Component.Roslyn.Compiler"
    $requiredPackageIds += "Microsoft.VisualStudio.Component.VSSDK"

    Add-Type -path $locateVsApi
    $visualStudioInstallationPath = [LocateVS.Instance]::GetInstallPath("15.0", $requiredPackageIds)

    return Join-Path -Path $visualStudioInstallationPath -ChildPath "Common7\Tools\"
}
catch [exception] {
    write-host $_.Exception
    return ""
    exit 1
}
