# Copy selected sample profiles to user's .macrobuttons directory
param(
    [string]$InstallFolder,
    [string]$SelectedSamples = ""
)

$ErrorActionPreference = 'SilentlyContinue'

# Ensure destination directory exists
$destDir = Join-Path $env:USERPROFILE '.macrobuttons'
New-Item -ItemType Directory -Path $destDir -Force | Out-Null

# Source samples directory
$samplesDir = Join-Path $InstallFolder 'samples'

if (-not (Test-Path $samplesDir)) {
    exit 0
}

# Parse selected samples list (semicolon-separated, may have leading semicolon)
$selectedList = @()
if ($SelectedSamples) {
    $selectedList = $SelectedSamples -split ';' | Where-Object { $_ -and $_.Trim() } | ForEach-Object { $_.Trim() }
}

# If no samples specified, exit (don't copy anything)
if ($selectedList.Count -eq 0) {
    exit 0
}

# Copy only selected JSON files from sample subdirectories
Get-ChildItem -Path $samplesDir -Directory | Where-Object { $selectedList -contains $_.Name } | ForEach-Object {
    $sampleDir = $_
    $sampleName = $sampleDir.Name

    # Find JSON file in this sample directory
    $jsonFile = Get-ChildItem -Path $sampleDir.FullName -Filter '*.json' | Select-Object -First 1

    if ($jsonFile) {
        # Create destination filename: .macrobuttons-{samplename}.json
        $destFileName = ".macrobuttons-$($sampleName.ToLower()).json"
        $destFile = Join-Path $destDir $destFileName

        # Copy the file
        Copy-Item -Path $jsonFile.FullName -Destination $destFile -Force
    }

    # Special handling for EliteDangerous elite-status script
    if ($sampleName -eq 'EliteDangerous') {
        $scriptFile = Join-Path $sampleDir.FullName 'elite-status'
        if (Test-Path $scriptFile) {
            Copy-Item -Path $scriptFile -Destination (Join-Path $destDir 'elite-status') -Force
        }
    }
}

exit 0
