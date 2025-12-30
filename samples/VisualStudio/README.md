# Visual Studio MacroButtons Profile

Quick access to essential Visual Studio IDE keyboard shortcuts for touch screen control.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/VisualStudio/visual-studio.json`
3. The profile will auto-activate when Visual Studio is the active window (matches "devenv" process)

## Shortcuts Included

### Debugging (9 shortcuts)
- **Start Debugging** (F5) - Begin debugging session or continue execution
- **Start No Debug** (Ctrl+F5) - Run application without debugger attached
- **Toggle Breakpoint** (F9) - Add or remove breakpoint at current line
- **Step Over** (F10) - Execute next line without entering functions
- **Step Into** (F11) - Execute next line and enter function calls
- **Step Out** (Shift+F11) - Execute until returning from current function
- **Stop Debugging** (Shift+F5) - End debugging session
- **Run to Cursor** (Ctrl+F10) - Execute until reaching cursor position
- **Quick Watch** (Shift+F9) - Open Quick Watch dialog for selected expression

### Navigation (4 shortcuts)
- **Go to Definition** (F12) - Navigate to symbol definition
- **Peek Definition** (Alt+F12) - View definition in inline peek window
- **Find References** (Shift+F12) - Find all references to selected symbol
- **Go to All** (Ctrl+T) - Universal search for files, types, members, symbols

### Code Editing (3 shortcuts)
- **Format Document** (Ctrl+K, Ctrl+D) - Auto-format entire document
- **Comment Lines** (Ctrl+K, Ctrl+C) - Comment out selected lines
- **Uncomment Lines** (Ctrl+K, Ctrl+U) - Remove comment syntax from selection

### Refactoring (2 shortcuts)
- **Rename Symbol** (Ctrl+R, Ctrl+R) - Rename symbol across entire solution
- **Extract Method** (Ctrl+R, Ctrl+M) - Extract selected code into new method

### Build & Project (1 shortcut)
- **Build Solution** (Ctrl+Shift+B) - Compile entire solution

### Window Management (1 shortcut)
- **Solution Explorer** (Ctrl+Alt+L) - Show/focus Solution Explorer window

## Tips

- **Debugging workflow**: Use F5 to start, F9 to set breakpoints, F10/F11 to step through code
- **Quick navigation**: Use Ctrl+T to quickly find anything in your project
- **Code quality**: Format Document and Extract Method help maintain clean code
- **Multi-step shortcuts**: For shortcuts like Ctrl+K, Ctrl+D, the button sends both key combinations in sequence
- Each button shows the keyboard shortcut on the second line for easy reference

## Customization

Edit the JSON file to:
- Add more shortcuts (see list below for additional options)
- Rearrange button layout to match your workflow
- Change colors/themes (current: green on black retro terminal style)
- Adjust auto-activation window pattern (`activeWindow: "devenv"` matches Visual Studio's process name)

## Additional Useful Shortcuts Not Included

Consider adding these Visual Studio shortcuts if you need them:

**More Debugging:**
- **Ctrl+Shift+F5** - Restart debugging session
- **Ctrl+Alt+Break** - Break all (pause execution)
- **Ctrl+F12** - Go to declaration

**More Navigation:**
- **Ctrl+-** - Navigate backward in code
- **Ctrl+Shift+-** - Navigate forward in code
- **F3** - Find next occurrence

**More Editing:**
- **Ctrl+L** - Cut entire line
- **Ctrl+D** - Duplicate line
- **Alt+Up/Down** - Move line up/down
- **Ctrl+K, Ctrl+I** - Quick Info tooltip
- **Ctrl+Space** - IntelliSense completion
- **Ctrl+J** - List members

**More Refactoring:**
- **Ctrl+R, Ctrl+E** - Encapsulate field
- **Ctrl+R, Ctrl+I** - Extract interface

**Windows & Views:**
- **Ctrl+Alt+O** - Output window
- **Ctrl+\\, E** - Error List
- **F4** - Properties window
- **Ctrl+Alt+X** - Toolbox

**Project Operations:**
- **Ctrl+Shift+A** - Add new item to project
- **Shift+Alt+A** - Add existing item

## Sources

This profile was created based on official Microsoft documentation:
- Microsoft Learn: [Default keyboard shortcuts in Visual Studio](https://learn.microsoft.com/en-us/visualstudio/ide/default-keyboard-shortcuts-in-visual-studio?view=vs-2022)
- Visual Studio productivity features and keyboard navigation guides

The shortcuts focus on Visual Studio-specific operations for debugging, navigation, refactoring, and code editing that are unique to the IDE.
