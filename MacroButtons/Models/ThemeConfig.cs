namespace MacroButtons.Models;

/// <summary>
/// Theme configuration for the application appearance.
/// </summary>
public class ThemeConfig
{
    /// <summary>
    /// Foreground color for text and borders (e.g., "darkgreen", "#00FF00").
    /// </summary>
    public string Foreground { get; set; } = "darkgreen";

    /// <summary>
    /// Background color for the window (e.g., "black", "#000000").
    /// </summary>
    public string Background { get; set; } = "black";
}
