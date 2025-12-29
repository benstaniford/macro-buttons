namespace MacroButtons.Models;

/// <summary>
/// Root configuration object for MacroButtons application.
/// </summary>
public class MacroButtonConfig
{
    public List<ButtonItem> Items { get; set; } = new();
    public List<Theme> Themes { get; set; } = new()
    {
        new Theme { Name = "default", Foreground = "darkgreen", Background = "black" }
    };
    public GlobalConfig Global { get; set; } = new();

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
