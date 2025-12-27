using System.Text.RegularExpressions;

namespace MacroButtons.Models;

/// <summary>
/// Configuration for keystroke sending behavior.
/// </summary>
public class SendKeysConfig
{
    /// <summary>
    /// Delay before sending keys after switching to the target application (e.g., "10ms", "50ms", "1s").
    /// </summary>
    public string Delay { get; set; } = "30ms";

    /// <summary>
    /// Duration to hold down each key (e.g., "30ms", "100ms", "1s").
    /// </summary>
    public string Duration { get; set; } = "30ms";

    /// <summary>
    /// Parses the delay string into a TimeSpan.
    /// Supports milliseconds (ms) and seconds (s).
    /// </summary>
    public TimeSpan GetDelay()
    {
        return ParseTimeString(Delay, TimeSpan.FromMilliseconds(30));
    }

    /// <summary>
    /// Parses the duration string into a TimeSpan.
    /// Supports milliseconds (ms) and seconds (s).
    /// </summary>
    public TimeSpan GetDuration()
    {
        return ParseTimeString(Duration, TimeSpan.FromMilliseconds(30));
    }

    /// <summary>
    /// Parses a time string with units (ms, s) into a TimeSpan.
    /// </summary>
    private TimeSpan ParseTimeString(string timeString, TimeSpan defaultValue)
    {
        var match = Regex.Match(timeString, @"^(\d+)(ms|s)$");
        if (!match.Success)
            return defaultValue;

        var value = int.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value;

        return unit switch
        {
            "ms" => TimeSpan.FromMilliseconds(value),
            "s" => TimeSpan.FromSeconds(value),
            _ => defaultValue
        };
    }
}
