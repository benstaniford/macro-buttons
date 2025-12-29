# Photoshop MacroButtons Profile

Quick access to essential Adobe Photoshop keyboard shortcuts optimized for touch screen workflow.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/Photoshop/photoshop.json`
3. The profile will auto-activate when Photoshop is the active window

## Shortcuts Included (20 + Clock)

### Layer Operations (6)
- New Layer (Ctrl+Shift+N)
- Duplicate Layer (Ctrl+J)
- Merge Down (Ctrl+E)
- Merge Visible (Ctrl+Shift+E)
- Group Layers (Ctrl+G)
- Stamp Visible (Ctrl+Shift+Alt+E)

### Tools (7)
- Brush Tool (B)
- Eraser Tool (E)
- Move Tool (V)
- Lasso Tool (L)
- Magic Wand (W)
- Eyedropper (I)
- Gradient Tool (G)

### Transform & Edit (4)
- Free Transform (Ctrl+T)
- Fill Dialog (Shift+F5)
- Fill Foreground (Alt+Backspace)
- Inverse Selection (Ctrl+Shift+I)

### View (3)
- Fit on Screen (Ctrl+0)
- Actual Pixels (Ctrl+Alt+0)
- Toggle Panels (Tab)

## Theme

Uses Photoshop blue (#4A90E2) for the color scheme to match the application's branding.

## Customization

Edit `photoshop.json` to:
- Add more shortcuts from [Adobe's official keyboard shortcuts guide](https://helpx.adobe.com/photoshop/using/default-keyboard-shortcuts.html)
- Rearrange button layout
- Change colors/themes
- Adjust the `activeWindow` pattern if your Photoshop window title differs

## Notes

- All shortcuts are application-specific to Photoshop (common Windows shortcuts like Ctrl+C/V/S are excluded)
- Each button shows the keyboard shortcut for easy reference
- Shortcuts are organized into logical categories for quick navigation
- Single-letter tool shortcuts (B, E, V, etc.) may cycle through related tools if pressed repeatedly in Photoshop
