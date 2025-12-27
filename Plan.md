<plan>

    <highlevel>Create a WPF/C# application which fills an entire screen on the windows OS.  This is intended for a secondary screen when gaming or creating and should provide a set of tiles which can either be buttons for macros, or display information, or both.  It's intended to be used with a small touch screen monitor</highlevel>

    <constraints>This window must never steal focus and should fill the screen.  It should also occlude the start menu.  It should be a so called non-activating window</constraints>

    <data>

    The application should be as flexible as possible and should read the %USERPROFILE%\.macrobuttons.json file when it starts.  If no such file exists, a new one should be created with a simple hello world application.

    The json file should allow the possibility of both macro buttons and information buttons and should be defined thus:

    {
    "items": [
        {
        "title": "An static button example",
        "action": { "keypress": "^v" }
        },
        {
        "title": "A second static button example",
        "action": { "python": ["~/mypython.py", "argument1"] }
        },
        {
        "title": "A third static button example",
        "action": { "exe": "c:/windows/notepad.exe" }
        },
        {
        "title": { "python": ["~/myinfoprogram.py"] },
        "action": null
        }
    ]
    "theme" :
    {
        "foreground": "darkgreen",
        "background": "black"
    }
    "global" :
    {
        "refresh": 30s
    }
    }

    </data>

    <appearance>The program should have the appearance of a retro terminal curses application although in fact, it will be entirely implemented in C#/WPF and should be a modern windows app under the hood.  It should use monospace fonts.  Each definition should be an automatically sized tile on the canvas.  The minimum number of tiles should be 3 high x 4 wide although if more are specified in the JSON, the program should try to add rows and columns to compenstate.  The tiles should fill the available space and there should be no border between them.  Unused tiles should simply be empty boxes.</appearance>

    <behavior>If a static title is declared, it should simply be shown as the title of the button</behavior>
    <behavior>If a command of some kind is declared as the title, it should be run and it's stdout should be used as the current title of the button</behavior>
    <behavior>Commands should always be run in such a way that window creation is completely supressed and the commands are run silently</behavior>
    <behavior>Actions may be python scripts, executables or keystrokes</behavior>
    <behavior>If keystrokes are specified, autohotkey's keystroke conventions should be used</behavior>
    <behavior>Keystrokes should always be sent to the application that was active prior to this application being used (e.g. the game that was being played)</behavior>
    <behavior>If keystrokes are specified, autohotkey's keystroke conventions should be used</behavior>

    <steps>
    <step>Clarify this plan asking any questions needed before we proceed</step>
    <step>Manually create a Windows c#/WPF project with an sln file, since you're running on linux and I'll be testing on windows</step>
    <step>Create the application</step>
    <step>Create a Wix Installer based on ~/Code/photonize/Photonize.Installer</step>
    <step>Create a github release action based on ~/Code/photonize/.github/workflows/release.yml</step>
    </steps>
</plan>
