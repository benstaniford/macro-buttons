# Windows Desktop MacroButtons Profile

Quick access to essential Windows keyboard shortcuts and system utilities.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/WindowsDesktop/windows-desktop.json`
3. The profile provides instant access to common Windows functions

## Shortcuts Included

### System Access
- **Settings** (Win+I) - Open Windows Settings
- **File Explorer** (Win+E) - Open File Explorer
- **Run Dialog** (Win+R) - Open Run command dialog

### Window & Desktop Management
- **Show Desktop** (Win+D) - Minimize all windows
- **Task View** (Win+Tab) - View all open windows and desktops
- **Lock Computer** (Win+L) - Lock your PC
- **New Virtual Desktop** (Win+Ctrl+D) - Create a new virtual desktop
- **Previous Desktop** (Win+Ctrl+Left) - Switch to previous virtual desktop
- **Next Desktop** (Win+Ctrl+Right) - Switch to next virtual desktop
- **Close Desktop** (Win+Ctrl+F4) - Close current virtual desktop

### Quick Actions
- **Screenshot** (Win+Shift+S) - Snipping tool for screenshots
- **Clipboard History** (Win+V) - Access clipboard history
- **Emoji Picker** (Win+.) - Insert emojis and symbols
- **Voice Typing** (Win+H) - Start voice dictation
- **Quick Settings** (Win+A) - Access Wi-Fi, Bluetooth, etc.
- **Connect/Cast** (Win+K) - Connect to wireless displays or audio

### System Utilities
- **Task Manager** (Ctrl+Shift+Esc) - Monitor system performance
- **Device Manager** - Manage hardware devices
- **Control Panel** - Classic Windows control panel
- **Calculator** - Launch Windows calculator
- **Magnifier** (Win++) - Zoom in on screen content

### Information Display
- **Clock** - Shows current time (updates every second)

## Theme Colors

- **Default** - Cyan text on black background (most shortcuts)
- **Prominent** - Black text on cyan background (clock display)
- **Utility** - Yellow text on dark teal background (system utilities)

## Customization

Edit the JSON file to:
- Add more Windows shortcuts or utilities
- Rearrange button layout
- Change theme colors
- Add custom Run commands (e.g., `regedit.exe`, `services.msc`, etc.)

## Notes

- Each button displays the keyboard shortcut for easy reference
- System utilities (Device Manager, Control Panel, Calculator) can be launched directly
- Perfect for touch-screen setups or quick access panels
- Virtual Desktop shortcuts help manage multiple workspaces efficiently

## Additional Utilities You Can Add

Here are some other useful Windows utilities you can add to the profile:

- `msconfig.exe` - System Configuration
- `regedit.exe` - Registry Editor
- `services.msc` - Windows Services
- `diskmgmt.msc` - Disk Management
- `cleanmgr.exe` - Disk Cleanup
- `perfmon.exe` - Performance Monitor
- `compmgmt.msc` - Computer Management
- `eventvwr.msc` - Event Viewer
- `resmon.exe` - Resource Monitor
- `mstsc.exe` - Remote Desktop Connection

Simply add them using the `exe` action format:
```json
{
  "title": "Registry Editor",
  "action": { "exe": "regedit.exe" },
  "theme": "utility"
}
```
