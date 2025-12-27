using MacroButtons.Helpers;
using WindowsInput;
using WindowsInput.Native;

namespace MacroButtons.Services;

/// <summary>
/// Service for simulating keyboard input using AutoHotkey syntax.
/// </summary>
public class KeystrokeService
{
    private readonly IInputSimulator _inputSimulator;
    private readonly AutoHotkeyParser _parser;
    private readonly WindowHelper _windowHelper;
    private readonly TimeSpan _sendKeysDelay;

    public KeystrokeService(WindowHelper windowHelper, TimeSpan? sendKeysDelay = null)
    {
        _inputSimulator = new InputSimulator();
        _parser = new AutoHotkeyParser();
        _windowHelper = windowHelper;
        _sendKeysDelay = sendKeysDelay ?? TimeSpan.FromMilliseconds(30); // default 30ms
    }

    /// <summary>
    /// Sends keystrokes to the previously active window using AutoHotkey syntax.
    /// </summary>
    /// <param name="autoHotkeyString">Keystroke string (e.g., "^v", "+{Enter}", "abc")</param>
    public async Task SendKeysAsync(string autoHotkeyString)
    {
        if (string.IsNullOrWhiteSpace(autoHotkeyString))
            return;

        // Get the previously active window
        var previousWindow = _windowHelper.GetPreviousActiveWindow();

        // Parse AutoHotkey syntax
        var keySequence = _parser.Parse(autoHotkeyString);

        // Restore focus to previous window if we have one
        if (previousWindow != IntPtr.Zero)
        {
            _windowHelper.RestorePreviousWindow(previousWindow);
            await Task.Delay(_sendKeysDelay); // Configurable delay to ensure window is active
        }

        // Send the keystrokes
        foreach (var keyAction in keySequence)
        {
            ExecuteKeyAction(keyAction);
            await Task.Delay(5); // Small delay between key actions for reliability
        }
    }

    /// <summary>
    /// Executes a single key action.
    /// </summary>
    private void ExecuteKeyAction(KeyAction action)
    {
        switch (action.Type)
        {
            case KeyActionType.ModifierDown:
                _inputSimulator.Keyboard.KeyDown(action.VirtualKeyCode);
                break;
            case KeyActionType.ModifierUp:
                _inputSimulator.Keyboard.KeyUp(action.VirtualKeyCode);
                break;
            case KeyActionType.KeyPress:
                _inputSimulator.Keyboard.KeyPress(action.VirtualKeyCode);
                break;
        }
    }
}
