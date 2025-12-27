using System.Text.RegularExpressions;

namespace MacroButtons.Models;

/// <summary>
/// Represents a single button/tile in the grid.
/// Title can be either a static string or a dynamic TitleDefinition.
/// Action can be null (info-only button) or an ActionDefinition.
/// </summary>
public class ButtonItem
{
    public object? Title { get; set; }
    public ActionDefinition? Action { get; set; }

    /// <summary>
    /// Optional refresh interval override for this tile (e.g., "100ms", "1s", "5m").
    /// If not specified, uses global refresh interval.
    /// Minimum is 100ms.
    /// </summary>
    public string? Refresh { get; set; }

    /// <summary>
    /// Returns true if the title is a static string.
    /// </summary>
    public bool IsStaticTitle => Title is string;

    /// <summary>
    /// Returns true if the title is a dynamic command (TitleDefinition).
    /// </summary>
    public bool IsDynamicTitle => Title is not string && Title != null;

    /// <summary>
    /// Returns true if this button has an action.
    /// </summary>
    public bool HasAction => Action != null;

    /// <summary>
    /// Gets the title as a string (for static titles).
    /// </summary>
    public string GetStaticTitle() => Title as string ?? string.Empty;

    /// <summary>
    /// Parses the refresh interval string into a TimeSpan.
    /// Supports formats: "100ms", "5s", "1m", "2h"
    /// Returns null if no refresh override is specified.
    /// </summary>
    public TimeSpan? GetRefreshInterval()
    {
        if (string.IsNullOrWhiteSpace(Refresh))
            return null;

        var match = Regex.Match(Refresh, @"^(\d+)(ms|s|m|h)$");
        if (!match.Success)
            return null;

        var value = int.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value;

        var interval = unit switch
        {
            "ms" => TimeSpan.FromMilliseconds(value),
            "s" => TimeSpan.FromSeconds(value),
            "m" => TimeSpan.FromMinutes(value),
            "h" => TimeSpan.FromHours(value),
            _ => (TimeSpan?)null
        };

        // Enforce minimum of 100ms
        if (interval.HasValue && interval.Value.TotalMilliseconds < 100)
            return TimeSpan.FromMilliseconds(100);

        return interval;
    }
}
