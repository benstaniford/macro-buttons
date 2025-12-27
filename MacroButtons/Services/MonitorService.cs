namespace MacroButtons.Services;

/// <summary>
/// Service for managing multi-monitor detection and positioning.
/// </summary>
public class MonitorService
{
    /// <summary>
    /// Gets a specific monitor by index.
    /// Returns primary screen if index is invalid.
    /// </summary>
    public System.Windows.Forms.Screen GetMonitorByIndex(int index)
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        return index >= 0 && index < screens.Length ? screens[index] : System.Windows.Forms.Screen.PrimaryScreen!;
    }

    /// <summary>
    /// Gets the bounds (position and size) of a specific monitor.
    /// Uses WorkingArea for better DPI-aware coordinates.
    /// </summary>
    public System.Drawing.Rectangle GetMonitorBounds(int monitorIndex)
    {
        var monitor = GetMonitorByIndex(monitorIndex);
        // Try WorkingArea first (excludes taskbar), fall back to Bounds
        var area = monitor.WorkingArea;

        System.Diagnostics.Debug.WriteLine($"Monitor {monitorIndex} - Bounds: {monitor.Bounds}, WorkingArea: {area}");

        // For full-screen apps, we want Bounds, but let's see what we get
        return monitor.Bounds;
    }

    /// <summary>
    /// Gets the total number of monitors available.
    /// </summary>
    public int GetMonitorCount()
    {
        return System.Windows.Forms.Screen.AllScreens.Length;
    }

    /// <summary>
    /// Gets the index of the smallest monitor by area.
    /// Useful for automatically selecting a secondary display for macro buttons.
    /// </summary>
    public int GetSmallestMonitorIndex()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (screens.Length == 0)
            return 0;

        int smallestIndex = 0;
        int smallestArea = int.MaxValue;

        System.Diagnostics.Debug.WriteLine($"Detecting smallest monitor from {screens.Length} available:");

        for (int i = 0; i < screens.Length; i++)
        {
            var bounds = screens[i].Bounds;
            int area = bounds.Width * bounds.Height;

            System.Diagnostics.Debug.WriteLine($"  Monitor {i}: {bounds.Width}x{bounds.Height} = {area} pixels (Primary: {screens[i].Primary})");

            if (area < smallestArea)
            {
                smallestArea = area;
                smallestIndex = i;
            }
        }

        System.Diagnostics.Debug.WriteLine($"Selected smallest monitor: {smallestIndex} ({smallestArea} pixels)");
        return smallestIndex;
    }
}
