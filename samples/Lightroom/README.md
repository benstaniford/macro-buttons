# Lightroom MacroButtons Profile

Quick access to essential Adobe Lightroom Classic keyboard shortcuts optimized for photo editing workflow.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/Lightroom/lightroom.json`
3. The profile will auto-activate when Lightroom is the active window

## Shortcuts Included (20 + Clock)

### Module Navigation (4)
- Library Grid (G) - View all photos in grid layout
- Develop Module (D) - Switch to editing mode
- Loupe View (E) - View single photo enlarged
- Compare View (C) - Compare two photos side-by-side

### Rating & Organization (6)
- Flag Pick (P) - Mark photo as a keeper
- Reject (X) - Mark photo for deletion
- 1 Star (1) - Rate with 1 star
- 2 Stars (2) - Rate with 2 stars
- 3 Stars (3) - Rate with 3 stars
- 4 Stars (4) - Rate with 4 stars

### Develop Tools (5)
- Crop Tool (R) - Activate crop/straighten tool
- Spot Removal (Q) - Remove blemishes and spots
- Brush Mask (K) - Create new brush adjustment mask
- Linear Gradient (M) - Create linear gradient mask
- Radial Gradient (Shift+M) - Create radial gradient mask

### Adjustments (3)
- Auto Tone (Ctrl+U) - Automatic tone adjustment
- Auto White Balance (Ctrl+Shift+U) - Automatic white balance
- Convert to B&W (V) - Convert image to black and white

### View & Interface (2)
- Lights Out (L) - Dim/hide interface (cycles through modes)
- Hide All Panels (Shift+Tab) - Maximize image viewing area

## Theme

Uses Lightroom's characteristic silver/gray color scheme (#C0C0C0) on dark background (#1A1A1A) to match the application's professional interface.

## Customization

Edit `lightroom.json` to:
- Add more shortcuts from [Adobe's official keyboard shortcuts guide](https://helpx.adobe.com/lightroom-classic/help/keyboard-shortcuts.html)
- Rearrange button layout
- Change colors/themes
- Adjust the `activeWindow` pattern if your Lightroom window title differs

## Notes

- All shortcuts are Lightroom-specific (common Windows shortcuts like Ctrl+C/V/S are excluded)
- Each button shows the keyboard shortcut for easy reference
- Shortcuts are organized by workflow stages (navigate → rate → edit → adjust → view)
- Star ratings only go to 4 stars to fit the grid layout (use keyboard for 5 stars)
- Press L multiple times to cycle through Lights Out modes: Dim → Full Black → Normal
