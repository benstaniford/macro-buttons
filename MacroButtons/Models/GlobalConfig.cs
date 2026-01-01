using System.Text.RegularExpressions;

namespace MacroButtons.Models;

/// <summary>
/// Global configuration settings.
/// </summary>
public class GlobalConfig
{
    /// <summary>
    /// Refresh interval for dynamic titles (e.g., "30s", "1m", "2h").
    /// </summary>
    public string Refresh { get; set; } = "30s";

    /// <summary>
    /// DEPRECATED: Monitor index to display the window on.
    /// This setting is now stored in the Windows Registry and managed globally.
    /// This property is kept for backward compatibility with old configs but is ignored.
    /// </summary>
    [Obsolete("MonitorIndex is no longer used. Monitor selection is now managed in the Windows Registry.")]
    public int MonitorIndex { get; set; } = 0;

    /// <summary>
    /// Configuration for keystroke sending behavior (delay before sending, key hold duration).
    /// </summary>
    public SendKeysConfig SendKeys { get; set; } = new SendKeysConfig();

    /// <summary>
    /// Profile name for this configuration.
    /// </summary>
    public string ProfileName { get; set; } = "default";

    /// <summary>
    /// Optional active window trigger for automatic profile switching.
    /// The process name can be specified with or without the .exe extension.
    /// Format: "ProcessName" or "ProcessName.exe" - matches process name only
    ///         "ProcessName|WindowClass" or "ProcessName.exe|WindowClass" - matches both process and window class
    /// Examples: "firefox", "firefox.exe", "chrome|Chrome_WidgetWin_1"
    /// </summary>
    public string? ActiveWindow { get; set; }

    /// <summary>
    /// Enable or disable button click sound effects.
    /// </summary>
    public bool SoundEnabled { get; set; } = true;

    /// <summary>
    /// Sound volume (0.0 to 1.0). Default is 0.5 (50%).
    /// </summary>
    public float SoundVolume { get; set; } = 0.5f;

    /// <summary>
    /// Parses the refresh interval string into a TimeSpan.
    /// </summary>
    public TimeSpan GetRefreshInterval()
    {
        var match = Regex.Match(Refresh, @"^(\d+)([smh])$");
        if (!match.Success)
            return TimeSpan.FromSeconds(30); // default

        var value = int.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value;

        return unit switch
        {
            "s" => TimeSpan.FromSeconds(value),
            "m" => TimeSpan.FromMinutes(value),
            "h" => TimeSpan.FromHours(value),
            _ => TimeSpan.FromSeconds(30)
        };
    }
}
