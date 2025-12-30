# File Explorer MacroButtons Profile

Quick access to Windows File Explorer keyboard shortcuts for efficient file and folder management.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/FileExplorer/file-explorer.json`
3. Use with File Explorer windows for rapid file operations

## Shortcuts Included

### Window Management
- **New Window** (Ctrl+N) - Open a new File Explorer window
- **Close Window** (Ctrl+W) - Close current File Explorer window
- **Full Screen** (F11) - Toggle full screen mode

### Navigation
- **Back** (Alt+Left) - Go back in navigation history
- **Forward** (Alt+Right) - Go forward in navigation history
- **Up One Level** (Alt+Up) - Navigate to parent folder
- **Address Bar** (Alt+D) - Focus address bar for typing path
- **Search** (Ctrl+F) - Focus search box
- **Refresh** (F5) - Refresh current folder view

### File Operations
- **New Folder** (Ctrl+Shift+N) - Create a new folder
- **Rename** (F2) - Rename selected file/folder
- **Copy** (Ctrl+C) - Copy selected items
- **Cut** (Ctrl+X) - Cut selected items
- **Paste** (Ctrl+V) - Paste copied/cut items
- **Undo** (Ctrl+Z) - Undo last operation
- **Properties** (Alt+Enter) - View properties of selected item

### Selection
- **Select All** (Ctrl+A) - Select all items in current folder
- **Invert Selection** (Ctrl+Shift+A) - Invert current selection

### Delete Operations
- **Delete** (Delete) - Move to Recycle Bin
- **Permanent Delete** (Shift+Delete) - Delete permanently (bypasses Recycle Bin)

### View Controls
- **Details View** (Ctrl+Shift+6) - Switch to details view
- **Large Icons** (Ctrl+Shift+2) - Switch to large icons view
- **Preview Pane** (Alt+P) - Toggle preview pane

### Information Display
- **Clock** - Shows current time (updates every second)

## Theme Colors

- **Default** - Cyan text on black background (general actions)
- **Prominent** - Black text on cyan background (clock display)
- **Navigation** - Light blue text on dark teal (navigation shortcuts)
- **File Operations** - Light green text on dark green (copy/paste/create)
- **Danger** - Red text on dark red (delete operations)

## Auto-Activation

This profile automatically activates when a File Explorer window is active using:
- **Process Name:** `explorer.exe`
- **Window Class:** `CabinetWClass`

The `activeWindow` setting is configured as `"explorer|CabinetWClass"`, which ensures this profile only activates for actual File Explorer windows (not the Desktop or other explorer.exe windows).

**Note:** While both the Windows Desktop and File Explorer profiles use the same `explorer.exe` process, the window class distinction allows MacroButtons to automatically switch between them based on whether you're in a File Explorer window or using general Windows shortcuts.

## Customization

Edit the JSON file to:
- Add more File Explorer shortcuts
- Change view mode shortcuts (List, Tiles, Content, etc.)
- Add quick navigation to favorite folders
- Customize theme colors for different action types

## Additional Shortcuts You Can Add

Here are other useful File Explorer shortcuts not included in this profile:

### View Modes
- `Ctrl+Shift+1` - Extra Large Icons
- `Ctrl+Shift+3` - Medium Icons
- `Ctrl+Shift+4` - Small Icons
- `Ctrl+Shift+5` - List View
- `Ctrl+Shift+7` - Tiles View
- `Ctrl+Shift+8` - Content View

### Navigation Panes
- `Ctrl+Shift+E` - Show all folders (expand navigation pane)
- `Ctrl+L` - Focus address bar (alternative to Alt+D)

### Quick Access
- `Ctrl+E` - Focus search box (alternative to Ctrl+F)
- `F3` - Focus search box (alternative)

Example addition:
```json
{
  "title": "List View\n(Ctrl+Shift+5)",
  "action": { "keypress": "^+5" }
}
```

## Tips

- Color-coded themes help identify action types at a glance
- Dangerous operations (delete, permanent delete) are highlighted in red
- File operations (copy, cut, paste, new folder) use green theme
- Navigation shortcuts use blue theme for quick identification
- Perfect for touch-screen setups or when working with large file sets
