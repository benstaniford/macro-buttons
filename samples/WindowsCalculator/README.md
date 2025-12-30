# Windows Calculator MacroButtons Profile

Quick access to Windows Calculator keyboard shortcuts for touch screen control.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/WindowsCalculator/windows-calculator.json`
3. The profile will auto-activate when Calculator is the active window

## Shortcuts Included

### Mode Switching
- **Standard Mode** (Alt+1) - Switch to basic calculator
- **Scientific Mode** (Alt+2) - Access scientific functions
- **Programmer Mode** (Alt+4) - Binary, hex, octal operations

### Memory Operations
- **Store Memory** (Ctrl+M) - Save current value to memory (MS)
- **Add to Memory** (Ctrl+P) - Add current value to memory (M+)
- **Recall Memory** (Ctrl+R) - Retrieve stored value (MR)
- **Clear Memory** (Ctrl+L) - Clear memory (MC)

### Scientific Functions
- **Toggle Sign** (F9) - Switch between positive/negative
- **Reciprocal** (R) - Calculate 1/x
- **Square Root** (@) - Calculate √x
- **Power** (Y) - Calculate x^y
- **Insert Pi** (P) - Insert π constant (3.14159...)
- **Insert E** (Shift+E) - Insert e constant (2.71828...)
- **Factorial** (!) - Calculate x!

### Trigonometry
- **Sine** (S) - Calculate sin(x)
- **Cosine** (O) - Calculate cos(x)
- **Tangent** (T) - Calculate tan(x)

### Programmer Mode
- **Hex Mode** (F5) - Switch to hexadecimal base
- **Binary Mode** (F8) - Switch to binary base

### Utilities
- **Clear History** (Ctrl+Shift+D) - Clear calculation history

## Tips

- Most scientific and trigonometric functions require **Scientific Mode** to be active
- Programmer mode functions (Hex, Binary) require **Programmer Mode** to be active
- Each button shows the keyboard shortcut on the second line for easy reference
- The clock tile displays the current time

## Customization

Edit the JSON file to:
- Add more shortcuts (Date Calculation mode with Alt+5, more programmer functions)
- Rearrange button layout to match your workflow
- Change colors/themes (current: green on black retro terminal style)
- Adjust auto-activation window pattern if needed

## Additional Shortcuts Not Included

Windows Calculator has 100+ keyboard shortcuts. Some other useful ones you might want to add:
- **Alt+3** - Graphing mode
- **Alt+5** - Date Calculation mode
- **F6** - Decimal mode (programmer)
- **F7** - Octal mode (programmer)
- **Esc** - Clear all (C)
- **Delete** - Clear entry (CE)
- **Ctrl+Q** - Subtract from memory (M-)

## Sources

This profile was created based on official Microsoft documentation and community resources for Windows Calculator shortcuts in Windows 10/11.
