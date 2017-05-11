
Set-StrictMode -version 2.0
$ErrorActionPreference="Stop"

function Create-Directory([string]$dir) {
    [IO.Directory]::CreateDirectory($dir) | Out-Null
}

function Get-VsixVersion([string]$feedUrl, $vsixId) { 
    $vsixGuid = [Guid]::Parse($vsixId)
    [xml]$x = (Invoke-WebRequest $feedUrl -usebasicparsing).Content
    foreach ($v in $x.feed.entry) {
        $id = [Guid]::Parse($v.Id)
        if ($id -eq $vsixGuid) { 
            return $v.Vsix.Version
        }
    }

    throw "Unable to find vsix $vsixId for feed $feedUrl"
}

# Ensure the VSIX content for the URL is available and return a path to 
# the folder containing the unzipped content.
function Ensure-VsixContent($feedUrl, $vsixId, $version) {
    $scratchDir = ${env:TEMP}
    Create-Directory $scratchDir
    Push-Location $scratchDir
    try {
        $outFilePath = Join-Path $scratchDir "roslyn-data-$($version).vsix"
        if (-not (Test-Path $outFilePath)) {
            $vsixUrl = "$($feedUrl)/$($vsixId)-$($version).vsix"
            Remove-Item $outFilePath -ErrorAction SilentlyContinue
            Invoke-WebRequest -Uri $vsixUrl -OutFile $outFilePath
            Write-Host "Downloaded to $outFilePath"
        }

        $contentDir = Join-Path $scratchDir "content"
        Remove-Item $contentDir -Re -ErrorAction SilentlyContinue
        Create-Directory $contentDir | Out-Null
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [IO.Compression.ZipFile]::ExtractToDirectory($outFilePath, $contentDir)

        return $contentDir
    } 
    finally {
        Pop-Location
    }
}

try {
    $vsixId = "0b48e25b-9903-4d8b-ad39-d4cca196e3c7"
    $feedUrl = "https://dotnet.myget.org/F/roslyn/vsix"
    $version = Get-VsixVersion $feedUrl $vsixId
    $content = Ensure-VsixContent $feedUrl $vsixId $version

    foreach ($f in Get-ChildItem (Join-Path $content "Vsixes")) { 
        Write-Host $f.FullName
    }


    exit 0
}
catch {
    Write-Host $_
    exit 1
}
