using System.Windows.Media;

namespace MacroButtons.Helpers;

/// <summary>
/// Helper class for converting color strings to WPF Brush objects.
/// </summary>
public static class ColorConverter
{
    /// <summary>
    /// Parses a color string (named color or hex) into a WPF Brush.
    /// Returns a default brush if parsing fails.
    /// </summary>
    public static Brush ParseColor(string colorString, Brush? defaultBrush = null)
    {
        if (string.IsNullOrWhiteSpace(colorString))
            return defaultBrush ?? Brushes.White;

        try
        {
            // Try parsing as named color or hex color
            var color = (Color)System.Windows.Media.ColorConverter.ConvertFromString(colorString);
            return new SolidColorBrush(color);
        }
        catch
        {
            // Fallback to default or white
            return defaultBrush ?? Brushes.White;
        }
    }

    /// <summary>
    /// Parses a color string into a Color object.
    /// </summary>
    public static Color ParseColorValue(string colorString, Color defaultColor)
    {
        if (string.IsNullOrWhiteSpace(colorString))
            return defaultColor;

        try
        {
            return (Color)System.Windows.Media.ColorConverter.ConvertFromString(colorString);
        }
        catch
        {
            return defaultColor;
        }
    }
}
