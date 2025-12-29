namespace MacroButtons.Services;

/// <summary>
/// Service for executing builtin commands that don't require external dependencies.
/// </summary>
public class BuiltinService
{
    /// <summary>
    /// Executes a builtin action command.
    /// </summary>
    /// <param name="builtinName">Name of the builtin command (e.g., "quit()")</param>
    public void ExecuteBuiltin(string builtinName)
    {
        var normalized = NormalizeBuiltinName(builtinName);

        switch (normalized)
        {
            case "quit":
                System.Windows.Application.Current.Shutdown();
                break;

            default:
                throw new InvalidOperationException($"Unknown builtin action: {builtinName}");
        }
    }

    /// <summary>
    /// Gets the dynamic title value for a builtin command.
    /// Returns JSON string in format: {"text": "...", "fg": "#color", "bg": "#color"}
    /// Or plain text if no formatting is needed.
    /// </summary>
    /// <param name="builtinName">Name of the builtin command (e.g., "clock()")</param>
    /// <returns>Text or JSON string representing the title</returns>
    public string GetBuiltinTitleValue(string builtinName)
    {
        var normalized = NormalizeBuiltinName(builtinName);

        switch (normalized)
        {
            case "clock":
                return GetClockValue();

            default:
                throw new InvalidOperationException($"Unknown builtin title: {builtinName}");
        }
    }

    /// <summary>
    /// Normalizes a builtin command name by removing parentheses and converting to lowercase.
    /// </summary>
    /// <param name="builtinName">Raw builtin name (e.g., "clock()", "Clock", "CLOCK()")</param>
    /// <returns>Normalized name (e.g., "clock")</returns>
    private string NormalizeBuiltinName(string builtinName)
    {
        return builtinName.Replace("(", "").Replace(")", "").Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Gets the current time in HH:mm:ss format.
    /// Returns plain text. Theme should be set via ButtonItem.Theme property.
    /// </summary>
    private string GetClockValue()
    {
        var currentTime = DateTime.Now.ToString("HH:mm:ss");
        return currentTime;
    }
}
