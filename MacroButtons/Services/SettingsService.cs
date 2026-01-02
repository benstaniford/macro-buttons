using Microsoft.Win32;

namespace MacroButtons.Services;

/// <summary>
/// Profile switching mode.
/// </summary>
public enum ProfileMode
{
    /// <summary>
    /// Automatically switch profiles based on activeWindow patterns.
    /// </summary>
    Auto,

    /// <summary>
    /// Stay on the currently selected profile, ignoring activeWindow patterns.
    /// </summary>
    Fixed
}

/// <summary>
/// Service for managing application settings in the Windows Registry.
/// </summary>
public class SettingsService
{
    private const string RegistryKeyPath = @"Software\MacroButtons";
    private const string MonitorIndexValueName = "MonitorIndex";
    private const string ProfileModeValueName = "ProfileMode";

    /// <summary>
    /// Gets the monitor index from registry.
    /// Returns null if not set.
    /// </summary>
    public int? GetMonitorIndex()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key != null)
            {
                var value = key.GetValue(MonitorIndexValueName);
                if (value is int intValue)
                {
                    return intValue;
                }
            }
        }
        catch
        {
            // Ignore errors, return null
        }

        return null;
    }

    /// <summary>
    /// Sets the monitor index in registry.
    /// </summary>
    public void SetMonitorIndex(int monitorIndex)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            key.SetValue(MonitorIndexValueName, monitorIndex, RegistryValueKind.DWord);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save monitor index to registry: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Determines the default monitor index (smallest monitor).
    /// </summary>
    public int GetDefaultMonitorIndex(MonitorService monitorService)
    {
        var monitorCount = monitorService.GetMonitorCount();
        if (monitorCount == 0)
        {
            return 0;
        }

        // Find the smallest monitor by area
        int smallestIndex = 0;
        int smallestArea = int.MaxValue;

        for (int i = 0; i < monitorCount; i++)
        {
            var bounds = monitorService.GetMonitorBounds(i);
            int area = bounds.Width * bounds.Height;
            if (area < smallestArea)
            {
                smallestArea = area;
                smallestIndex = i;
            }
        }

        return smallestIndex;
    }

    /// <summary>
    /// Gets the monitor index, initializing to smallest monitor if not set.
    /// </summary>
    public int GetOrInitializeMonitorIndex(MonitorService monitorService)
    {
        var monitorIndex = GetMonitorIndex();
        if (monitorIndex.HasValue)
        {
            return monitorIndex.Value;
        }

        // Not set yet, initialize to smallest monitor
        var defaultIndex = GetDefaultMonitorIndex(monitorService);
        SetMonitorIndex(defaultIndex);
        return defaultIndex;
    }

    /// <summary>
    /// Gets the profile mode from registry.
    /// Defaults to Auto if not set.
    /// </summary>
    public ProfileMode GetProfileMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key != null)
            {
                var value = key.GetValue(ProfileModeValueName);
                if (value is int intValue && Enum.IsDefined(typeof(ProfileMode), intValue))
                {
                    return (ProfileMode)intValue;
                }
            }
        }
        catch
        {
            // Ignore errors, return default
        }

        return ProfileMode.Auto;
    }

    /// <summary>
    /// Sets the profile mode in registry.
    /// </summary>
    public void SetProfileMode(ProfileMode mode)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            key.SetValue(ProfileModeValueName, (int)mode, RegistryValueKind.DWord);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save profile mode to registry: {ex.Message}", ex);
        }
    }
}
