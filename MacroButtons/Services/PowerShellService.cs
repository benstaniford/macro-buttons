using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.IO;

namespace MacroButtons.Services;

/// <summary>
/// Service for executing PowerShell scripts and commands in-process using System.Management.Automation.
/// Creates isolated runspaces for each execution to ensure thread safety and state isolation.
/// </summary>
public class PowerShellService
{
    private readonly LoggingService? _loggingService;

    public PowerShellService(LoggingService? loggingService = null)
    {
        _loggingService = loggingService;
    }
    /// <summary>
    /// Executes a PowerShell script file.
    /// </summary>
    /// <param name="scriptPath">Path to .ps1 file (supports ~, %VAR%, and relative paths)</param>
    /// <param name="parameters">Optional named parameters to pass to the script</param>
    /// <param name="captureOutput">Whether to capture and return output (for dynamic titles)</param>
    /// <returns>Output text or error message</returns>
    public async Task<string> ExecuteScriptFileAsync(
        string scriptPath,
        Dictionary<string, object>? parameters = null,
        bool captureOutput = false)
    {
        if (string.IsNullOrWhiteSpace(scriptPath))
            throw new ArgumentException("Script path cannot be empty", nameof(scriptPath));

        var expandedPath = ExpandPath(scriptPath);

        // Check if file exists
        if (!File.Exists(expandedPath))
            return captureOutput ? $"Error: Script not found: {scriptPath}" : string.Empty;

        try
        {
            // Read script content asynchronously
            var scriptContent = await File.ReadAllTextAsync(expandedPath);
            return await ExecuteScriptAsync(scriptContent, parameters, captureOutput);
        }
        catch (Exception ex)
        {
            return captureOutput ? $"Error: {ex.Message}" : string.Empty;
        }
    }

    /// <summary>
    /// Executes a PowerShell command or script block directly.
    /// </summary>
    /// <param name="command">PowerShell command or script text</param>
    /// <param name="parameters">Optional named parameters</param>
    /// <param name="captureOutput">Whether to capture and return output</param>
    /// <returns>Output text or error message</returns>
    public async Task<string> ExecuteCommandAsync(
        string command,
        Dictionary<string, object>? parameters = null,
        bool captureOutput = false)
    {
        if (string.IsNullOrWhiteSpace(command))
            throw new ArgumentException("Command cannot be empty", nameof(command));

        return await ExecuteScriptAsync(command, parameters, captureOutput);
    }

    /// <summary>
    /// Core execution method. Creates an isolated runspace for each execution.
    /// </summary>
    private async Task<string> ExecuteScriptAsync(
        string scriptContent,
        Dictionary<string, object>? parameters,
        bool captureOutput)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Create initial session state with standard PowerShell modules
                // Use CreateDefault() to include ConvertTo-Json and other utility cmdlets
                // (CreateDefault2() is faster but lacks standard modules)
                var initialState = InitialSessionState.CreateDefault();

                // Create isolated runspace for this execution
                using var runspace = RunspaceFactory.CreateRunspace(initialState);
                runspace.Open();

                // Set working directory to user profile (matches CommandExecutionService)
                runspace.SessionStateProxy.Path.SetLocation(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                );

                // Create PowerShell instance within the runspace
                using var powershell = PowerShell.Create();
                powershell.Runspace = runspace;

                // Add the script/command
                powershell.AddScript(scriptContent);

                // Add parameters if provided
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        powershell.AddParameter(param.Key, param.Value);
                    }
                }

                // Execute the script
                var results = powershell.Invoke();

                if (captureOutput)
                {
                    // Capture output stream
                    var output = new StringBuilder();
                    foreach (var result in results)
                    {
                        if (result != null)
                            output.AppendLine(result.ToString());
                    }

                    // If no output, check for errors
                    if (output.Length == 0 && powershell.HadErrors)
                    {
                        foreach (var error in powershell.Streams.Error)
                        {
                            output.AppendLine($"Error: {error}");
                        }
                    }

                    var outputText = output.ToString().Trim();

                    // Log output
                    _loggingService?.LogCommandOutput("PowerShell",
                        scriptContent.Length > 100 ? scriptContent.Substring(0, 100) + "..." : scriptContent,
                        outputText,
                        powershell.HadErrors);

                    return outputText;
                }
                else
                {
                    // Action execution (non-capture mode)
                    // Still check for errors and log them
                    if (powershell.HadErrors)
                    {
                        var errorOutput = new StringBuilder();
                        foreach (var error in powershell.Streams.Error)
                        {
                            errorOutput.AppendLine(error.ToString());
                        }

                        _loggingService?.LogCommandOutput("PowerShell",
                            scriptContent.Length > 100 ? scriptContent.Substring(0, 100) + "..." : scriptContent,
                            errorOutput.ToString(),
                            isError: true);
                    }

                    return string.Empty;
                }
            }
            catch (RuntimeException ex)
            {
                // PowerShell terminating errors (syntax errors, cmdlet not found, etc.)
                _loggingService?.LogError($"PowerShell RuntimeException: {scriptContent.Substring(0, Math.Min(100, scriptContent.Length))}", ex);
                return captureOutput ? $"Error: {ex.Message}" : string.Empty;
            }
            catch (Exception ex)
            {
                // Other exceptions (should be rare)
                _loggingService?.LogError($"PowerShell Exception: {scriptContent.Substring(0, Math.Min(100, scriptContent.Length))}", ex);
                return captureOutput ? $"Error: {ex.Message}" : string.Empty;
            }
        });
    }

    /// <summary>
    /// Expands tilde (~) to user profile path, expands environment variables, and normalizes path separators.
    /// Supports: ~/, %VAR%, c:/path, c:\path
    /// </summary>
    private string ExpandPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        // Expand tilde
        if (path.StartsWith("~"))
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(userProfile, path.Substring(1).TrimStart('/', '\\'));
        }

        // Expand environment variables (e.g., %SystemRoot%, %ProgramFiles%, etc.)
        path = Environment.ExpandEnvironmentVariables(path);

        // Convert forward slashes to backslashes on Windows
        path = path.Replace('/', Path.DirectorySeparatorChar);

        return path;
    }
}
