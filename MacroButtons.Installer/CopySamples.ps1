# Copy selected sample profiles to user's .macrobuttons directory
# Called by MSI installer deferred custom action
# SelectedSamples is pipe-delimited list of sample folder names (e.g., "WindowsDesktop|VSCode|ComfyUI")
param(
    [string]$InstallFolder,
    [string]$SelectedSamples = ""
)

$ErrorActionPreference = 'SilentlyContinue'

# Log to temp file for debugging (can be removed once working)
$logFile = Join-Path $env:TEMP 'MacroButtons_CopySamples.log'
"$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Script started" | Out-File $logFile -Append
"InstallFolder: $InstallFolder" | Out-File $logFile -Append
"SelectedSamples: $SelectedSamples" | Out-File $logFile -Append

# Ensure destination directory exists
$destDir = Join-Path $env:USERPROFILE '.macrobuttons'
New-Item -ItemType Directory -Path $destDir -Force | Out-Null

# Source samples directory
$samplesDir = Join-Path $InstallFolder 'samples'

if (-not (Test-Path $samplesDir)) {
    exit 0
}

# Parse selected samples list (pipe-separated, may have leading pipe)
$selectedList = @()
if ($SelectedSamples) {
    $selectedList = $SelectedSamples -split '\|' | Where-Object { $_ -and $_.Trim() } | ForEach-Object { $_.Trim() }
}

"Parsed samples: $($selectedList -join ', ')" | Out-File $logFile -Append
"Sample count: $($selectedList.Count)" | Out-File $logFile -Append

# If no samples specified, exit (don't copy anything)
if ($selectedList.Count -eq 0) {
    "No samples to copy, exiting" | Out-File $logFile -Append
    exit 0
}

"Destination directory: $destDir" | Out-File $logFile -Append
"Samples directory: $samplesDir" | Out-File $logFile -Append
"Samples dir exists: $(Test-Path $samplesDir)" | Out-File $logFile -Append

# Copy only selected JSON files from sample subdirectories
$foundDirs = Get-ChildItem -Path $samplesDir -Directory -ErrorAction SilentlyContinue
"Found directories in samples: $($foundDirs.Name -join ', ')" | Out-File $logFile -Append

Get-ChildItem -Path $samplesDir -Directory -ErrorAction SilentlyContinue | Where-Object { $selectedList -contains $_.Name } | ForEach-Object {
    $sampleDir = $_
    $sampleName = $sampleDir.Name
    "Processing sample: $sampleName" | Out-File $logFile -Append

    # Find JSON file in this sample directory
    $jsonFile = Get-ChildItem -Path $sampleDir.FullName -Filter '*.json' -ErrorAction SilentlyContinue | Select-Object -First 1

    if ($jsonFile) {
        # Create destination filename: .macrobuttons-{samplename}.json
        $destFileName = ".macrobuttons-$($sampleName.ToLower()).json"
        $destFile = Join-Path $destDir $destFileName

        # Copy the file
        "Copying $($jsonFile.FullName) to $destFile" | Out-File $logFile -Append
        Copy-Item -Path $jsonFile.FullName -Destination $destFile -Force -ErrorAction SilentlyContinue
        if (Test-Path $destFile) {
            "Successfully copied $destFileName" | Out-File $logFile -Append
        } else {
            "FAILED to copy $destFileName" | Out-File $logFile -Append
        }
    }

    # Special handling for EliteDangerous elite-status script
    if ($sampleName -eq 'EliteDangerous') {
        $scriptFile = Join-Path $sampleDir.FullName 'elite-status'
        if (Test-Path $scriptFile) {
            Copy-Item -Path $scriptFile -Destination (Join-Path $destDir 'elite-status') -Force -ErrorAction SilentlyContinue
            "Copied elite-status script" | Out-File $logFile -Append
        }
    }
}

"Script completed" | Out-File $logFile -Append
exit 0
