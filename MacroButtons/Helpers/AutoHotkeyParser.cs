using WindowsInput.Native;

namespace MacroButtons.Helpers;

/// <summary>
/// Parses AutoHotkey keystroke syntax into key action sequences.
/// Supports: ^ (Ctrl), + (Shift), ! (Alt), # (Win), {SpecialKey}
/// </summary>
public class AutoHotkeyParser
{
    /// <summary>
    /// Parses an AutoHotkey keystroke string into a list of key actions.
    /// </summary>
    public List<KeyAction> Parse(string autoHotkeyString)
    {
        if (string.IsNullOrEmpty(autoHotkeyString))
            return new List<KeyAction>();

        var actions = new List<KeyAction>();
        var modifiers = new List<VirtualKeyCode>();

        int i = 0;
        while (i < autoHotkeyString.Length)
        {
            char c = autoHotkeyString[i];

            switch (c)
            {
                case '^':
                    modifiers.Add(VirtualKeyCode.CONTROL);
                    i++;
                    break;
                case '+':
                    modifiers.Add(VirtualKeyCode.SHIFT);
                    i++;
                    break;
                case '!':
                    modifiers.Add(VirtualKeyCode.MENU); // Alt
                    i++;
                    break;
                case '#':
                    modifiers.Add(VirtualKeyCode.LWIN);
                    i++;
                    break;
                case '{':
                    // Parse special key like {Enter}, {Tab}, etc.
                    var endBrace = autoHotkeyString.IndexOf('}', i);
                    if (endBrace == -1)
                    {
                        // Malformed, skip
                        i++;
                        continue;
                    }
                    var specialKey = autoHotkeyString.Substring(i + 1, endBrace - i - 1);
                    var vk = ParseSpecialKey(specialKey);
                    if (vk != null)
                    {
                        actions.AddRange(CreateKeyActions(modifiers, vk.Value));
                    }
                    i = endBrace + 1;
                    modifiers.Clear();
                    break;
                default:
                    // Regular character
                    var keyCode = CharToVirtualKey(c);
                    if (keyCode != null)
                    {
                        actions.AddRange(CreateKeyActions(modifiers, keyCode.Value));
                    }
                    i++;
                    modifiers.Clear();
                    break;
            }
        }

        return actions;
    }

    /// <summary>
    /// Parses special key names like "Enter", "Tab", "Space", etc.
    /// </summary>
    private VirtualKeyCode? ParseSpecialKey(string key)
    {
        return key.ToLower() switch
        {
            "enter" or "return" => VirtualKeyCode.RETURN,
            "tab" => VirtualKeyCode.TAB,
            "space" => VirtualKeyCode.SPACE,
            "backspace" or "bs" => VirtualKeyCode.BACK,
            "delete" or "del" => VirtualKeyCode.DELETE,
            "insert" or "ins" => VirtualKeyCode.INSERT,
            "home" => VirtualKeyCode.HOME,
            "end" => VirtualKeyCode.END,
            "pageup" or "pgup" => VirtualKeyCode.PRIOR,
            "pagedown" or "pgdn" => VirtualKeyCode.NEXT,
            "up" => VirtualKeyCode.UP,
            "down" => VirtualKeyCode.DOWN,
            "left" => VirtualKeyCode.LEFT,
            "right" => VirtualKeyCode.RIGHT,
            "esc" or "escape" => VirtualKeyCode.ESCAPE,
            "f1" => VirtualKeyCode.F1,
            "f2" => VirtualKeyCode.F2,
            "f3" => VirtualKeyCode.F3,
            "f4" => VirtualKeyCode.F4,
            "f5" => VirtualKeyCode.F5,
            "f6" => VirtualKeyCode.F6,
            "f7" => VirtualKeyCode.F7,
            "f8" => VirtualKeyCode.F8,
            "f9" => VirtualKeyCode.F9,
            "f10" => VirtualKeyCode.F10,
            "f11" => VirtualKeyCode.F11,
            "f12" => VirtualKeyCode.F12,
            "printscreen" or "prtsc" => VirtualKeyCode.SNAPSHOT,
            "pause" => VirtualKeyCode.PAUSE,
            "capslock" => VirtualKeyCode.CAPITAL,
            "numlock" => VirtualKeyCode.NUMLOCK,
            "scrolllock" => VirtualKeyCode.SCROLL,
            _ => null
        };
    }

    /// <summary>
    /// Converts a character to its virtual key code.
    /// </summary>
    private VirtualKeyCode? CharToVirtualKey(char c)
    {
        // Convert to uppercase for letter keys
        c = char.ToUpper(c);

        // Letters A-Z
        if (c >= 'A' && c <= 'Z')
        {
            return (VirtualKeyCode)c;
        }

        // Numbers 0-9
        if (c >= '0' && c <= '9')
        {
            return (VirtualKeyCode)c;
        }

        // Special characters
        return c switch
        {
            ' ' => VirtualKeyCode.SPACE,
            ',' => VirtualKeyCode.OEM_COMMA,
            '.' => VirtualKeyCode.OEM_PERIOD,
            '/' => VirtualKeyCode.OEM_2, // Forward slash
            ';' => VirtualKeyCode.OEM_1, // Semicolon
            '\'' => VirtualKeyCode.OEM_7, // Quote
            '[' => VirtualKeyCode.OEM_4, // Open bracket
            ']' => VirtualKeyCode.OEM_6, // Close bracket
            '\\' => VirtualKeyCode.OEM_5, // Backslash
            '-' => VirtualKeyCode.OEM_MINUS,
            '=' => VirtualKeyCode.OEM_PLUS,
            '`' => VirtualKeyCode.OEM_3, // Tilde/backtick
            _ => null
        };
    }

    /// <summary>
    /// Creates a sequence of key actions with modifiers.
    /// </summary>
    private List<KeyAction> CreateKeyActions(List<VirtualKeyCode> modifiers, VirtualKeyCode key)
    {
        var actions = new List<KeyAction>();

        // Press modifiers down
        foreach (var mod in modifiers)
        {
            actions.Add(new KeyAction
            {
                Type = KeyActionType.ModifierDown,
                VirtualKeyCode = mod
            });
        }

        // Press the key
        actions.Add(new KeyAction
        {
            Type = KeyActionType.KeyPress,
            VirtualKeyCode = key
        });

        // Release modifiers (in reverse order)
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            actions.Add(new KeyAction
            {
                Type = KeyActionType.ModifierUp,
                VirtualKeyCode = modifiers[i]
            });
        }

        return actions;
    }
}

/// <summary>
/// Represents a single keystroke action.
/// </summary>
public class KeyAction
{
    public KeyActionType Type { get; set; }
    public VirtualKeyCode VirtualKeyCode { get; set; }
}

/// <summary>
/// Types of key actions.
/// </summary>
public enum KeyActionType
{
    ModifierDown,
    ModifierUp,
    KeyPress
}
