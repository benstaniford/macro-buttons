# MacroButtons Configuration Guide

This guide explains the JSON configuration format for MacroButtons profiles and how to create dynamic tiles with custom scripts.

## Table of Contents

- [Configuration File Location](#configuration-file-location)
- [Basic Structure](#basic-structure)
- [Configuration Properties](#configuration-properties)
  - [Items (Buttons)](#items-buttons)
  - [Themes](#themes)
  - [Global Settings](#global-settings)
- [Button Properties](#button-properties)
  - [Static Titles](#static-titles)
  - [Dynamic Titles](#dynamic-titles)
  - [Actions](#actions)
  - [Submenus](#submenus)
- [Writing Scripts for Dynamic Tiles](#writing-scripts-for-dynamic-tiles)
- [Examples](#examples)

## Configuration File Location

Profiles are stored in:
```
%USERPROFILE%\.macrobuttons\profiles\{profile-name}.json
```

For example:
- Default profile: `%USERPROFILE%\.macrobuttons\profiles\default.json`
- Gaming profile: `%USERPROFILE%\.macrobuttons\profiles\gaming.json`

## Basic Structure

A MacroButtons configuration file has three main sections:

```json
{
  "global": {
    "monitorIndex": 0,
    "refresh": "30s"
  },
  "themes": [
    {
      "name": "default",
      "foreground": "green",
      "background": "black"
    }
  ],
  "items": [
    {
      "title": "My Button",
      "action": {
        "keypress": "^v"
      }
    }
  ]
}
```

## Configuration Properties

### Items (Buttons)

The `items` array defines all buttons in your grid. Each button is a JSON object with properties like `title`, `action`, and optional `theme`.

**Minimum grid size:** 3 rows × 5 columns (15 tiles)

The grid automatically expands to fit your buttons. Empty spaces show as empty tiles.

### Themes

The `themes` array defines color schemes that buttons can use.

```json
"themes": [
  {
    "name": "default",
    "foreground": "green",
    "background": "black"
  },
  {
    "name": "warning",
    "foreground": "black",
    "background": "orange"
  },
  {
    "name": "highlight",
    "foreground": "#FFFFFF",
    "background": "#0066CC"
  }
]
```

**Theme Properties:**
- `name` (string, required) - Unique identifier for the theme
- `foreground` (string) - Text color (CSS color name or hex like `#00FF00`)
- `background` (string) - Background color

**Built-in themes in default config:**
- `"default"` - Green text on black background
- `"toggled"` - Black text on orange background (for warnings/active states)
- `"prominent"` - Black text on bright green background (for important info)

### Global Settings

The `global` object contains settings that apply to the entire profile.

```json
"global": {
  "monitorIndex": 1,
  "profileName": "default",
  "refresh": "30s",
  "activeWindow": "Elite - Dangerous",
  "sendKeys": {
    "delay": "10ms",
    "duration": "30ms"
  }
}
```

**Properties:**

- **`monitorIndex`** (number) - Which monitor to display on (0 = first, 1 = second, etc.)
  - Default: 0 (or smallest monitor if multiple)

- **`profileName`** (string) - Name of this profile
  - Set automatically when creating/renaming profiles

- **`refresh`** (string) - Default refresh interval for dynamic tiles
  - Format: `{number}{unit}` where unit is `ms`, `s`, `m`, or `h`
  - Examples: `"100ms"`, `"5s"`, `"1m"`, `"2h"`
  - Minimum: 100ms

- **`activeWindow`** (string, optional) - Window title pattern for auto-switching
  - When a window matching this pattern is active, MacroButtons auto-switches to this profile
  - Uses partial matching (case-sensitive)
  - Example: `"Elite - Dangerous"` matches "Elite - Dangerous (CLIENT)"

- **`sendKeys`** (object, optional) - Timing configuration for keyboard simulation
  - `delay` - Delay before sending keys after clicking (default: 10ms)
  - `duration` - How long to hold each key down (default: 30ms)

## Button Properties

Each button in the `items` array can have these properties:

```json
{
  "title": "Button Text",
  "action": { /* action definition */ },
  "theme": "warning",
  "items": [ /* submenu items */ ]
}
```

### Static Titles

A simple string:

```json
{
  "title": "Copy",
  "action": { "keypress": "^c" }
}
```

### Dynamic Titles

An object that executes a command and displays the output:

```json
{
  "title": {
    "builtin": "clock()",
    "refresh": "1s",
    "theme": "prominent"
  }
}
```

**Dynamic Title Properties:**

- **Command** (one required):
  - `builtin` - Built-in command (`"clock()"` for current time)
  - `python` - Python command as array: `["-c", "print('Hello')"]` or `["script.py", "arg1"]`
  - `exe` - Executable as array: `["C:\\path\\to\\program.exe", "arg1", "arg2"]`

- **`refresh`** (string, optional) - Override the global refresh interval for this tile
  - Format: same as global refresh
  - Example: `"500ms"` to update twice per second

- **`theme`** (string, optional) - Theme to use for this tile
  - Overrides the button's `theme` property
  - Can still be overridden by script output (see [Dynamic Output Format](#dynamic-output-format))

**Path expansion:**
- `~` expands to user home directory
- `%ENVVAR%` expands environment variables
- Example: `"python": ["~/.local/bin/myscript.py"]`

### Actions

The `action` property defines what happens when you click a button. Can be `null` for information-only tiles.

#### Keypress Action

Simulates keyboard input using AutoHotkey syntax:

```json
{
  "action": {
    "keypress": "^v"
  }
}
```

**Modifier Keys:**
- `^` = Ctrl
- `+` = Shift
- `!` = Alt
- `#` = Windows key

**Special Keys:**
```
{Enter}  {Tab}      {Space}   {Backspace}
{Delete} {Insert}   {Home}    {End}
{PgUp}   {PgDn}     {Up}      {Down}
{Left}   {Right}
{F1}     {F2}       ... {F12}
{Esc}    {PrintScreen}
{NumLock} {ScrollLock} {CapsLock}
```

**Examples:**
- `"^c"` - Ctrl+C
- `"^+s"` - Ctrl+Shift+S
- `"!{F4}"` - Alt+F4
- `"#d"` - Windows+D (show desktop)
- `"{F11}"` - F11 key
- `"^v{Enter}"` - Ctrl+V then Enter

#### Python Action

Executes a Python script or command:

```json
{
  "action": {
    "python": ["-c", "import os; os.startfile('C:\\\\Users')"]
  }
}
```

Or with a script file:

```json
{
  "action": {
    "python": ["~/scripts/my-script.py", "--arg1", "value"]
  }
}
```

**Notes:**
- Runs silently (no console window)
- Use `python` (not `pythonw`) - MacroButtons hides the window automatically
- First array element is the script/command, rest are arguments

#### Executable Action

Launches any executable:

```json
{
  "action": {
    "exe": "C:\\Program Files\\MyApp\\app.exe"
  }
}
```

With arguments:

```json
{
  "action": {
    "exe": "notepad.exe C:\\temp\\file.txt"
  }
}
```

Or as an array:

```json
{
  "action": {
    "exe": ["notepad.exe", "C:\\temp\\file.txt"]
  }
}
```

#### Builtin Action

Execute built-in commands:

```json
{
  "action": {
    "builtin": "quit()"
  }
}
```

**Available builtins:**
- `quit()` - Exit MacroButtons

### Submenus

Buttons can have nested submenus for organizing related actions:

```json
{
  "title": "Games",
  "items": [
    {
      "title": "Minecraft",
      "action": { "exe": "C:\\Games\\Minecraft\\launcher.exe" }
    },
    {
      "title": "Steam",
      "action": { "exe": "steam://open/games" }
    }
  ]
}
```

**Submenu behavior:**
- Clicking the button navigates into the submenu
- A "← BACK" button is automatically added as the first item
- Can nest multiple levels deep

**Submenu + Action:**

Buttons can have BOTH an action and a submenu:

```json
{
  "title": "Launch & Configure",
  "action": { "exe": "myapp.exe" },
  "items": [
    {
      "title": "Settings",
      "action": { "exe": "myapp.exe --settings" }
    }
  ]
}
```

The action executes first, then navigates to the submenu.

## Writing Scripts for Dynamic Tiles

Dynamic tiles execute commands periodically and display the output. This is perfect for system monitoring, game integration, or any real-time data.

### Basic Script Output

The simplest approach: output plain text to stdout.

**Python example:**

```python
#!/usr/bin/env python3
import psutil
cpu_percent = psutil.cpu_percent(interval=1)
print(f"CPU: {cpu_percent}%")
```

**Configuration:**

```json
{
  "title": {
    "python": ["~/scripts/cpu-monitor.py"],
    "refresh": "2s"
  }
}
```

Result: Tile shows "CPU: 23.5%" using the button's theme colors.

### Dynamic Output Format

For full control over appearance, output JSON with this format:

```json
{
  "text": "Display text",
  "theme": "warning",
  "fg": "#00FF00",
  "bg": "#000000"
}
```

**Properties (all optional except `text`):**

- **`text`** (string, required) - The text to display on the tile

- **`theme`** (string) - Theme name to use
  - Looks up colors from your `themes` array
  - Example: `"warning"` uses the "warning" theme colors

- **`fg`** (string) - Foreground color override
  - CSS color name or hex: `"red"`, `"#FF0000"`
  - Overrides theme foreground

- **`bg`** (string) - Background color override
  - CSS color name or hex: `"black"`, `"#000000"`
  - Overrides theme background

**Priority:** `fg`/`bg` > `theme` > button's theme > `"default"` theme

### Advanced Examples

#### Example 1: Using Theme Names

**Script (Python):**

```python
#!/usr/bin/env python3
import json
import psutil

cpu = psutil.cpu_percent(interval=0.1)

# Use "warning" theme if CPU is high
if cpu > 80:
    output = {"text": f"CPU: {cpu}%", "theme": "warning"}
else:
    output = {"text": f"CPU: {cpu}%", "theme": "default"}

print(json.dumps(output))
```

**Result:**
- Below 80%: Green text on black (default theme)
- Above 80%: Black text on orange (warning theme)

#### Example 2: Custom Colors

**Script (Bash):**

```bash
#!/bin/bash
if ping -c 1 8.8.8.8 &> /dev/null; then
    echo '{"text": "Online", "fg": "#00FF00", "bg": "#003300"}'
else
    echo '{"text": "Offline", "fg": "#FF0000", "bg": "#330000"}'
fi
```

#### Example 3: Elite Dangerous Integration

**Script (from samples):**

```python
#!/usr/bin/env python3
import json
import os
from pathlib import Path

# Read Elite Dangerous status file
status_file = Path.home() / 'Saved Games' / 'Frontier Developments' / 'Elite Dangerous' / 'Status.json'

with open(status_file) as f:
    data = json.load(f)

# Check landing gear flag (bit 2)
flags = data.get('Flags', 0)
if flags & 4:
    print(json.dumps({"text": "GEAR LOWERED", "theme": "toggled"}))
else:
    print(json.dumps({"text": "GEAR RAISED", "theme": "default"}))
```

See `samples/EliteDangerous/` for the complete example.

### Script Best Practices

1. **Keep it fast** - Scripts run on the UI thread
   - Aim for < 100ms execution time
   - Don't set refresh intervals faster than necessary

2. **Handle errors gracefully** - If your script crashes:
   - The tile shows "Error: {message}"
   - Fix the script and reload

3. **Use environment variables** - For paths:
   ```python
   import os
   home = os.environ.get('USERPROFILE')  # Windows
   # or
   home = os.path.expanduser('~')  # Cross-platform
   ```

4. **Output JSON for best results** - Plain text works, but JSON gives you:
   - Theme support
   - Color control
   - Better formatting options

5. **Test standalone first** - Before adding to MacroButtons:
   ```bash
   python your-script.py
   # Should output either plain text or valid JSON
   ```

## Examples

### Example 1: Simple Macro Buttons

```json
{
  "items": [
    {
      "title": "Copy",
      "action": { "keypress": "^c" }
    },
    {
      "title": "Paste",
      "action": { "keypress": "^v" }
    },
    {
      "title": "Save",
      "action": { "keypress": "^s" }
    }
  ]
}
```

### Example 2: Application Launcher

```json
{
  "items": [
    {
      "title": "Browser",
      "action": { "exe": "C:\\Program Files\\Mozilla Firefox\\firefox.exe" }
    },
    {
      "title": "Calculator",
      "action": { "exe": "calc.exe" }
    },
    {
      "title": "Notepad",
      "action": { "exe": "notepad.exe" }
    }
  ]
}
```

### Example 3: Dynamic System Monitor

```json
{
  "items": [
    {
      "title": {
        "python": ["-c", "import psutil; print(f'CPU: {psutil.cpu_percent()}%')"],
        "refresh": "2s"
      }
    },
    {
      "title": {
        "python": ["-c", "import psutil; print(f'RAM: {psutil.virtual_memory().percent}%')"],
        "refresh": "5s"
      }
    },
    {
      "title": {
        "builtin": "clock()",
        "refresh": "1s",
        "theme": "prominent"
      }
    }
  ]
}
```

### Example 4: Gaming Profile with Themes

```json
{
  "global": {
    "monitorIndex": 1,
    "activeWindow": "MyGame.exe"
  },
  "themes": [
    {
      "name": "default",
      "foreground": "cyan",
      "background": "black"
    },
    {
      "name": "active",
      "foreground": "black",
      "background": "cyan"
    }
  ],
  "items": [
    {
      "title": "Quick Save",
      "action": { "keypress": "{F5}" },
      "theme": "default"
    },
    {
      "title": "Quick Load",
      "action": { "keypress": "{F9}" },
      "theme": "default"
    },
    {
      "title": {
        "python": ["~/scripts/game-stats.py"],
        "refresh": "1s",
        "theme": "active"
      }
    }
  ]
}
```

### Example 5: Hierarchical Menu

```json
{
  "items": [
    {
      "title": "Development",
      "items": [
        {
          "title": "VS Code",
          "action": { "exe": "code.exe" }
        },
        {
          "title": "Terminal",
          "action": { "exe": "wt.exe" }
        },
        {
          "title": "Git",
          "items": [
            {
              "title": "Status",
              "action": { "exe": "wt.exe git status" }
            },
            {
              "title": "Pull",
              "action": { "exe": "wt.exe git pull" }
            }
          ]
        }
      ]
    },
    {
      "title": "Media",
      "items": [
        {
          "title": "Spotify",
          "action": { "exe": "spotify.exe" }
        },
        {
          "title": "VLC",
          "action": { "exe": "vlc.exe" }
        }
      ]
    }
  ]
}
```

## Tips and Tricks

### Using Multiple Themes Effectively

Create themes for different states:

```json
"themes": [
  {"name": "normal", "foreground": "green", "background": "black"},
  {"name": "warning", "foreground": "yellow", "background": "#332200"},
  {"name": "alert", "foreground": "white", "background": "red"},
  {"name": "info", "foreground": "cyan", "background": "#003333"}
]
```

Then use them in scripts based on state:

```python
if value < 50:
    theme = "normal"
elif value < 80:
    theme = "warning"
else:
    theme = "alert"

print(json.dumps({"text": f"Value: {value}", "theme": theme}))
```

### Conditional Profile Switching

Set up profiles that auto-activate based on the active window:

**gaming.json:**
```json
{
  "global": {
    "activeWindow": "MyGame"
  },
  "items": [ /* gaming controls */ ]
}
```

**streaming.json:**
```json
{
  "global": {
    "activeWindow": "OBS"
  },
  "items": [ /* OBS controls */ ]
}
```

MacroButtons will automatically switch profiles when you switch windows!

### Environment Variables in Paths

Instead of hardcoding paths:

```json
{
  "action": {
    "python": ["%USERPROFILE%\\scripts\\my-script.py"]
  }
}
```

Or use tilde expansion:

```json
{
  "action": {
    "python": ["~/scripts/my-script.py"]
  }
}
```

### Debugging Dynamic Tiles

If a dynamic tile shows "Error":

1. Test the script manually:
   ```bash
   python ~/scripts/my-script.py
   ```

2. Check for valid JSON output (if using JSON format):
   ```bash
   python ~/scripts/my-script.py | python -m json.tool
   ```

3. Check the script has correct line endings (especially on Windows)

4. Ensure the script has execute permissions (Linux/Mac):
   ```bash
   chmod +x ~/scripts/my-script.py
   ```

## Reference

### Complete Configuration Schema

```json
{
  "global": {
    "monitorIndex": 0,
    "profileName": "default",
    "refresh": "30s",
    "activeWindow": "optional window pattern",
    "sendKeys": {
      "delay": "10ms",
      "duration": "30ms"
    }
  },
  "themes": [
    {
      "name": "theme-name",
      "foreground": "color",
      "background": "color"
    }
  ],
  "items": [
    {
      "title": "static text",
      "action": {
        "keypress": "^v"
      },
      "theme": "theme-name"
    },
    {
      "title": {
        "python": ["script.py"],
        "refresh": "1s",
        "theme": "theme-name"
      },
      "action": null
    },
    {
      "title": "Menu",
      "items": [
        { /* nested buttons */ }
      ]
    }
  ]
}
```

### Action Types

| Type | Property | Value | Example |
|------|----------|-------|---------|
| Keyboard | `keypress` | AutoHotkey string | `"^v"` |
| Python | `python` | Array of strings | `["script.py", "arg"]` |
| Executable | `exe` | String or array | `"calc.exe"` |
| Builtin | `builtin` | Command string | `"quit()"` |

### Dynamic Title Types

| Type | Property | Value | Example |
|------|----------|-------|---------|
| Python | `python` | Array of strings | `["script.py"]` |
| Executable | `exe` | Array of strings | `["cmd.exe", "/c", "echo Hi"]` |
| Builtin | `builtin` | Command string | `"clock()"` |

### Time Format

Used for `refresh`, `delay`, `duration`:

| Unit | Example | Meaning |
|------|---------|---------|
| `ms` | `"100ms"` | Milliseconds |
| `s` | `"30s"` | Seconds |
| `m` | `"5m"` | Minutes |
| `h` | `"2h"` | Hours |

Minimum refresh interval: `100ms`

---

For more information, see the [main README](README.md) or check out the [samples](samples/) directory.
