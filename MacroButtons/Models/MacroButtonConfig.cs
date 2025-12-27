namespace MacroButtons.Models;

/// <summary>
/// Root configuration object for MacroButtons application.
/// </summary>
public class MacroButtonConfig
{
    public List<ButtonItem> Items { get; set; } = new();
    public ThemeConfig Theme { get; set; } = new();
    public GlobalConfig Global { get; set; } = new();
}
