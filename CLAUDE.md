# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

MacroButtons is a WPF/C# application designed for touch-screen macro button panels with a retro terminal aesthetic. The application provides a full-screen, non-activating window that displays a grid of customizable tiles for executing macros (keystrokes, Python scripts, PowerShell commands, executables) or displaying dynamic information.

**Key Features:**
- Non-activating window that never steals focus (critical for gaming/creation workflows)
- AutoHotkey-style keystroke simulation (pure C# implementation)
- Silent Python/executable/PowerShell execution with stdout capture
- In-process PowerShell execution via System.Management.Automation (no console windows)
- Dynamic information tiles with configurable refresh intervals
- Multi-monitor support
- Retro terminal aesthetic with configurable themes (monospace fonts, configurable colors)

## Architecture Overview

### Technology Stack
- **Framework**: .NET 8 Windows (WPF)
- **Architecture**: MVVM (Model-View-ViewModel)
- **UI Framework**: WPF with UniformGrid layout
- **Installer**: WiX Toolset 3.11
- **CI/CD**: GitHub Actions

### Project Structure

```
MacroButtons/
├── MacroButtons.sln                    # Solution file
├── MacroButtons/                       # Main WPF application
│   ├── MacroButtons.csproj             # .NET 8 WPF project
│   ├── App.xaml / App.xaml.cs          # Application entry point with single-instance check
│   ├── MainWindow.xaml / xaml.cs       # Main window with non-activating behavior
│   │
│   ├── Models/                         # Configuration data models
│   │   ├── MacroButtonConfig.cs        # Root configuration (Items, Theme, Global)
│   │   ├── ButtonItem.cs               # Individual button definition
│   │   ├── ActionDefinition.cs         # Action types (Keypress, Python, PowerShell, Exe)
│   │   ├── TitleDefinition.cs          # Dynamic title command definition
│   │   ├── ThemeConfig.cs              # Foreground/Background colors
│   │   └── GlobalConfig.cs             # Refresh interval, MonitorIndex
│   │
│   ├── ViewModels/                     # MVVM view models
│   │   ├── ViewModelBase.cs            # Base class with ObservableObject
│   │   ├── MainViewModel.cs            # Main window logic, grid calculation
│   │   └── ButtonTileViewModel.cs      # Individual tile logic, action execution, per-tile refresh
│   │
│   ├── Views/                          # User controls
│   │   └── ButtonTile.xaml / xaml.cs   # Individual tile UI with retro styling
│   │
│   ├── Services/                       # Business logic services
│   │   ├── ConfigurationService.cs     # JSON config loading/creation
│   │   ├── CommandExecutionService.cs  # Silent process execution
│   │   ├── PowerShellService.cs        # In-process PowerShell execution via runspaces
│   │   ├── LoggingService.cs           # Application event and error logging
│   │   ├── KeystrokeService.cs         # Keystroke simulation
│   │   └── MonitorService.cs           # Multi-monitor management
│   │
│   ├── Helpers/                        # Utility classes
│   │   ├── AutoHotkeyParser.cs         # Parse AHK syntax to VirtualKeyCodes
│   │   ├── WindowHelper.cs             # Win32 window management
│   │   └── ColorConverter.cs           # Color string parsing
│   │
│   └── Resources/
│       ├── DefaultConfig.json          # Embedded default configuration
│       └── MacroButtons.ico            # Application icon
│
├── MacroButtons.Installer/             # WiX installer project
│   ├── MacroButtons.Installer.wixproj  # WiX 3.11 project
│   ├── Product.wxs                     # Installer definition
│   ├── License.rtf                     # MIT License for installer
│   └── MacroButtons.ico                # Icon for Add/Remove Programs
│
└── .github/workflows/
    └── release.yml                     # Automated release pipeline
```

## Core Components Deep Dive

### 1. Non-Activating Window (MainWindow.xaml.cs)

**Critical Win32 API Integration:**
```csharp
// WS_EX_NOACTIVATE prevents window from stealing focus
private const int WS_EX_NOACTIVATE = 0x08000000;
private const int GWL_EXSTYLE = -20;
```

**Key Behaviors:**
- `OnSourceInitialized()`: Sets WS_EX_NOACTIVATE style via SetWindowLong
- `Window_Loaded()`: Positions window on configured monitor using MonitorService
- `OnActivated()`: Stores previous window handle for keystroke restoration
- `Topmost = true`: Ensures window occludes Start Menu

**Important:** Never remove WS_EX_NOACTIVATE - this is essential for gaming/creation workflows where focus must remain on the primary application.

### 2. Configuration System

**File Location:** `%USERPROFILE%\.macrobuttons.json`

**JSON Schema:**
```json
{
  "items": [
    {
      "title": "Static Text" | {
        "python": [...] | "exe": [...] | "powershell": "command" | "powershellScript": "path/to/script.ps1",
        "refresh": "100ms",  // Optional: per-tile refresh interval (overrides global)
        "powershellParameters": { "ParamName": "value" }  // Optional: named parameters for PowerShell
      },
      "action": {
        "keypress": "^v"
      } | {
        "python": [...]
      } | {
        "exe": "..."
      } | {
        "powershell": "Get-Date -Format 'HH:mm:ss'"  // Inline PowerShell command
      } | {
        "powershellScript": "~/scripts/my-script.ps1",  // PowerShell script file
        "powershellParameters": { "Name": "value" }  // Optional: named parameters
      } | null
    }
  ],
  "theme": {
    "foreground": "darkgreen",  // Named color or hex
    "background": "black"
  },
  "global": {
    "refresh": "30s",           // Format: \d+(ms|s|m|h) (milliseconds/seconds/minutes/hours)
    "monitorIndex": 0           // 0-based monitor index
  }
}
```

**Special Handling:**
- `ButtonItem.Title` can be string OR TitleDefinition (polymorphic deserialization)
- Each dynamic tile can override the global refresh interval (minimum 100ms)
- Uses Newtonsoft.Json with JObject parsing for flexible schema
- Creates default config from embedded resource if file doesn't exist
- Tilde (~) expansion for paths

### 3. AutoHotkey Syntax Parser

**Supported Syntax:**
- Modifiers: `^` (Ctrl), `+` (Shift), `!` (Alt), `#` (Win)
- Special keys: `{Enter}`, `{Tab}`, `{Space}`, `{F1}` through `{F12}`, arrow keys, etc.
- Plain text: Any alphanumeric character

**Examples:**
- `^v` → Ctrl+V (paste)
- `+{Enter}` → Shift+Enter
- `!{F4}` → Alt+F4 (close window)
- `#r` → Win+R (run dialog)

**Implementation:** Parses to list of KeyAction objects (ModifierDown → KeyPress → ModifierUp sequence)

### 4. PowerShell Execution System

**Architecture:** In-process execution using System.Management.Automation (NuGet package v7.4.7)

**Two Execution Modes:**

1. **Inline Commands** (`powershell` property):
   ```json
   {
     "action": {
       "powershell": "Get-Date -Format 'HH:mm:ss'"
     }
   }
   ```

2. **Script Files** (`powershellScript` property):
   ```json
   {
     "action": {
       "powershellScript": "~/scripts/toggle-mic.ps1",
       "powershellParameters": {
         "DeviceIndex": 0,
         "Verbose": true
       }
     }
   }
   ```

**Key Implementation Details:**
- **Service:** PowerShellService.cs creates isolated runspaces for each execution
- **Runspace Strategy:** Per-execution runspace creation (thread-safe, no state pollution)
- **Performance:** ~100-200ms per execution (acceptable for user-triggered actions)
- **No Console Windows:** Executes in-process, completely silent
- **Output Capture:** Captures Output stream + Error stream for dynamic titles
- **Path Expansion:** Supports ~ (tilde), %VAR% (environment variables), absolute and relative paths
- **Working Directory:** Set to UserProfile (matches CommandExecutionService)
- **Error Handling:** RuntimeException and HadErrors captured and displayed in tiles

**Dynamic Title Example (Microphone Status):**
```json
{
  "title": {
    "powershell": "Import-Module AudioDeviceCmdlets; $device = Get-AudioDevice -Recording; if ($device.Mute) { '{\"text\": \"MIC\\nMUTED\", \"theme\": \"prominent\"}' } else { 'MIC\\nACTIVE' }",
    "refresh": "1s"
  },
  "action": {
    "powershell": "Import-Module AudioDeviceCmdlets; $device = Get-AudioDevice -Recording; Set-AudioDevice -Recording -Mute (!$device.Mute)"
  }
}
```

**Important:** Requires PowerShell modules to be installed (e.g., `Install-Module AudioDeviceCmdlets -Scope CurrentUser`)

### 5. Logging System

**Log File Location:** `%LOCALAPPDATA%\MacroButtons\macrobuttons.log`

**What Gets Logged:**
- Application startup and profile loading
- Button presses with action type
- PowerShell command output and errors
- Exceptions with full stack traces
- Dynamic title update failures

**Features:**
- **Thread-safe**: Uses lock for concurrent write protection
- **Auto-rotation**: Rotates log when it exceeds 5 MB
- **Backup retention**: Keeps last 5 rotated log files
- **Structured format**: Timestamped entries with log levels (INFO, WARN, ERROR, BUTTON, STDOUT, STDERR)

**Accessing the Log:**
- Tray icon → Right-click → "View Log" (opens in Notepad)
- Manually navigate to: `%LOCALAPPDATA%\MacroButtons\macrobuttons.log`

**Log Entry Format:**
```
[2025-01-01 12:34:56.789] [INFO  ] MacroButtons Started
[2025-01-01 12:35:01.234] [BUTTON] Pressed: 'Mute Mic' (Action: PowerShell)
[2025-01-01 12:35:01.456] [STDERR] [PowerShell] Import-Module AudioDeviceCmdlets...
Output: Error: Module 'AudioDeviceCmdlets' not found
```

**Troubleshooting PowerShell Issues:**
1. Check log for PowerShell exceptions: `grep -i "powershell" macrobuttons.log`
2. Look for ERROR entries with full stack traces
3. Verify module installation: Open PowerShell and run `Get-Module -ListAvailable`

### 6. Grid Layout Algorithm

**Minimum:** 3 rows × 4 columns (12 tiles)

**Expansion Logic:**
```csharp
int totalTiles = Math.Max(itemCount, 12);
int columns = Math.Max(4, (int)Math.Ceiling(Math.Sqrt(totalTiles * 1.33))); // 4:3 ratio
int rows = Math.Max(3, (int)Math.Ceiling((double)totalTiles / columns));
```

**Examples:**
- 4 items → 3×4 grid (12 tiles, 8 empty)
- 15 items → 4×4 grid (16 tiles, 1 empty)
- 24 items → 4×6 grid (24 tiles, 0 empty)

**UI Implementation:** UniformGrid automatically sizes tiles to fill screen

## Development Commands

### Building on Windows

**Prerequisites:**
- .NET 8 SDK
- Visual Studio 2022 (for MSBuild and WPF designer)
- WiX Toolset 3.11+ (for installer)

**Build Commands:**
```bash
# Restore NuGet packages
dotnet restore

# Build application (Debug)
dotnet build MacroButtons/MacroButtons.csproj -c Debug

# Build application (Release)
dotnet build MacroButtons/MacroButtons.csproj -c Release

# Run application
dotnet run --project MacroButtons/MacroButtons.csproj

# Build installer (requires WiX Toolset and MSBuild)
msbuild MacroButtons.Installer/MacroButtons.Installer.wixproj /p:Configuration=Release /p:Platform=x64
```

**Output Locations:**
- Application: `MacroButtons/bin/Release/net8.0-windows/`
- Installer: `MacroButtons.Installer/bin/x64/Release/MacroButtons.msi`

### Testing on Windows

**Manual Testing Checklist:**
1. Window appears on correct monitor (edit `monitorIndex` in config)
2. Window fills entire screen
3. Window never steals focus (click tile, verify game/app stays focused)
4. Window occludes Start Menu when visible
5. Static titles display correctly
6. Dynamic titles refresh on interval
7. Keypress actions work (test `^v`, `+{Enter}`, etc.)
8. Python scripts execute silently (test with `print()` statement)
9. Executables launch without windows (test with `calc.exe`)
10. Theme colors apply correctly
11. Empty tiles display as boxes
12. Grid expands for more items

**Test Configuration:**
Create `%USERPROFILE%\.macrobuttons.json` with test items:
```json
{
  "items": [
    {
      "title": "Paste Test",
      "action": { "keypress": "^v" }
    },
    {
      "title": {
        "python": ["-c", "import datetime; print(datetime.datetime.now().strftime('%H:%M:%S'))"],
        "refresh": "1s"
      },
      "action": null
    },
    {
      "title": "Calculator",
      "action": { "exe": "calc.exe" }
    }
  ],
  "theme": {
    "foreground": "darkgreen",
    "background": "black"
  },
  "global": {
    "refresh": "30s",
    "monitorIndex": 0
  }
}
```

## Key File Locations

### Critical Application Files

**MainWindow.xaml.cs** (`MacroButtons/MainWindow.xaml.cs`)
- Non-activating window setup
- Multi-monitor positioning
- Window lifecycle management
- **Never modify:** WS_EX_NOACTIVATE flag, OnSourceInitialized behavior

**ConfigurationService.cs** (`MacroButtons/Services/ConfigurationService.cs`)
- JSON loading with polymorphic deserialization
- Default config creation from embedded resource
- **Important:** Handles both string and TitleDefinition for ButtonItem.Title

**KeystrokeService.cs** (`MacroButtons/Services/KeystrokeService.cs`)
- InputSimulatorPlus integration
- Previous window focus restoration
- **Important:** 50ms delay before sending keys ensures window is active

**AutoHotkeyParser.cs** (`MacroButtons/Helpers/AutoHotkeyParser.cs`)
- AutoHotkey syntax → VirtualKeyCode conversion
- **Extend here:** Add new special keys in ParseSpecialKey() method

**ButtonTileViewModel.cs** (`MacroButtons/ViewModels/ButtonTileViewModel.cs`)
- Action execution logic (keypress, python, exe)
- Dynamic title update logic with per-tile refresh timer
- Each dynamic tile manages its own DispatcherTimer (supports per-tile refresh intervals)
- **Error handling:** Displays error in tile if command fails
- **Important:** Implements IDisposable to stop refresh timer on cleanup

### Configuration Files

**MacroButtons.csproj** (`MacroButtons/MacroButtons.csproj`)
- NuGet package references (4 packages)
- Embedded resource for DefaultConfig.json
- **Important:** UseWindowsForms=true required for Screen.AllScreens

**DefaultConfig.json** (`MacroButtons/Resources/DefaultConfig.json`)
- Embedded default configuration
- Created in user profile if .macrobuttons.json missing
- **Modify this:** To change default tiles/theme

### Installer Files

**Product.wxs** (`MacroButtons.Installer/Product.wxs`)
- WiX installer definition
- Component GUIDs (NEVER change after first release)
- **UpgradeCode:** `{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}` - NEVER CHANGE
- **ProductVersion:** Updated by GitHub Actions during release

**MacroButtons.Installer.wixproj**
- References MacroButtons.csproj for file harvesting
- Builds x64 MSI installer
- **WiX Extensions:** WixUIExtension for installer UI

### CI/CD Files

**release.yml** (`.github/workflows/release.yml`)
- Triggered by tags matching `v*.*.*` or manual dispatch
- Updates Product.wxs version automatically
- Builds MSI and creates GitHub release
- **Version format:** Tag must be `v1.0.0` format (semantic versioning with 'v' prefix)

## Development Patterns

### Adding a New Button Action Type

1. **Update ActionDefinition.cs:**
   ```csharp
   public List<string>? NewActionType { get; set; }
   ```

2. **Add to ActionType enum:**
   ```csharp
   public enum ActionType { None, Keypress, Python, Executable, NewActionType }
   ```

3. **Update GetActionType() method:**
   ```csharp
   if (NewActionType != null && NewActionType.Count > 0) return ActionType.NewActionType;
   ```

4. **Implement in ButtonTileViewModel.ClickAsync():**
   ```csharp
   case ActionType.NewActionType:
       await _yourService.ExecuteAsync(...);
       break;
   ```

### Adding a New Special Key to Parser

Edit `AutoHotkeyParser.cs` → `ParseSpecialKey()`:
```csharp
"newkey" => VirtualKeyCode.YOUR_KEY,
```

### Changing Grid Layout Algorithm

Edit `MainViewModel.cs` → `CalculateGridLayout()`:
```csharp
// Example: Change to 5×5 minimum
const int MIN_ROWS = 5;
const int MIN_COLS = 5;
```

### Adding a New Service

1. Create in `Services/` directory
2. Inject into `MainViewModel` constructor
3. Pass to `ButtonTileViewModel` constructors if needed
4. Follow dependency injection pattern (constructor parameters)

### Adding a New Sample Profile to the Installer

When creating a new sample profile (e.g., Excel, Word, Photoshop), you must update the WiX installer to include it. This involves changes to two installer files.

**Step 1: Create the sample files**

Create a new directory under `samples/` with the application name:
```bash
samples/
├── YourApp/
│   ├── your-app.json      # The profile configuration
│   └── README.md           # Documentation for the sample
```

**Step 2: Update Product.wxs**

File: `MacroButtons.Installer/Product.wxs`

Add two entries:

1. **Add a Feature definition** (in the `SamplesFeature` section, around line 96-101):
   ```xml
   <Feature Id="Sample_YourApp_Feature" Title="Your App Name" Level="1000"
            Description="Macro buttons for Your Application description" AllowAdvertise="no">
     <ComponentRef Id="Sample_YourApp_Registry" />
   </Feature>
   ```

   **Notes:**
   - `Level="1000"` means optional (not selected by default)
   - `Level="1"` means selected by default (use sparingly, only for truly universal samples)
   - The Feature Id must match the pattern `Sample_{Name}_Feature`

2. **Add a Registry component** (in the DirectoryRef section, around line 258-260):
   ```xml
   <Component Id="Sample_YourApp_Registry" Guid="{NEW-GUID-HERE}">
     <RegistryValue Root="HKCU" Key="Software\MacroButtons\PendingSamples" Name="YourApp" Type="string" Value="1" KeyPath="yes" />
   </Component>
   ```

   **Generate a new GUID** using PowerShell:
   ```powershell
   [Guid]::NewGuid()
   ```

**Step 3: Update Samples.wxs**

File: `MacroButtons.Installer/Samples.wxs`

Add three sections:

1. **Add directory definition** (DirectoryRef section, around line 6-19):
   ```xml
   <Directory Id="dir_YourApp" Name="YourApp" />
   ```

   **Note:** Keep directories in alphabetical order for maintainability.

2. **Add Program Files components** (AllSamplesInProgramFiles section, around line 32-142):
   ```xml
   <!-- Your App -->
   <Component Id="cmp_PF_YourApp_json" Directory="dir_YourApp" Guid="{NEW-GUID-1}">
     <File Id="fil_PF_YourApp_json" Source="$(var.SamplesSourceDir)\YourApp\your-app.json" KeyPath="yes" />
   </Component>
   <Component Id="cmp_PF_YourApp_README" Directory="dir_YourApp" Guid="{NEW-GUID-2}">
     <File Id="fil_PF_YourApp_README" Source="$(var.SamplesSourceDir)\YourApp\README.md" KeyPath="yes" />
   </Component>
   ```

   **If your sample includes additional files** (like scripts):
   ```xml
   <Component Id="cmp_PF_YourApp_script" Directory="dir_YourApp" Guid="{NEW-GUID-3}">
     <File Id="fil_PF_YourApp_script" Source="$(var.SamplesSourceDir)\YourApp\your-script.py" KeyPath="yes" />
   </Component>
   ```

3. **Add User Profile component** (after WindowsTerminal section, around line 250-258):
   ```xml
   <!-- Your App User Profile -->
   <Fragment>
     <ComponentGroup Id="Sample_YourApp_UserProfile">
       <Component Id="cmp_UP_YourApp_json" Directory="USERPROFILEFOLDER" Guid="{NEW-GUID-4}">
         <File Id="fil_UP_YourApp_json" Name=".macrobuttons-your-app.json" Source="$(var.SamplesSourceDir)\YourApp\your-app.json" />
         <RegistryValue Root="HKCU" Key="Software\MacroButtons\Samples" Name="YourApp" Type="integer" Value="1" KeyPath="yes" />
       </Component>
     </ComponentGroup>
   </Fragment>
   ```

**Important GUID Requirements:**
- **Every GUID must be unique** across the entire installer
- Generate a new GUID for each Component using `[Guid]::NewGuid()` in PowerShell
- **NEVER reuse GUIDs** - this will cause installer conflicts
- **NEVER change GUIDs after release** - this breaks upgrades

**Naming Conventions:**
- Feature Id: `Sample_{Name}_Feature`
- Registry Component Id: `Sample_{Name}_Registry`
- Directory Id: `dir_{Name}`
- Program Files Component Id: `cmp_PF_{Name}_{filetype}`
- User Profile Component Id: `cmp_UP_{Name}_{filetype}`
- File Id: `fil_PF_{Name}_{filetype}` or `fil_UP_{Name}_{filetype}`
- Registry Name: Use the sample name without "Sample_" prefix

**Testing the Installer:**

After making changes, build and test the installer:

```bash
# Build the application first
dotnet build MacroButtons/MacroButtons.csproj -c Release

# Build the installer
msbuild MacroButtons.Installer/MacroButtons.Installer.wixproj /p:Configuration=Release /p:Platform=x64

# Test the MSI
# Install: MacroButtons.Installer/bin/x64/Release/MacroButtons.msi
# Verify your sample appears in the installer feature tree
# Verify files are installed to C:\Program Files\MacroButtons\samples\YourApp\
```

**Example: Adding Excel Sample**

See the Excel sample additions in Product.wxs (lines 101-104, 261-263) and Samples.wxs (lines 9, 54-60, 260-268) for a complete working example.

## Common Issues and Solutions

### Issue: Window Steals Focus

**Symptom:** Clicking buttons causes game/app to lose focus

**Solution:** Verify WS_EX_NOACTIVATE is set in MainWindow.OnSourceInitialized(). Check that Topmost is set correctly.

### Issue: Keystrokes Not Working

**Symptom:** Clicking keypress button does nothing

**Potential Causes:**
1. Previous window not stored (check OnActivated)
2. Invalid AutoHotkey syntax (check parser)
3. Insufficient delay before sending keys (increase delay in KeystrokeService)

**Debug:** Add logging to KeystrokeService.SendKeysAsync()

### Issue: Dynamic Titles Not Refreshing

**Symptom:** Command-based titles show "Loading..." forever

**Potential Causes:**
1. Command execution failing (check CommandExecutionService error handling)
2. Refresh service not started (verify StartDynamicTitleRefresh called)
3. Invalid refresh interval format (check GlobalConfig.GetRefreshInterval)

**Debug:** Check if IsDynamic is true for tiles

### Issue: Python Scripts Show Console Window

**Symptom:** Console window flashes when executing Python

**Solution:** Verify ProcessStartInfo has:
```csharp
CreateNoWindow = true
WindowStyle = ProcessWindowStyle.Hidden
```

Consider using `pythonw.exe` instead of `python.exe` for GUI-less execution.

### Issue: Installer Build Fails

**Potential Causes:**
1. WiX Toolset not installed
2. Application not built before installer
3. Missing DLL references in Product.wxs

**Solution:**
```bash
# Build app first
dotnet build MacroButtons/MacroButtons.csproj -c Release

# Then build installer
msbuild MacroButtons.Installer/MacroButtons.Installer.wixproj /p:Configuration=Release /p:Platform=x64
```

## Extending the Application

### Adding Multi-Language Support

1. Create resource files (.resx) in `Resources/`
2. Update UI strings to use resource references
3. Add language selection to GlobalConfig
4. Apply culture in App.xaml.cs OnStartup

### Adding Custom Themes

Current theme only supports foreground/background colors. To extend:

1. Add to ThemeConfig.cs:
   ```csharp
   public string HoverColor { get; set; }
   public string BorderColor { get; set; }
   public int FontSize { get; set; }
   ```

2. Update ButtonTile.xaml styling
3. Apply in ButtonTileViewModel constructor

### Adding Sound Effects

1. Create SoundService.cs in Services/
2. Add sound file paths to ActionDefinition or GlobalConfig
3. Play sound in ButtonTileViewModel.ClickAsync() before action execution

### Adding Tile Animations

1. Add Storyboard resources to ButtonTile.xaml
2. Trigger animations on button press/hover
3. Configure animation duration in theme

## Release Process

### Creating a New Release

1. **Update version** (optional, auto-handled by tag):
   ```bash
   # Version comes from Git tag
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **GitHub Actions automatically:**
   - Builds .NET application
   - Updates Product.wxs version
   - Builds WiX installer
   - Creates MSI artifact
   - Creates GitHub release
   - Uploads MSI to release

3. **Manual release** (if needed):
   - Go to Actions → Release → Run workflow
   - Enter version number (e.g., "1.0.0" without 'v')

### Version Numbering

- **Format:** `v{major}.{minor}.{patch}` (semantic versioning)
- **Example:** `v1.2.3`
- **Major:** Breaking changes to config schema or API
- **Minor:** New features, backward-compatible
- **Patch:** Bug fixes only

### Pre-Release Testing

Before tagging a release:
1. Test on clean Windows 10/11 machine
2. Verify all features in testing checklist
3. Test installer installation/uninstallation
4. Verify .macrobuttons.json creation
5. Test multi-monitor support
6. Verify non-activating window behavior

## Dependencies

### NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| Newtonsoft.Json | 13.0.3 | JSON serialization/deserialization |
| InputSimulatorPlus | 1.0.7 | Keystroke simulation (pure C#) |
| CommunityToolkit.Mvvm | 8.4.0 | MVVM helpers (ObservableProperty, RelayCommand) |
| System.Drawing.Common | 8.0.0 | Multi-monitor support (Screen.AllScreens) |

**Important:** InputSimulatorPlus is critical - do not replace without testing all keystroke functionality.

### Build Dependencies

- **.NET 8 SDK:** Required for compilation
- **WiX Toolset 3.11+:** Required for MSI installer creation
- **Visual Studio 2022:** Recommended for development (includes MSBuild)

### WiX Installer Dependency Management

**IMPORTANT:** When adding a new NuGet package, you MUST update `MacroButtons.Installer/Product.wxs` to include the DLL in the installer.

**Current NuGet Dependencies in WiX:**
- `Newtonsoft.Json.dll` (Component: NewtonsoftJson)
- `WindowsInput.dll` (Component: InputSimulatorPlus)
- `CommunityToolkit.Mvvm.dll` (Component: CommunityToolkitMvvm)
- `System.Management.Automation.dll` (Component: SystemManagementAutomation)
- `Microsoft.Management.Infrastructure.dll` (Component: MicrosoftManagementInfrastructure)
- `Microsoft.PowerShell.CoreCLR.Eventing.dll` (Component: MicrosoftPowerShellCorePS)

**Steps to add a new NuGet dependency:**
1. Add `<PackageReference>` to `MacroButtons.csproj`
2. Add corresponding `<Component>` to `Product.wxs` in the `ProductComponents` group
3. Generate a new GUID for the Component (use `[Guid]::NewGuid()` in PowerShell)
4. Reference the DLL from `$(var.MacroButtons.TargetDir)DllName.dll`
5. Test the installer build locally before committing

**Example:**
```xml
<Component Id="MyNewPackage" Guid="{NEW-GUID-HERE}">
  <File Id="MyNewPackageDll"
        Source="$(var.MacroButtons.TargetDir)MyPackage.dll"
        KeyPath="yes" />
</Component>
```

**Note:** The GitHub Actions workflow automatically restores NuGet packages, but the WiX installer must be manually configured to include each DLL.

## Security Considerations

### Configuration File

- Stored in user profile (`%USERPROFILE%\.macrobuttons.json`)
- No sensitive data should be stored in configuration
- Python/exe paths are executed with user privileges
- **Warning:** Users can execute arbitrary commands - this is by design

### Keystroke Simulation

- Uses Win32 SendInput API (via InputSimulatorPlus)
- Keystrokes sent to foreground window (previously active)
- No keylogging or input capture
- **Privacy:** Application does not record or transmit user input

### Code Execution

- Python scripts and executables run with user privileges
- No sandboxing or privilege elevation
- **User Responsibility:** Users must trust their own configured commands

## Performance Considerations

### Dynamic Title Refresh

- Default: 30 seconds global refresh interval
- Each dynamic tile manages its own DispatcherTimer (runs on UI thread)
- Tiles can override global refresh with per-tile `refresh` setting (minimum 100ms)
- **Recommendation:** Don't set fast refresh intervals (< 1s) for many tiles to avoid excessive CPU usage

### Memory Usage

- Minimal memory footprint (~20-30 MB typical)
- Each dynamic tile has its own timer (stopped and disposed on exit)
- **Monitor:** ButtonTileViewModel.Dispose() stops timers when MainViewModel is disposed

### Startup Time

- Fast startup (~500ms on modern hardware)
- Configuration loaded synchronously (intentional - must complete before UI)
- **Optimization opportunity:** Async config loading with loading screen

## Troubleshooting

### Application Won't Start

1. Check .NET 8 Desktop Runtime installed
2. Verify no other instance running (single instance check)
3. Check for malformed .macrobuttons.json (app creates backup)
4. Run from command line to see error messages

### Configuration Issues

**Config not loading:**
```bash
# Check if file exists
ls %USERPROFILE%\.macrobuttons.json

# Validate JSON syntax
type %USERPROFILE%\.macrobuttons.json | python -m json.tool
```

**Config reset:**
```bash
# Rename current config
ren %USERPROFILE%\.macrobuttons.json .macrobuttons.json.backup

# Restart app to create new default config
```

### Build Issues

**Missing NuGet packages:**
```bash
dotnet restore --force
```

**WiX not found:**
```bash
# Install via Chocolatey
choco install wixtoolset --version=3.11.2

# Or download from https://wixtoolset.org/
```

## Future Enhancement Ideas

### Completed Features
✅ Non-activating window
✅ AutoHotkey keystroke simulation
✅ Python/executable execution
✅ In-process PowerShell execution (System.Management.Automation)
✅ Comprehensive logging system
✅ Dynamic title refresh
✅ Multi-monitor support
✅ Retro terminal theme

### Potential Enhancements
- 🔲 Profile switching (multiple .macrobuttons-{profile}.json files)
- 🔲 Tile icons/images in addition to text
- 🔲 Keyboard shortcuts to trigger tiles (global hotkeys)
- 🔲 Mouse gesture support
- 🔲 Tile grouping/categories
- 🔲 Conditional visibility (show/hide tiles based on active window)
- 🔲 Network monitoring tiles (ping, bandwidth)
- 🔲 System resource tiles (CPU, RAM, GPU usage)
- 🔲 Web API integration tiles (weather, stocks, etc.)
- 🔲 Tile scripting (Lua/Python for tile logic)
- 🔲 Touch gesture support (swipe, long-press)
- 🔲 Audio visualization tiles
- 🔲 Screen recording controls
- 🔲 OBS integration (scene switching)
- 🔲 Discord Rich Presence integration

## Contact and Contribution

This project was created as a custom tool for macro button workflows. Future development depends on user needs and feedback.

**Development Philosophy:**
- Keep it simple (no over-engineering)
- Performance matters (gaming/creation workflows)
- User configuration over code changes
- Backward compatibility for config files

---

**Note:** This CLAUDE.md is designed to help Claude Code understand and work with this codebase. When making changes, ensure this file stays updated with architectural decisions and patterns.
