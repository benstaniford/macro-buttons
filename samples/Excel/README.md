# Microsoft Excel MacroButtons Profile

Quick access to common Microsoft Excel keyboard shortcuts optimized for touch-screen macro panels.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/Excel/microsoft-excel.json`
3. The profile will auto-activate when Excel is the active window

## Shortcuts Included

### Main Panel (20 shortcuts + clock)

**Formulas & Functions:**
- Insert Function (Shift+F3)
- Auto Sum (Alt+=)
- Toggle Absolute Reference (F4) - Switch between A1, $A1, A$1, $A$1
- Show/Hide Formulas (Ctrl+`)
- Edit Cell (F2)

**Data Entry:**
- Fill Down (Ctrl+D) - Copy cell content down
- Fill Right (Ctrl+R) - Copy cell content right
- Insert Current Date (Ctrl+;)
- Insert Current Time (Ctrl+Shift+:)
- New Line in Cell (Alt+Enter) - Add line break within a cell

**Formatting:**
- Format Cells Dialog (Ctrl+1)
- Currency Format (Ctrl+Shift+$)
- Percent Format (Ctrl+Shift+%)
- Date Format (Ctrl+Shift+#)

**Navigation:**
- Go To (Ctrl+G) - Jump to specific cell
- Next Sheet (Ctrl+Page Down)
- Previous Sheet (Ctrl+Page Up)

**Data Management:**
- Create Table (Ctrl+T)
- Create Chart (F11)

**Table Operations (Submenu):**
- Delete Row (Shift+Space, Ctrl+-)
- Delete Column (Ctrl+Space, Ctrl+-)
- Insert Row (Shift+Space, Ctrl++)
- Insert Column (Ctrl+Space, Ctrl++)
- Hide Row (Ctrl+9)
- Hide Column (Ctrl+0)
- Unhide Rows (Ctrl+Shift+9)
- Unhide Columns (Ctrl+Shift+0)

## Features

- **Educational Format:** Each button displays the keyboard shortcut on the second line for easy reference
- **Auto-Activation:** Profile automatically activates when Excel window is in focus
- **Touch-Optimized:** Large buttons perfect for touch screens and secondary displays
- **Retro Terminal Theme:** Green-on-black aesthetic with prominent clock display

## Customization

Edit the JSON file to:
- Add more Excel-specific shortcuts
- Rearrange button layout to match your workflow
- Change colors/themes (foreground, background)
- Adjust refresh interval for the clock
- Modify auto-activation window pattern

## Tips

- **Complex Shortcuts:** Some table operations use two-step shortcuts (e.g., "Shift+Space, Ctrl+-" first selects the row, then deletes it)
- **Format Shortcuts:** Quickly apply number formatting without opening the Format Cells dialog
- **Toggle Absolute Ref (F4):** Press repeatedly while editing a formula to cycle through reference types
- **Show Formulas (Ctrl+`):** Great for reviewing or presenting spreadsheet logic

## Requirements

- MacroButtons application installed
- Microsoft Excel (any recent version)
- Windows with .NET 8 Desktop Runtime
