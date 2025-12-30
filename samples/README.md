# MacroButtons Sample Profiles

This directory contains sample profiles that demonstrate various features of MacroButtons.

## Available Samples

### ComfyUI
**Location:** `ComfyUI/`

Keyboard shortcuts for ComfyUI AI image generation interface.

### Elite Dangerous
**Location:** `EliteDangerous/`

Integration with Elite Dangerous space simulation game. Shows dynamic status tiles for landing gear and cargo scoop state.

**Key Features:**
- Dynamic tile updates from game status file
- Theme-based color switching (default/toggled)
- Python script integration

### File Explorer
**Location:** `FileExplorer/`

Quick access to Windows File Explorer keyboard shortcuts for efficient file and folder management.

**Key Features:**
- File operations (Copy, Cut, Paste, Delete, Rename, New Folder)
- Navigation shortcuts (Back, Forward, Up One Level, Address Bar)
- View controls (Details, Large Icons, Preview Pane)
- Selection tools (Select All, Invert Selection)
- Color-coded themes (Navigation, File Operations, Danger zones)
- Auto-activates when File Explorer window is active (uses `explorer|CabinetWClass` to distinguish from Desktop)

### Firefox
**Location:** `Firefox/`

Common Firefox browser keyboard shortcuts optimized for touch screen control with Firefox's signature orange theme.

**Key Features:**
- Tab management (Reopen Closed, Next/Previous Tab)
- Navigation (Address Bar, Search, Back/Forward)
- Bookmarks and history access
- Developer tools shortcuts
- Auto-activates when Firefox is the active window

### Lightroom
**Location:** `Lightroom/`

Quick access macro buttons for Adobe Lightroom photo editing workflows.

### Photoshop
**Location:** `Photoshop/`

Quick access macro buttons for Adobe Photoshop with 20 common keyboard shortcuts for file operations, layer management, tools, and editing.

**Key Features:**
- File operations (New, Open, Save, Save As)
- Layer management (New, Duplicate, Merge)
- Common tools (Move, Brush, Eraser, Lasso)
- Edit operations (Transform, Undo/Redo)
- Selection shortcuts
- Live clock display

### Visual Studio
**Location:** `VisualStudio/`

Essential Visual Studio IDE keyboard shortcuts for touch screen control.

**Key Features:**
- Comprehensive debugging shortcuts (F5, F9, F10, F11, Step Into/Over/Out)
- Navigation (Go to Definition, Find References, Go to All)
- Code editing (Format Document, Comment/Uncomment)
- Refactoring tools (Rename Symbol, Extract Method)
- Auto-activates when Visual Studio (devenv) is the active window

### VS Code
**Location:** `VSCode/`

Essential VS Code keyboard shortcuts for touch screen control.

**Key Features:**
- Navigation (Command Palette, Go to Symbol, Go to Line)
- View management (Toggle Terminal, Sidebar, Split Editor)
- Code editing (Delete Line, Move Line, Format Document)
- Multi-cursor editing and debugging shortcuts
- Auto-activates when VS Code is the active window

### Windows Calculator
**Location:** `WindowsCalculator/`

Keyboard shortcuts for Windows Calculator application.

### Windows Desktop
**Location:** `WindowsDesktop/`

Quick access to essential Windows keyboard shortcuts and system utilities.

**Key Features:**
- System access (Settings, File Explorer, Run Dialog)
- Window management (Show Desktop, Task View, Virtual Desktops)
- Quick actions (Screenshot, Clipboard History, Emoji Picker)
- System utilities (Task Manager, Device Manager, Control Panel)
- Live clock display

### Windows Terminal
**Location:** `WindowsTerminal/`

Keyboard shortcuts for Windows Terminal application.

## Using Samples

To use any sample profile:

1. **Import via Tray Menu:**
   - Right-click the MacroButtons tray icon
   - Select "Profiles" → "Import Profile..."
   - Browse to the sample folder
   - Select the `.json` profile file

2. **Review the README:**
   - Each sample has its own README with specific setup instructions
   - Pay attention to script paths and dependencies

3. **Customize:**
   - After importing, feel free to customize the profile for your needs
   - Right-click tray icon → "Edit Config" to modify

## Contributing Samples

If you create an interesting MacroButtons configuration, consider contributing it as a sample! Useful samples often include:
- Game integrations
- Productivity workflows
- Creative tool macros
- System monitoring tiles

## File Structure

Each sample should include:
- `<name>.json` - The MacroButtons profile configuration
- `README.md` - Setup instructions and description
- Any required scripts or helper files
