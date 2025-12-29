# Elite Dangerous Sample Profile

This sample profile demonstrates how to integrate MacroButtons with Elite Dangerous, the space simulation game.

## Files

- **elitedangerous.json** - MacroButtons profile configuration for Elite Dangerous
- **elite-status** - Python script that reads Elite Dangerous game status

## Features

This profile provides dynamic tiles that show:
- **Landing Gear Status** - Shows "GEAR LOWERED" (orange) or "GEAR RAISED" (green)
- **Cargo Scoop Status** - Shows "SCOOP DEPLOYED" (orange) or "SCOOP RETRACTED" (green)

The tiles automatically update by reading the `Status.json` file that Elite Dangerous maintains while the game is running.

## Installation

1. **Import the Profile:**
   - Right-click the MacroButtons tray icon
   - Select "Profiles" → "Import Profile..."
   - Choose `elitedangerous.json`
   - Name the profile (e.g., "EliteDangerous")

2. **Install the Script:**
   - Copy `elite-status` to a location in your PATH, or
   - Update the profile configuration to point to the script location

   On Windows, you might place it in:
   ```
   %USERPROFILE%\bin\elite-status
   ```

3. **Verify Elite Dangerous Status File Location:**
   - The script expects Elite Dangerous status files at:
     - Windows: `%USERPROFILE%\Saved Games\Frontier Developments\Elite Dangerous\Status.json`
   - If your files are elsewhere, you may need to adjust the script

## How It Works

The `elite-status` script reads Elite Dangerous's `Status.json` file and checks the `Flags` field for various status bits:
- **Bit 2 (value 4)** - Landing gear deployed
- **Bit 9 (value 512)** - Cargo scoop deployed

The script outputs JSON in the format:
```json
{"text": "GEAR LOWERED", "theme": "toggled"}
```

MacroButtons uses the `theme` property to apply the appropriate color scheme:
- **"default"** theme - Green text on black background (normal state)
- **"toggled"** theme - Black text on orange background (warning/active state)

## Customization

You can extend this profile to monitor other Elite Dangerous status flags:
- Hardpoints deployed (bit 6, value 64)
- Supercruise (bit 4, value 16)
- In danger (bit 16, value 65536)
- And many more...

See the [Elite Dangerous Journal documentation](https://elite-journal.readthedocs.io/en/latest/Status%20File/) for all available flags.
