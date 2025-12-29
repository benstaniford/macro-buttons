# ComfyUI MacroButtons Profile

Quick access to ComfyUI's most powerful keyboard shortcuts for workflow optimization.

## Usage

1. Right-click MacroButtons tray icon → Profiles → Import Profile
2. Browse to this file: `samples/ComfyUI/comfyui.json`
3. The profile will auto-activate when ComfyUI is the active window

## Shortcuts Included (20 Action Buttons + Clock)

### Generation Control
- **Queue Prompt** (Ctrl+Enter) - Generate your workflow
- **Queue Front** (Ctrl+Shift+Enter) - Priority generation
- **Interrupt** (Ctrl+Alt+Enter) - Stop current generation

### Node Operations
- **Mute Nodes** (Ctrl+M) - Exclude nodes from execution
- **Bypass Nodes** (Ctrl+B) - Pass data through without processing
- **Collapse Nodes** (Alt+C) - Minimize nodes for clean workspace
- **Add Frame** (Ctrl+G) - Group selected nodes
- **Refresh Nodes** (R) - Reload node definitions

### Workflow Management
- **Paste with Links** (Ctrl+Shift+V) - Paste maintaining connections
- **Load Default** (Ctrl+D) - Load default graph

### UI Toggles
- **Toggle Queue** (Q) - Show/hide queue sidebar
- **Toggle Workflow** (W) - Show/hide workflow sidebar
- **Toggle Nodes** (N) - Show/hide node library
- **Toggle Models** (M) - Show/hide model library
- **Toggle Log** (Ctrl+`) - Show/hide log panel
- **Focus Mode** (F) - Distraction-free workspace

### View Controls
- **Zoom In** (Alt+=) - Increase canvas zoom
- **Zoom Out** (Alt+-) - Decrease canvas zoom
- **Fit View** (.) - Fit selected nodes to view
- **Pin Items** (P) - Pin/unpin items in place

## Notes

- Each button displays the keyboard shortcut for easy learning
- Common Windows shortcuts (Ctrl+C/V/S/O/Z/Y) are excluded since they work system-wide
- The profile auto-activates when ComfyUI window is focused
- For macOS users: Replace Ctrl with Cmd and Alt with Opt

## Customization

Edit the JSON file to:
- Add more shortcuts
- Rearrange button layout
- Change colors/themes (current: green on black terminal style)
- Adjust auto-activation window pattern
- Modify refresh interval for the clock display

## Sources

- [ComfyUI Official Shortcuts Documentation](https://docs.comfy.org/interface/shortcuts)
- [ComfyUI Community Manual](https://blenderneko.github.io/ComfyUI-docs/Interface/Shortcuts/)
- [ComfyUI Wiki - Keyboard Shortcuts](https://comfyui-wiki.com/en/interface/settings/shortcuts)
