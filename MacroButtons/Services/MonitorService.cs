using System.Drawing;
using System.Windows.Forms;

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
    public Screen GetMonitorByIndex(int index)
    {
        var screens = Screen.AllScreens;
        return index >= 0 && index < screens.Length ? screens[index] : Screen.PrimaryScreen!;
    }

    /// <summary>
    /// Gets the bounds (position and size) of a specific monitor.
    /// </summary>
    public Rectangle GetMonitorBounds(int monitorIndex)
    {
        var monitor = GetMonitorByIndex(monitorIndex);
        return monitor.Bounds;
    }

    /// <summary>
    /// Gets the total number of monitors available.
    /// </summary>
    public int GetMonitorCount()
    {
        return Screen.AllScreens.Length;
    }
}
