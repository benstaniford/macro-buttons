# Firefox MacroButtons Profile

Quick access to common Firefox keyboard shortcuts optimized for touch screen control.

## Features

This profile includes **21 tiles** with 20 Firefox-specific shortcuts plus a clock display:

### Tab Management
- Reopen Closed Tab (Ctrl+Shift+T)
- Next Tab (Ctrl+Tab)
- Previous Tab (Ctrl+Shift+Tab)
- Jump to Tab 1 (Ctrl+1)
- Jump to Last Tab (Ctrl+9)

### Navigation
- Address Bar (Ctrl+L)
- Search Bar (Ctrl+K)
- Back (Alt+Left)
- Forward (Alt+Right)
- Hard Refresh (Ctrl+Shift+R)

### Bookmarks & History
- Bookmark Page (Ctrl+D)
- Bookmark All Tabs (Ctrl+Shift+D)
- History Sidebar (Ctrl+H)
- Bookmarks Sidebar (Ctrl+Shift+B)

### Developer Tools
- Developer Tools (F12)
- Web Console (Ctrl+Shift+K)
- Responsive Design Mode (Ctrl+Shift+M)

### Window & Settings
- Private Window (Ctrl+Shift+P)
- Add-ons Manager (Ctrl+Shift+A)
- Settings (Ctrl+,)
- Full Screen (F11)

## Usage

1. Right-click the MacroButtons tray icon → Profiles → Import Profile
2. Browse to: `samples/Firefox/firefox.json`
3. The profile will auto-activate when Firefox is the active window

**Note:** The profile is configured to activate when the "firefox" process is detected. You can specify the process name with or without the ".exe" extension (both "firefox" and "firefox.exe" work).

## Theme

Uses Firefox's signature orange (#FF9500) on dark background (#1C1B22) for an authentic Firefox look.

## Customization

Edit the JSON file to:
- Add more Firefox shortcuts
- Rearrange button layout
- Change colors/themes
- Adjust the auto-activation window pattern (currently set to "firefox")

## Notes

- All shortcuts are Firefox-specific (excludes common Windows shortcuts like Ctrl+C/V/X/S)
- Each button displays the keyboard shortcut for easy reference
- Shortcuts are converted to AutoHotkey format for MacroButtons
- The clock tile uses the prominent theme for visibility

## References

Shortcuts sourced from:
- [Firefox Official Keyboard Shortcuts](https://support.mozilla.org/en-US/kb/keyboard-shortcuts-perform-firefox-tasks-quickly)
- [Firefox Developer Tools Documentation](https://firefox-source-docs.mozilla.org/devtools-user/keyboard_shortcuts/index.html)
