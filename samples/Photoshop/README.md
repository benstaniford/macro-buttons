# Photoshop Sample Profile

This sample profile demonstrates how to integrate MacroButtons with Adobe Photoshop for quick access to common editing shortcuts.

## Files

- **photoshop.json** - MacroButtons profile configuration for Adobe Photoshop

## Features

This profile provides 20 macro buttons for frequently-used Photoshop operations:

### File Operations
- **NEW DOCUMENT** - Ctrl+N
- **OPEN** - Ctrl+O
- **SAVE** - Ctrl+S
- **SAVE AS** - Ctrl+Shift+S

### Layer Management
- **NEW LAYER** - Ctrl+Shift+N
- **DUPLICATE LAYER** - Ctrl+J
- **MERGE DOWN** - Ctrl+E
- **MERGE VISIBLE** - Ctrl+Shift+E

### Tools
- **MOVE TOOL** - V
- **BRUSH TOOL** - B
- **ERASER TOOL** - E
- **LASSO TOOL** - L

### Edit Operations
- **FREE TRANSFORM** - Ctrl+T
- **UNDO** - Ctrl+Z
- **STEP BACKWARD** - Ctrl+Alt+Z
- **REDO** - Ctrl+Shift+Z

### Selection & View
- **DESELECT** - Ctrl+D
- **INVERSE SELECT** - Ctrl+Shift+I
- **TOGGLE PANELS** - Tab

### Information Display
- **CLOCK** - Shows current time with 1-second refresh

## Installation

1. **Import the Profile:**
   - Right-click the MacroButtons tray icon
   - Select "Profiles" → "Import Profile..."
   - Choose `photoshop.json`
   - Name the profile (e.g., "Photoshop")

2. **Configure Monitor:**
   - The profile is configured for monitor index 1 (secondary monitor)
   - Edit `monitorIndex` in the JSON file to change which monitor displays the buttons:
     - `0` = Primary monitor
     - `1` = Secondary monitor
     - `2+` = Additional monitors

3. **Verify Photoshop Process Name:**
   - The profile activates when `Photoshop.exe` is running
   - If your Photoshop uses a different process name, update `activewindow` in the JSON

## How It Works

MacroButtons uses keystroke simulation to send keyboard shortcuts to Photoshop:
- When you click a button, MacroButtons sends the configured keystroke to the currently active window
- The window remains non-activating, so your Photoshop workspace keeps focus
- Shortcuts are sent using AutoHotkey syntax:
  - `^` = Ctrl
  - `+` = Shift
  - `!` = Alt
  - `#` = Win

## Customization

You can easily customize this profile to add your own favorite shortcuts:

### Adding a New Button

Edit `photoshop.json` and add a new item to the `items` array:

```json
{
  "action": {
    "keypress": "^!+s"
  },
  "title": "SAVE FOR WEB"
}
```

### Common Shortcuts You Might Add

- **Ctrl+Alt+Shift+S** - Save for Web
- **Ctrl+I** - Invert
- **Ctrl+L** - Levels
- **Ctrl+M** - Curves
- **Ctrl+U** - Hue/Saturation
- **Ctrl+B** - Color Balance
- **F** - Toggle screen modes
- **W** - Magic Wand tool
- **G** - Gradient/Paint Bucket tool
- **Ctrl+[** - Send layer backward
- **Ctrl+]** - Bring layer forward

### Changing the Theme

The profile uses an orange/dark blue color scheme. You can customize colors by editing the `themes` section:

```json
"themes": [
  {
    "name": "default",
    "foreground": "#FF6B35",  // Orange text
    "background": "#1A1A2E"   // Dark blue/black background
  }
]
```

Supported color formats:
- Named colors: `"red"`, `"green"`, `"blue"`, etc.
- Hex colors: `"#FF6B35"`, `"#1A1A2E"`, etc.

## Tips

- Position your MacroButtons window on a secondary monitor or tablet for easy access while working
- The non-activating window ensures Photoshop stays in focus when you click buttons
- Organize shortcuts by workflow (editing, layers, tools) to build muscle memory
- Use the prominent theme for important buttons like the clock to make them stand out

## Compatibility

- Works with all modern versions of Adobe Photoshop (CS6, CC 2015+)
- Tested with Photoshop 2024 on Windows 10/11
- Keyboard shortcuts may vary slightly between Photoshop versions - verify your shortcuts match
