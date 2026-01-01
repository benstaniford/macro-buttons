using MacroButtons.Models;
using Newtonsoft.Json;

namespace MacroButtons.Services;

/// <summary>
/// Service for executing dynamic submenu commands and parsing JSON output into ButtonItem lists.
/// </summary>
public class DynamicSubmenuService
{
    private readonly CommandExecutionService _commandService;
    private readonly PowerShellService _powershellService;
    private readonly LoggingService _loggingService;

    public DynamicSubmenuService(
        CommandExecutionService commandService,
        PowerShellService powershellService,
        LoggingService loggingService)
    {
        _commandService = commandService;
        _powershellService = powershellService;
        _loggingService = loggingService;
    }

    /// <summary>
    /// Executes the dynamic submenu command and parses JSON output into ButtonItem list.
    /// Returns list with error tile on failure. Empty list is valid (results in submenu with only BACK button).
    /// </summary>
    public async Task<List<ButtonItem>> ExecuteAndParseAsync(DynamicSubmenuDefinition definition)
    {
        try
        {
            // 1. Execute command based on type (Python/Exe/PowerShell/PowerShellScript)
            _loggingService.LogInfo("Executing dynamic submenu command");
            string output = await ExecuteCommandAsync(definition);

            // 2. Trim output
            output = output.Trim();
            if (string.IsNullOrEmpty(output))
            {
                _loggingService.LogInfo("Dynamic submenu returned empty output");
                return new List<ButtonItem>(); // Empty array -> submenu with only BACK button
            }

            // 3. Parse JSON array of ButtonItem objects
            _loggingService.LogInfo($"Parsing dynamic submenu JSON output (length: {output.Length} chars)");
            var items = JsonConvert.DeserializeObject<List<ButtonItem>>(output);

            if (items == null)
            {
                _loggingService.LogWarning("Dynamic submenu JSON deserialized to null");
                return CreateErrorTileList("JSON\ndeserialized\nto null");
            }

            // 4. Validate items (skip items without titles)
            var validItems = items.Where(item => item.Title != null).ToList();
            if (validItems.Count != items.Count)
            {
                int skippedCount = items.Count - validItems.Count;
                _loggingService.LogWarning($"Skipped {skippedCount} items without titles");
            }

            _loggingService.LogInfo($"Dynamic submenu generated {validItems.Count} valid items");
            return validItems;
        }
        catch (JsonException ex)
        {
            _loggingService.LogError($"Dynamic submenu JSON parse error: {ex.Message}");
            return CreateErrorTileList($"Invalid JSON:\n{ex.Message}");
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Dynamic submenu execution failed: {ex}");
            return CreateErrorTileList($"Error:\n{ex.Message}");
        }
    }

    /// <summary>
    /// Executes the command based on the definition type.
    /// </summary>
    private async Task<string> ExecuteCommandAsync(DynamicSubmenuDefinition definition)
    {
        if (definition.IsPython)
        {
            return await _commandService.ExecutePythonAsync(definition.Python!, captureOutput: true);
        }
        else if (definition.IsExecutable)
        {
            return await _commandService.ExecuteFromListAsync(definition.Exe!, captureOutput: true);
        }
        else if (definition.IsPowerShell)
        {
            return await _powershellService.ExecuteCommandAsync(
                definition.PowerShell!,
                definition.PowerShellParameters,
                captureOutput: true);
        }
        else if (definition.IsPowerShellScript)
        {
            return await _powershellService.ExecuteScriptFileAsync(
                definition.PowerShellScript!,
                definition.PowerShellParameters,
                captureOutput: true);
        }
        else
        {
            throw new InvalidOperationException("No valid command type specified in DynamicSubmenuDefinition");
        }
    }

    /// <summary>
    /// Creates a single-item list with an error tile.
    /// </summary>
    private List<ButtonItem> CreateErrorTileList(string errorMessage)
    {
        return new List<ButtonItem>
        {
            new ButtonItem
            {
                Title = errorMessage,
                Theme = "utility", // Yellow/red error theme
                Action = null
            }
        };
    }
}
