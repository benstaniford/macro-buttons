using System.Linq;
using MacroButtons.Helpers;
using MacroButtons.Models;
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
    private readonly TimeSpan _delay;
    private readonly TimeSpan _duration;

    public KeystrokeService(WindowHelper windowHelper, SendKeysConfig? sendKeysConfig = null)
    {
        _inputSimulator = new InputSimulator();
        _parser = new AutoHotkeyParser();
        _windowHelper = windowHelper;

        if (sendKeysConfig != null)
        {
            _delay = sendKeysConfig.GetDelay();
            _duration = sendKeysConfig.GetDuration();
        }
        else
        {
            _delay = TimeSpan.FromMilliseconds(10); // default 10ms
            _duration = TimeSpan.FromMilliseconds(30); // default 30ms
        }
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

        // Check if this sequence contains Windows key modifiers
        bool hasWindowsKey = keySequence.Any(k =>
            k.VirtualKeyCode == VirtualKeyCode.LWIN ||
            k.VirtualKeyCode == VirtualKeyCode.RWIN);

        // Restore focus to previous window ONLY if not using Windows key
        // (Windows key shortcuts are system-level and don't need window focus)
        if (!hasWindowsKey && previousWindow != IntPtr.Zero)
        {
            _windowHelper.RestorePreviousWindow(previousWindow);
            await Task.Delay(_delay); // Configurable delay to ensure window is active
        }

        // Send the keystrokes
        foreach (var keyAction in keySequence)
        {
            await ExecuteKeyActionAsync(keyAction);
            await Task.Delay(5); // Small delay between key actions for reliability
        }
    }

    /// <summary>
    /// Executes a single key action with configurable duration for key presses.
    /// </summary>
    private async Task ExecuteKeyActionAsync(KeyAction action)
    {
        switch (action.Type)
        {
            case KeyActionType.ModifierDown:
                _inputSimulator.Keyboard.KeyDown(action.VirtualKeyCode);
                // Add extra delay for Windows key to ensure it's recognized
                if (action.VirtualKeyCode == VirtualKeyCode.LWIN || action.VirtualKeyCode == VirtualKeyCode.RWIN)
                {
                    await Task.Delay(50); // Extra delay for Windows key
                }
                break;
            case KeyActionType.ModifierUp:
                _inputSimulator.Keyboard.KeyUp(action.VirtualKeyCode);
                break;
            case KeyActionType.KeyPress:
                // Hold key down for configured duration
                _inputSimulator.Keyboard.KeyDown(action.VirtualKeyCode);
                await Task.Delay(_duration);
                _inputSimulator.Keyboard.KeyUp(action.VirtualKeyCode);
                break;
        }
    }
}
