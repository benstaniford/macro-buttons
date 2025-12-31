using System.IO;
using System.Text;

namespace MacroButtons.Services;

/// <summary>
/// Service for logging application events, button presses, and errors.
/// Logs are written to %LOCALAPPDATA%\MacroButtons\macrobuttons.log
/// </summary>
public class LoggingService
{
    private static readonly object _lockObject = new object();
    private static string? _logFilePath;
    private const long MaxLogSizeBytes = 5 * 1024 * 1024; // 5 MB

    /// <summary>
    /// Gets the log file path. Creates the directory if it doesn't exist.
    /// </summary>
    public static string GetLogFilePath()
    {
        if (_logFilePath != null)
            return _logFilePath;

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDataFolder = Path.Combine(localAppData, "MacroButtons");

        // Ensure directory exists
        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
        }

        _logFilePath = Path.Combine(appDataFolder, "macrobuttons.log");
        return _logFilePath;
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public void LogInfo(string message)
    {
        Log("INFO", message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void LogWarning(string message)
    {
        Log("WARN", message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public void LogError(string message, Exception? exception = null)
    {
        if (exception != null)
        {
            message = $"{message}\nException: {exception.GetType().Name}: {exception.Message}\nStackTrace: {exception.StackTrace}";
        }
        Log("ERROR", message);
    }

    /// <summary>
    /// Logs a button press event.
    /// </summary>
    public void LogButtonPress(string buttonTitle, string actionType)
    {
        Log("BUTTON", $"Pressed: '{buttonTitle}' (Action: {actionType})");
    }

    /// <summary>
    /// Logs command output (stdout/stderr).
    /// </summary>
    public void LogCommandOutput(string commandType, string command, string output, bool isError = false)
    {
        var level = isError ? "STDERR" : "STDOUT";
        var truncatedOutput = output.Length > 500 ? output.Substring(0, 500) + "..." : output;
        Log(level, $"[{commandType}] {command}\nOutput: {truncatedOutput}");
    }

    /// <summary>
    /// Core logging method. Thread-safe.
    /// </summary>
    private void Log(string level, string message)
    {
        try
        {
            lock (_lockObject)
            {
                var logPath = GetLogFilePath();

                // Check file size and rotate if needed
                if (File.Exists(logPath))
                {
                    var fileInfo = new FileInfo(logPath);
                    if (fileInfo.Length > MaxLogSizeBytes)
                    {
                        // Rotate log file
                        var backupPath = Path.Combine(
                            Path.GetDirectoryName(logPath)!,
                            $"macrobuttons_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                        File.Move(logPath, backupPath);

                        // Clean up old backup files (keep last 5)
                        var backupFiles = Directory.GetFiles(Path.GetDirectoryName(logPath)!, "macrobuttons_*.log")
                            .OrderByDescending(f => f)
                            .Skip(5)
                            .ToArray();
                        foreach (var oldBackup in backupFiles)
                        {
                            try { File.Delete(oldBackup); } catch { }
                        }
                    }
                }

                // Write log entry
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level,-6}] {message}{Environment.NewLine}";

                File.AppendAllText(logPath, logEntry, Encoding.UTF8);
            }
        }
        catch
        {
            // Silently fail - logging should never crash the application
        }
    }

    /// <summary>
    /// Opens the log file in Notepad.
    /// </summary>
    public static void OpenLogInNotepad()
    {
        try
        {
            var logPath = GetLogFilePath();

            // Create empty log file if it doesn't exist
            if (!File.Exists(logPath))
            {
                File.WriteAllText(logPath, $"MacroButtons Log File - Created {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}");
            }

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{logPath}\"",
                UseShellExecute = false
            };

            System.Diagnostics.Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to open log file: {ex.Message}",
                "Open Log Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Clears the log file.
    /// </summary>
    public void ClearLog()
    {
        try
        {
            lock (_lockObject)
            {
                var logPath = GetLogFilePath();
                if (File.Exists(logPath))
                {
                    File.WriteAllText(logPath, $"Log cleared at {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}");
                }
            }
        }
        catch
        {
            // Silently fail
        }
    }
}
