# Automatic Sample Harvesting for Installer

## Overview

The WiX installer now automatically includes all files from the `samples/` directory without requiring manual edits to the installer configuration. Simply add new sample folders to `samples/` and they'll be picked up during the next build.

## How It Works

1. **Pre-Build Step**: Before building the installer, the `HarvestSamples` MSBuild target runs
2. **Heat.exe**: WiX's Heat tool scans the entire `samples/` directory recursively
3. **Auto-Generation**: Creates `SamplesAuto.wxs` with all discovered files and folders
4. **Compilation**: The installer is compiled with both `Product.wxs` and `SamplesAuto.wxs`

## Adding New Samples

To add a new sample profile (e.g., "FinalCutPro"):

1. Create the folder:
   ```bash
   mkdir samples/FinalCutPro
   ```

2. Add your files:
   ```bash
   samples/FinalCutPro/
   ├── finalcutpro.json    # Sample configuration
   └── README.md           # Documentation
   ```

3. Build the installer:
   ```bash
   msbuild MacroButtons.Installer/MacroButtons.Installer.wixproj /p:Configuration=Release /p:Platform=x64
   ```

**That's it!** No need to edit any WiX files. The new sample will automatically be included.

## Files Modified

- **MacroButtons.Installer/MacroButtons.Installer.wixproj**: Added `HarvestSamples` build target
- **MacroButtons.Installer/Product.wxs**: Removed manual sample definitions
- **MacroButtons.Installer/SamplesAuto.wxs**: AUTO-GENERATED (do not edit manually)
- **.gitignore**: Added `SamplesAuto.wxs` to ignore list

## Current Samples Included

After these changes, all three samples are now included:

- ✅ **EliteDangerous** (was already in installer)
- ✅ **Lightroom** (now auto-included)
- ✅ **Photoshop** (now auto-included)

## Technical Details

### HarvestDirectory Task

The `HarvestSamples` target in the .wixproj file:

```xml
<Target Name="HarvestSamples" BeforeTargets="BeforeBuild">
  <HeatDirectory Directory="$(ProjectDir)..\samples"
                 DirectoryRefId="SAMPLESFOLDER"
                 ComponentGroupName="SamplesComponents"
                 OutputFile="SamplesAuto.wxs"
                 SuppressFragments="true"
                 SuppressRegistry="true"
                 SuppressRootDirectory="true"
                 AutogenerateGuids="true"
                 GenerateGuidsNow="true"
                 ToolPath="$(WixToolPath)"
                 PreprocessorVariable="var.SamplesSourceDir" />
</Target>
```

### Generated Structure

The auto-generated `SamplesAuto.wxs` creates:

1. **Directory structure**: Mirrors the `samples/` folder structure
2. **Components**: One WiX Component per file
3. **ComponentGroup**: A single `SamplesComponents` group referenced in `Product.wxs`

Example structure for `samples/Photoshop/photoshop.json`:
```xml
<Directory Id="dir_Photoshop" Name="Photoshop" />
<Component Id="cmp_photoshop.json" Directory="dir_Photoshop" Guid="*">
  <File Id="fil_photoshop.json" KeyPath="yes" Source="...\samples\Photoshop\photoshop.json" />
</Component>
```

## Troubleshooting

### Build Fails with "Heat.exe not found"

Ensure WiX Toolset 3.11+ is installed:
```bash
choco install wixtoolset --version=3.11.2
```

### Files Not Appearing in Installer

1. Verify files exist in `samples/` directory
2. Clean and rebuild:
   ```bash
   msbuild MacroButtons.Installer/MacroButtons.Installer.wixproj /t:Clean
   msbuild MacroButtons.Installer/MacroButtons.Installer.wixproj /p:Configuration=Release /p:Platform=x64
   ```
3. Check that `SamplesAuto.wxs` was regenerated (timestamp should be recent)

### Merge Conflicts in SamplesAuto.wxs

This file is auto-generated and should not be committed to Git. If it appears in a merge conflict:

1. Delete the conflicting file
2. Rebuild to regenerate it
3. Verify `.gitignore` includes `MacroButtons.Installer/SamplesAuto.wxs`

## GitHub Actions Integration

The GitHub Actions release workflow automatically:

1. Checks out the repository
2. Installs WiX Toolset
3. Builds the application
4. Runs `msbuild` on the installer project
   - This triggers `HarvestSamples` automatically
   - Generates `SamplesAuto.wxs` during the build
5. Creates the MSI with all samples included

No workflow changes were needed - it works out of the box!

## Benefits

- ✅ **No manual editing**: Just add files to `samples/`
- ✅ **Less error-prone**: No risk of forgetting to update GUIDs or file paths
- ✅ **Faster development**: Focus on creating samples, not installer configuration
- ✅ **Automatic structure**: Folder hierarchy is preserved automatically
- ✅ **Future-proof**: Any files added to `samples/` are automatically included

---

**Note**: The initial `SamplesAuto.wxs` file is included in the repository for reference, but it will be regenerated on every build. Do not edit it manually.
