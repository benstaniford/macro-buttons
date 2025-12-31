namespace MacroButtons.Models;

/// <summary>
/// Defines an action to execute when a button is clicked.
/// Only one of: Keypress, Python, Exe, or Builtin should be specified.
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
    /// Builtin command to execute (e.g., "clock()", "quit()").
    /// </summary>
    public string? Builtin { get; set; }

    /// <summary>
    /// PowerShell command to execute inline (e.g., "Get-Date -Format 'HH:mm:ss'").
    /// </summary>
    public string? PowerShell { get; set; }

    /// <summary>
    /// PowerShell script file path to execute (e.g., "~/scripts/toggle-mic.ps1").
    /// </summary>
    public string? PowerShellScript { get; set; }

    /// <summary>
    /// Optional named parameters to pass to PowerShell script/command.
    /// </summary>
    public Dictionary<string, object>? PowerShellParameters { get; set; }

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
        if (!string.IsNullOrEmpty(PowerShell)) return ActionType.PowerShell;
        if (!string.IsNullOrEmpty(PowerShellScript)) return ActionType.PowerShellScript;
        if (!string.IsNullOrEmpty(Builtin)) return ActionType.Builtin;
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
    PowerShell,
    PowerShellScript,
    NavigateBack,
    Builtin
}
