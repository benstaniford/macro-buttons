namespace MacroButtons.Models;

/// <summary>
/// Theme configuration containing an array of named themes.
/// </summary>
public class ThemeConfig
{
    /// <summary>
    /// List of available themes. Each theme has a name and color scheme.
    /// </summary>
    public List<Theme> Themes { get; set; } = new()
    {
        new Theme { Name = "default", Foreground = "darkgreen", Background = "black" }
    };

    /// <summary>
    /// Gets a theme by name. Returns the "default" theme if the specified theme is not found.
    /// </summary>
    public Theme GetTheme(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Themes.FirstOrDefault(t => t.Name == "default") ?? Themes.FirstOrDefault() ?? new Theme();

        return Themes.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? Themes.FirstOrDefault(t => t.Name == "default")
            ?? Themes.FirstOrDefault()
            ?? new Theme();
    }
}
