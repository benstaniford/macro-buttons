# MacroButtons

A touch-screen macro button panel with a retro terminal aesthetic. Perfect for streamers, flight sim fans, creators, and anyone who wants quick access to macros and system information on a secondary touch screen display.

![MacroButtons Screenshot](https://via.placeholder.com/800x450?text=MacroButtons+Screenshot)

## What is MacroButtons?

MacroButtons turns a secondary touch screen monitor in Windows into a customizable grid of buttons that can:

- **Execute keyboard shortcuts** - Paste, save, copy, or any key combination using AutoHotkey syntax
- **Run scripts and programs** - Launch Python scripts, executables, or built-in commands
- **Display live information** - Show system stats, game status, or any data that updates in real-time
- **Never steal focus** - Click buttons without interrupting your work, game, or stream

The window fills your screen with a grid of tiles, each customizable with different colors and actions. It's designed to feel like a retro terminal interface while being completely modern under the hood.

## Installation

### Windows

1. Download the latest `MacroButtons.msi` installer from the [Releases](https://github.com/benstaniford/macro-buttons/releases) page
2. Run the installer
3. MacroButtons will start automatically and appear in your system tray

### First Run

On first launch, MacroButtons creates a default configuration at:
```
%USERPROFILE%\.macrobuttons\profiles\default.json
```

The default profile includes sample buttons to get you started:
- Copy (Ctrl+C)
- Paste (Ctrl+V)
- Calculator
- Clock (updates every second)
- Quit button

## Using MacroButtons

### System Tray Menu

MacroButtons runs in the background. Right-click the tray icon to access:

- **Profiles** - Switch between different button layouts, create new profiles, import/export
- **Edit Config** - Opens your current profile configuration in your default JSON editor
- **Reload** - Refresh the display after editing your config
- **Quit** - Exit the application

### Profiles

Profiles let you have different button layouts for different tasks:

- **Create New Profile** - Start fresh or copy your current setup
- **Import Profile** - Load a profile from a file (opens to samples folder by default)
- **Rename Profile** - Change the name of your current profile
- **Delete Profile** - Remove a profile (you must have at least one)

### Automatic Profile Switching

Profiles can automatically switch based on the active window. For example, have a "Gaming" profile that appears when you launch a game, and automatically switch back when you close it.

Configure this in your profile's JSON under `global.activeWindow` (see [Configuration Guide](ConfigGuide.md)).

## Sample Profiles

MacroButtons ships with sample profiles to help you get started. After installation, you can find them at:

```
C:\Program Files\MacroButtons\samples\
```

### Available Samples

#### Elite Dangerous
Located in: `samples/EliteDangerous/`

Integration with Elite Dangerous space simulation game. Shows dynamic tiles for:
- Landing gear status (green when raised, orange when lowered)
- Cargo scoop status (green when retracted, orange when deployed)

**To try it:**
1. Right-click tray icon → Profiles → Import Profile...
2. Browse to `samples/EliteDangerous/elitedangerous.json`
3. Follow the README in that folder for setup instructions

More samples coming soon! You can also [contribute your own](#contributing).

## Configuration

MacroButtons uses JSON configuration files. The simplest way to edit your config is:

1. Right-click tray icon → **Edit Config**
2. Make your changes in the JSON editor
3. Right-click tray icon → **Reload**

For detailed configuration documentation, see the [Configuration Guide](ConfigGuide.md).

### Quick Example

Here's a simple button that opens your browser:

```json
{
  "items": [
    {
      "title": "Open Browser",
      "action": {
        "exe": "C:\\Program Files\\Mozilla Firefox\\firefox.exe"
      }
    }
  ]
}
```

See the [Configuration Guide](ConfigGuide.md) for complete documentation on:
- Button actions (keyboard shortcuts, scripts, executables)
- Dynamic tiles (updating information)
- Themes (customizing colors)
- Advanced features

## Multi-Monitor Setup

By default, MacroButtons appears on your smallest monitor. To change this:

1. Edit your profile config
2. Find the `"monitorIndex"` setting under `"global"`
3. Set it to the monitor number (0 = first monitor, 1 = second, etc.)
4. Reload

## Keyboard Shortcuts

MacroButtons supports AutoHotkey-style keyboard shortcut syntax:

- `^` = Ctrl
- `+` = Shift
- `!` = Alt
- `#` = Windows key

**Examples:**
- `^c` = Ctrl+C (copy)
- `^+s` = Ctrl+Shift+S (save as)
- `!{F4}` = Alt+F4 (close window)
- `{Enter}` = Enter key

See the [Configuration Guide](ConfigGuide.md) for the complete list of special keys.

## Troubleshooting

### Window not appearing
- Check that you have multiple monitors
- Try setting `monitorIndex` to 0 in your config
- Ensure MacroButtons is running (check system tray)

### Buttons not working
- Verify your JSON syntax is valid
- Check the reload menu option after making changes
- Look for errors in your script paths

### Focus issues
- The window is designed to never steal focus
- If a game or app is blocking clicks, try running MacroButtons as administrator

## Contributing

We welcome contributions! Whether it's:

- **Sample profiles** for popular games and applications
- **Bug reports** and feature requests
- **Code improvements** and new features

### Sharing Sample Profiles

Created a useful MacroButtons profile? Share it!

1. Export your profile (right-click → Profiles → your profile → copy the JSON file)
2. Create a folder in `samples/` with your profile and README
3. Submit a pull request

### Reporting Issues

Found a bug? [Open an issue](https://github.com/yourusername/macro-buttons/issues) with:
- What you expected to happen
- What actually happened
- Your configuration file (if relevant)
- Windows version

## License

GNU General Public License v3.0 - see [LICENSE](LICENSE) file for details.

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

---

**Made with ❤️ for streamers, creators, and power users**
