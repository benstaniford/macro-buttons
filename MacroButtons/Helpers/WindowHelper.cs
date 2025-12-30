using System.Runtime.InteropServices;

namespace MacroButtons.Helpers;

/// <summary>
/// Helper class for Win32 window management operations.
/// </summary>
public class WindowHelper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint MONITOR_DEFAULTTONULL = 0;

    private IntPtr _previousWindow = IntPtr.Zero;
    private POINT _previousCursorPosition;
    private IntPtr _ourMonitorHandle = IntPtr.Zero;
    private IntPtr _ourWindowHandle = IntPtr.Zero;

    /// <summary>
    /// Stores the currently active window handle and cursor position for later restoration.
    /// </summary>
    public void StorePreviousActiveWindow()
    {
        _previousWindow = GetForegroundWindow();
        GetCursorPos(out _previousCursorPosition);
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

    /// <summary>
    /// Gets the current cursor position in screen coordinates.
    /// </summary>
    public POINT GetCursorPosition()
    {
        GetCursorPos(out POINT point);
        return point;
    }

    /// <summary>
    /// Gets the previously stored cursor position.
    /// </summary>
    public POINT GetPreviousCursorPosition()
    {
        return _previousCursorPosition;
    }

    /// <summary>
    /// Sets the cursor position to the specified screen coordinates.
    /// </summary>
    public bool SetCursorPosition(int x, int y)
    {
        return SetCursorPos(x, y);
    }

    /// <summary>
    /// Sets the cursor position to the specified point.
    /// </summary>
    public bool SetCursorPosition(POINT point)
    {
        return SetCursorPos(point.X, point.Y);
    }

    /// <summary>
    /// Restores the cursor to the previously stored position.
    /// </summary>
    public bool RestorePreviousCursorPosition()
    {
        return SetCursorPos(_previousCursorPosition.X, _previousCursorPosition.Y);
    }

    /// <summary>
    /// Sets our window handle so we can track foreground window only when it's NOT our window.
    /// </summary>
    public void SetOurWindowHandle(IntPtr hWnd)
    {
        _ourWindowHandle = hWnd;
    }

    /// <summary>
    /// Sets the monitor that our window is on, so we can track cursor position only when it's NOT on our monitor.
    /// </summary>
    public void SetOurMonitorBounds(System.Drawing.Rectangle bounds)
    {
        var point = new POINT
        {
            X = bounds.Left + bounds.Width / 2,
            Y = bounds.Top + bounds.Height / 2
        };
        _ourMonitorHandle = MonitorFromPoint(point, MONITOR_DEFAULTTONULL);
    }

    /// <summary>
    /// Updates the stored cursor position if the cursor is not on our monitor.
    /// Call this periodically to track cursor position on other monitors.
    /// </summary>
    public void UpdateCursorPositionIfNotOnOurMonitor()
    {
        GetCursorPos(out POINT currentPos);
        IntPtr cursorMonitor = MonitorFromPoint(currentPos, MONITOR_DEFAULTTONULL);

        // Only save cursor position if it's on a different monitor than ours
        if (cursorMonitor != _ourMonitorHandle)
        {
            _previousCursorPosition = currentPos;
        }
    }

    /// <summary>
    /// Updates the stored foreground window if it's not our window.
    /// Call this periodically to track which window should receive keystrokes.
    /// </summary>
    public void UpdatePreviousWindowIfNotUs()
    {
        IntPtr currentForeground = GetForegroundWindow();

        // Only save foreground window if it's NOT our window
        if (currentForeground != IntPtr.Zero && currentForeground != _ourWindowHandle)
        {
            _previousWindow = currentForeground;
        }
    }

    /// <summary>
    /// Gets the current foreground window handle.
    /// </summary>
    public IntPtr GetCurrentForegroundWindow()
    {
        return GetForegroundWindow();
    }

    /// <summary>
    /// Gets the process name (e.g., "notepad.exe") from a window handle.
    /// Returns null if the process cannot be determined.
    /// </summary>
    public string? GetProcessNameFromWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            return null;

        try
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            if (processId == 0)
                return null;

            using var process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName + ".exe";
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the window class name from a window handle.
    /// Returns null if the class name cannot be determined.
    /// </summary>
    public string? GetWindowClassName(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            return null;

        try
        {
            var className = new System.Text.StringBuilder(256);
            int result = GetClassName(hWnd, className, className.Capacity);
            return result > 0 ? className.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if a window matches the activeWindow pattern.
    /// Pattern formats:
    /// - "ProcessName" or "ProcessName.exe" - matches process name only
    /// - "ProcessName|WindowClass" or "ProcessName.exe|WindowClass" - matches both process name and window class
    /// </summary>
    public bool WindowMatchesPattern(IntPtr hWnd, string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || hWnd == IntPtr.Zero)
            return false;

        var processName = GetProcessNameFromWindow(hWnd);
        if (processName == null)
            return false;

        // Check if pattern contains window class separator
        var parts = pattern.Split('|', 2);
        var expectedProcessName = parts[0].Trim();

        // Normalize the expected process name to include .exe if not present
        if (!expectedProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            expectedProcessName += ".exe";
        }

        // Match process name (case-insensitive)
        if (!processName.Equals(expectedProcessName, StringComparison.OrdinalIgnoreCase))
            return false;

        // If pattern includes window class, check it too
        if (parts.Length > 1)
        {
            var expectedClassName = parts[1].Trim();
            var windowClass = GetWindowClassName(hWnd);

            if (windowClass == null)
                return false;

            // Match window class (case-insensitive)
            if (!windowClass.Equals(expectedClassName, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}
