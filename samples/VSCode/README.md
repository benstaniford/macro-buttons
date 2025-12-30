# VS Code MacroButtons Profile

Quick access to essential VS Code keyboard shortcuts for touch screen control.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/VSCode/vscode.json`
3. The profile will auto-activate when VS Code is the active window (matches "Code" process)

## Shortcuts Included

### Navigation (4 shortcuts)
- **Command Palette** (Ctrl+Shift+P) - Access all VS Code commands instantly
- **Go to Symbol** (Ctrl+Shift+O) - Navigate to functions, classes in current file
- **Go to Line** (Ctrl+G) - Jump directly to any line number
- **Go to Definition** (F12) - Navigate to symbol definition

### View Management (3 shortcuts)
- **Toggle Terminal** (Ctrl+`) - Show/hide integrated terminal
- **Toggle Sidebar** (Ctrl+B) - Show/hide file explorer sidebar
- **Split Editor** (Ctrl+\\) - Split editor for side-by-side editing

### Code Editing (7 shortcuts)
- **Delete Line** (Ctrl+Shift+K) - Remove entire line without selecting
- **Move Line Up** (Alt+Up) - Move current line up one position
- **Move Line Down** (Alt+Down) - Move current line down one position
- **Copy Line Down** (Shift+Alt+Down) - Duplicate line below current position
- **Toggle Comment** (Ctrl+/) - Comment/uncomment selected lines
- **Format Document** (Shift+Alt+F) - Auto-format entire document
- **Rename Symbol** (F2) - Rename symbol across all files

### Advanced Navigation (3 shortcuts)
- **Peek Definition** (Alt+F12) - View definition in inline peek window
- **Find References** (Shift+F12) - Find all references to symbol
- **Next Match** (Ctrl+D) - Select next occurrence of current selection

### Multi-Cursor (1 shortcut)
- **Select All Matches** (Ctrl+Shift+L) - Add cursors to all occurrences

### Debugging (2 shortcuts)
- **Toggle Breakpoint** (F9) - Add/remove breakpoint at current line
- **Start Debugging** (F5) - Begin debugging session

## Tips

- **Command Palette is your friend**: Ctrl+Shift+P gives you access to everything in VS Code
- **Multi-cursor editing**: Use Ctrl+D to select next match, then Ctrl+Shift+L to select all at once
- **Quick navigation**: Ctrl+Shift+O for symbols in file, F12 for definitions
- **Terminal integration**: Ctrl+` toggles terminal without leaving editor
- Each button shows the keyboard shortcut on the second line for easy reference

## Customization

Edit the JSON file to:
- Add more shortcuts (see list below for additional options)
- Rearrange button layout to match your workflow
- Change colors/themes (current: green on black retro terminal style)
- Adjust auto-activation window pattern (`activeWindow: "Code"` matches VS Code's process name)

## Additional Useful Shortcuts Not Included

Consider adding these VS Code shortcuts if you need them:

**More Editing:**
- **Ctrl+L** - Select current line
- **Shift+Alt+Up** - Copy line up
- **Ctrl+Shift+[** - Fold code block
- **Ctrl+Shift+]** - Unfold code block
- **Ctrl+K Ctrl+0** - Fold all regions
- **Ctrl+K Ctrl+J** - Unfold all regions
- **Ctrl+K Ctrl+F** - Format selection
- **Alt+Click** - Add cursor at click position
- **Ctrl+Alt+Up/Down** - Add cursor above/below

**More Navigation:**
- **Ctrl+T** - Go to symbol in workspace
- **Ctrl+R** - Open recent folders
- **Alt+Left/Right** - Navigate backward/forward
- **Ctrl+Shift+M** - Show problems panel
- **F8 / Shift+F8** - Navigate through errors

**More Debugging:**
- **F10** - Step over
- **F11** - Step into
- **Shift+F11** - Step out
- **Shift+F5** - Stop debugging
- **Ctrl+Shift+D** - Show Debug view

**View & Panel:**
- **Ctrl+J** - Toggle panel (terminal/output/problems)
- **Ctrl+Shift+E** - Show Explorer view
- **Ctrl+Shift+F** - Global search across files
- **Ctrl+Shift+X** - Show Extensions view
- **Ctrl+K Z** - Toggle Zen Mode (distraction-free)

**File Operations:**
- **Ctrl+K Ctrl+S** - Open keyboard shortcuts editor
- **Ctrl+K Ctrl+T** - Change color theme
- **Shift+Ctrl+V** - Markdown preview

## Sources

This profile was created based on official VS Code documentation and productivity guides:
- [VS Code Tips and Tricks](https://code.visualstudio.com/docs/getstarted/tips-and-tricks)
- [Hackr.io - The 30 Best VSCode Shortcuts for 2025](https://hackr.io/blog/best-vscode-shortcuts)
- [Turing Blog - Best VS Code Shortcuts for 2025](https://www.turing.com/blog/vs-code-shortcuts-and-productivity-hacks)

The shortcuts focus on VS Code-specific operations for editing, navigation, debugging, and multi-cursor editing that make the editor powerful for modern development workflows.
