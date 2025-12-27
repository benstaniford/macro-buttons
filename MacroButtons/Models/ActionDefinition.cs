namespace MacroButtons.Models;

/// <summary>
/// Defines an action to execute when a button is clicked.
/// Only one of: Keypress, Python, or Exe should be specified.
/// </summary>
public class ActionDefinition
{
    /// <summary>
    /// Keypress action using AutoHotkey syntax (e.g., "^v" for Ctrl+V).
    /// </summary>
    public string? Keypress { get; set; }

    /// <summary>
    /// Python script to execute. First element is script path, rest are arguments.
    /// </summary>
    public List<string>? Python { get; set; }

    /// <summary>
    /// Executable path to run.
    /// </summary>
    public string? Exe { get; set; }

    /// <summary>
    /// Gets the type of action defined.
    /// </summary>
    public ActionType GetActionType()
    {
        if (!string.IsNullOrEmpty(Keypress))
        {
            // Check for special BACK navigation marker
            if (Keypress == "__NAVIGATE_BACK__")
                return ActionType.NavigateBack;
            return ActionType.Keypress;
        }
        if (Python != null && Python.Count > 0) return ActionType.Python;
        if (!string.IsNullOrEmpty(Exe)) return ActionType.Executable;
        return ActionType.None;
    }
}

/// <summary>
/// Types of actions that can be performed.
/// </summary>
public enum ActionType
{
    None,
    Keypress,
    Python,
    Executable,
    NavigateBack
}
