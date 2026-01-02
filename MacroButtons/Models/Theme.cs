namespace MacroButtons.Models;

/// <summary>
/// Represents a named theme with foreground and background colors.
/// </summary>
public class Theme
{
    /// <summary>
    /// Name of the theme (e.g., "default", "toggled", "prominent").
    /// </summary>
    public string Name { get; set; } = "default";

    /// <summary>
    /// Foreground color for text and borders (e.g., "darkgreen", "#00FF00").
    /// </summary>
    public string Foreground { get; set; } = "darkgreen";

    /// <summary>
    /// Background color for the tile (e.g., "black", "#000000").
    /// </summary>
    public string Background { get; set; } = "black";

    /// <summary>
    /// Visual style for the theme (e.g., "retro", "modern").
    /// "retro" uses monospace font (Consolas) with square corners.
    /// "modern" uses sans-serif font (Segoe UI) with rounded corners.
    /// </summary>
    public string Style { get; set; } = "retro";
}
