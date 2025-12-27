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
    /// Monitor index to display the window on (0 = first monitor).
    /// </summary>
    public int MonitorIndex { get; set; } = 0;

    /// <summary>
    /// Configuration for keystroke sending behavior (delay before sending, key hold duration).
    /// </summary>
    public SendKeysConfig SendKeys { get; set; } = new SendKeysConfig();

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
