namespace MacroButtons.Models;

/// <summary>
/// Defines a dynamic submenu that executes a command and parses JSON output to generate submenu items.
/// Mirrors the structure of ActionDefinition/TitleDefinition for consistency.
/// </summary>
public class DynamicSubmenuDefinition
{
    /// <summary>
    /// Python command: [script_path, arg1, arg2, ...]
    /// </summary>
    public List<string>? Python { get; set; }

    /// <summary>
    /// Executable command: [exe_path, arg1, arg2, ...]
    /// </summary>
    public List<string>? Exe { get; set; }

    /// <summary>
    /// Inline PowerShell command.
    /// </summary>
    public string? PowerShell { get; set; }

    /// <summary>
    /// PowerShell script file path.
    /// </summary>
    public string? PowerShellScript { get; set; }

    /// <summary>
    /// Optional named parameters for PowerShell commands/scripts.
    /// </summary>
    public Dictionary<string, object>? PowerShellParameters { get; set; }

    /// <summary>
    /// Returns true if this is a Python command.
    /// </summary>
    public bool IsPython => Python != null && Python.Count > 0;

    /// <summary>
    /// Returns true if this is an executable command.
    /// </summary>
    public bool IsExecutable => Exe != null && Exe.Count > 0;

    /// <summary>
    /// Returns true if this is an inline PowerShell command.
    /// </summary>
    public bool IsPowerShell => !string.IsNullOrEmpty(PowerShell);

    /// <summary>
    /// Returns true if this is a PowerShell script file.
    /// </summary>
    public bool IsPowerShellScript => !string.IsNullOrEmpty(PowerShellScript);
}
