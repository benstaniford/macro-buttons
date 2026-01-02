using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace MacroButtons.Services;

public class SampleActivationService
{
    private readonly LoggingService _loggingService;
    private const string RegistryKey = @"Software\MacroButtons\PendingSamples";
    private const string InstallPathKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}";

    public SampleActivationService(LoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public void ProcessPendingSamples()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
            if (key == null)
            {
                return;
            }

            var pendingSamples = key.GetValueNames().ToList();
            if (pendingSamples.Count == 0)
            {
                return;
            }

            _loggingService.Log($"Found {pendingSamples.Count} pending sample(s) to activate");

            var installPath = GetInstallPath();
            if (string.IsNullOrEmpty(installPath))
            {
                _loggingService.LogError("Could not determine MacroButtons installation path");
                return;
            }

            var samplesDir = Path.Combine(installPath, "samples");
            if (!Directory.Exists(samplesDir))
            {
                _loggingService.LogError($"Samples directory not found: {samplesDir}");
                return;
            }

            var destDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".macrobuttons");
            Directory.CreateDirectory(destDir);

            foreach (var sampleName in pendingSamples)
            {
                CopySample(samplesDir, destDir, sampleName);
            }

            DeleteRegistryKey();
            _loggingService.Log("Sample activation completed, registry key deleted");
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error processing pending samples: {ex.Message}", ex);
        }
    }

    private void CopySample(string samplesDir, string destDir, string sampleName)
    {
        try
        {
            var sampleSourceDir = Path.Combine(samplesDir, sampleName);
            if (!Directory.Exists(sampleSourceDir))
            {
                _loggingService.LogWarning($"Sample directory not found: {sampleSourceDir}");
                return;
            }

            var jsonFiles = Directory.GetFiles(sampleSourceDir, "*.json");
            if (jsonFiles.Length == 0)
            {
                _loggingService.LogWarning($"No JSON file found in sample: {sampleName}");
                return;
            }

            var sourceFile = jsonFiles[0];
            var destFileName = $".macrobuttons-{sampleName.ToLower()}.json";
            var destFile = Path.Combine(destDir, destFileName);

            File.Copy(sourceFile, destFile, true);
            _loggingService.Log($"Activated sample: {sampleName} -> {destFileName}");

            if (sampleName == "EliteDangerous")
            {
                var scriptFile = Path.Combine(sampleSourceDir, "elite-status");
                if (File.Exists(scriptFile))
                {
                    var destScript = Path.Combine(destDir, "elite-status");
                    File.Copy(scriptFile, destScript, true);
                    _loggingService.Log("Copied elite-status script");
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error copying sample {sampleName}: {ex.Message}", ex);
        }
    }

    private string? GetInstallPath()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\MacroButtons", false);
            var installLocation = key?.GetValue("InstallLocation")?.ToString();
            if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
            {
                return installLocation;
            }
        }
        catch
        {
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var defaultPath = Path.Combine(programFiles, "MacroButtons");
        return Directory.Exists(defaultPath) ? defaultPath : null;
    }

    private void DeleteRegistryKey()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKey(RegistryKey, false);
        }
        catch (Exception ex)
        {
            _loggingService.LogWarning($"Could not delete registry key: {ex.Message}");
        }
    }
}
