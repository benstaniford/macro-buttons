# MacroButtons

A WPF/C# application for creating a touch-screen macro button panel with a retro terminal aesthetic.

## Features

- Full-screen non-activating window that never steals focus
- Grid of customizable tiles for macros and information display
- Support for keyboard shortcuts (AutoHotkey syntax)
- Execute Python scripts and executables
- Dynamic information tiles with periodic refresh
- Multi-monitor support
- Retro terminal aesthetic with configurable themes

## Configuration

Edit `%USERPROFILE%\.macrobuttons.json` to customize your macro buttons.

## Building

Requires:
- .NET 8 SDK
- Visual Studio 2022 (for Windows builds)
- WiX Toolset 3.11+ (for installer)

```bash
dotnet restore
dotnet build -c Release
```

## License

MIT License
