using System.Diagnostics;
using System.IO;
using System.Text;

namespace MacroButtons.Services;

/// <summary>
/// Service for executing external commands (Python scripts, executables) silently.
/// </summary>
public class CommandExecutionService
{
    /// <summary>
    /// Executes a Python script with arguments.
    /// </summary>
    /// <param name="pythonCommand">List where first element is script path, rest are arguments.</param>
    /// <param name="captureOutput">Whether to capture and return stdout.</param>
    /// <returns>The stdout output if captureOutput is true, otherwise empty string.</returns>
    public async Task<string> ExecutePythonAsync(List<string> pythonCommand, bool captureOutput = false)
    {
        if (pythonCommand == null || pythonCommand.Count == 0)
            throw new ArgumentException("Python command cannot be empty", nameof(pythonCommand));

        // Expand tilde in script path
        var scriptPath = ExpandPath(pythonCommand[0]);
        var arguments = pythonCommand.Skip(1).ToList();

        // Build full arguments: script path + arguments
        var fullArgs = new List<string> { scriptPath };
        fullArgs.AddRange(arguments);

        var argsString = string.Join(" ", fullArgs.Select(EscapeArgument));

        return await ExecuteProcessAsync("python", argsString, captureOutput);
    }

    /// <summary>
    /// Executes an executable with optional arguments.
    /// </summary>
    /// <param name="executable">Executable path.</param>
    /// <param name="args">Optional arguments.</param>
    /// <param name="captureOutput">Whether to capture and return stdout.</param>
    /// <returns>The stdout output if captureOutput is true, otherwise empty string.</returns>
    public async Task<string> ExecuteAsync(string executable, string[]? args = null, bool captureOutput = false)
    {
        if (string.IsNullOrWhiteSpace(executable))
            throw new ArgumentException("Executable path cannot be empty", nameof(executable));

        // Expand tilde in executable path
        var expandedExe = ExpandPath(executable);

        var arguments = args != null ? string.Join(" ", args.Select(EscapeArgument)) : string.Empty;
        return await ExecuteProcessAsync(expandedExe, arguments, captureOutput);
    }

    /// <summary>
    /// Executes an executable from a list format (first element is exe, rest are args).
    /// </summary>
    /// <param name="exeCommand">List where first element is exe path, rest are arguments.</param>
    /// <param name="captureOutput">Whether to capture and return stdout.</param>
    /// <returns>The stdout output if captureOutput is true, otherwise empty string.</returns>
    public async Task<string> ExecuteFromListAsync(List<string> exeCommand, bool captureOutput = false)
    {
        if (exeCommand == null || exeCommand.Count == 0)
            throw new ArgumentException("Executable command cannot be empty", nameof(exeCommand));

        var executable = exeCommand[0];
        var args = exeCommand.Skip(1).ToArray();

        return await ExecuteAsync(executable, args, captureOutput);
    }

    /// <summary>
    /// Core method for executing a process with complete window suppression.
    /// </summary>
    private async Task<string> ExecuteProcessAsync(string fileName, string arguments, bool captureOutput)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = captureOutput,
            RedirectStandardError = captureOutput,
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();

            string output = string.Empty;
            if (captureOutput)
            {
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                output = await outputTask;
                var error = await errorTask;

                // If there's stderr output and no stdout, use stderr
                if (string.IsNullOrWhiteSpace(output) && !string.IsNullOrWhiteSpace(error))
                {
                    output = error;
                }
            }
            else
            {
                await process.WaitForExitAsync();
            }

            return output;
        }
        catch (Exception ex)
        {
            // Return error message if command fails
            return captureOutput ? $"Error: {ex.Message}" : string.Empty;
        }
    }

    /// <summary>
    /// Expands tilde (~) to user profile path and resolves relative paths.
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

        // Convert forward slashes to backslashes on Windows
        path = path.Replace('/', Path.DirectorySeparatorChar);

        return path;
    }

    /// <summary>
    /// Properly escapes command line arguments containing spaces or special characters.
    /// </summary>
    private string EscapeArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg))
            return "\"\"";

        // If argument contains spaces, quotes, or special characters, wrap in quotes
        if (arg.Contains(' ') || arg.Contains('"') || arg.Contains('&') || arg.Contains('|'))
        {
            // Escape existing quotes
            arg = arg.Replace("\"", "\\\"");
            return $"\"{arg}\"";
        }

        return arg;
    }
}
