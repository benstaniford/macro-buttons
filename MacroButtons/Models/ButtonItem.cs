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
}
