using System.Runtime.InteropServices;

namespace MacroButtons.Helpers;

/// <summary>
/// Helper class for Win32 window management operations.
/// </summary>
public class WindowHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    private IntPtr _previousWindow = IntPtr.Zero;

    /// <summary>
    /// Stores the currently active window handle for later restoration.
    /// </summary>
    public void StorePreviousActiveWindow()
    {
        _previousWindow = GetForegroundWindow();
    }

    /// <summary>
    /// Gets the previously stored active window handle.
    /// </summary>
    public IntPtr GetPreviousActiveWindow()
    {
        return _previousWindow;
    }

    /// <summary>
    /// Sets the foreground window to the specified handle.
    /// </summary>
    public bool RestorePreviousWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            return false;

        return SetForegroundWindow(hWnd);
    }

    /// <summary>
    /// Makes a window always appear on top of other windows.
    /// </summary>
    public bool MakeWindowTopmost(IntPtr hWnd)
    {
        return SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
    }
}
