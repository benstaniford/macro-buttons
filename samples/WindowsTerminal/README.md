# Windows Terminal MacroButtons Profile

Quick access to essential Windows Terminal keyboard shortcuts for power users.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/WindowsTerminal/windows-terminal.json`
3. The profile will auto-activate when Windows Terminal is the active window

## Shortcuts Included

### Command & Settings (3 shortcuts)
- **Command Palette** (Ctrl+Shift+P) - Quick access to all commands
- **Open Settings** (Ctrl+,) - Opens settings.json
- **Settings UI** (Ctrl+Shift+,) - Opens graphical settings interface

### Tab Management (6 shortcuts)
- **New Tab** (Ctrl+Shift+T) - Opens new tab with default profile
- **New Tab Profile 1** (Ctrl+Shift+1) - Opens tab with specific profile
- **Next Tab** (Ctrl+Tab) - Cycle to next tab
- **Previous Tab** (Ctrl+Shift+Tab) - Cycle to previous tab
- **Switch to Tab 1** (Ctrl+Alt+1) - Jump directly to first tab
- **Duplicate Tab** (Ctrl+Shift+D) - Clone current tab

### Pane Management (6 shortcuts)
- **Split Horizontal** (Alt+Shift+-) - Split pane horizontally
- **Split Vertical** (Alt+Shift+=) - Split pane vertically
- **Focus Up/Down** (Alt+Arrow) - Move focus between panes
- **Resize Up/Down** (Alt+Shift+Arrow) - Resize focused pane

### Search & View (5 shortcuts)
- **Find** (Ctrl+Shift+F) - Search terminal output
- **Fullscreen** (F11) - Toggle fullscreen mode
- **Zoom In/Out** (Ctrl+/Ctrl-) - Adjust font size
- **Reset Zoom** (Ctrl+0) - Return to default font size

## Theme

The profile uses a **cyan on black** color scheme that complements the default Windows Terminal aesthetic.

## Customization

Edit the JSON file to:
- Add more profile-specific shortcuts (Ctrl+Shift+2, Ctrl+Shift+3, etc.)
- Customize colors to match your Windows Terminal theme
- Add additional pane navigation shortcuts (Alt+Left, Alt+Right)
- Include dropdown menu shortcut (Ctrl+Shift+Space)

## Notes

- The profile targets windows with "Windows Terminal" in the title
- All shortcuts are Windows Terminal-specific - common shortcuts like Ctrl+C/V are intentionally excluded
- Each button displays the keyboard shortcut on the second line for easy reference
