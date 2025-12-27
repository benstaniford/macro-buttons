using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroButtons.Helpers;
using MacroButtons.Models;
using MacroButtons.Services;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Newtonsoft.Json.Linq;

namespace MacroButtons.ViewModels;

/// <summary>
/// View model for an individual button tile.
/// </summary>
public partial class ButtonTileViewModel : ViewModelBase
{
    private readonly ButtonItem? _config;
    private readonly CommandExecutionService _commandService;
    private readonly KeystrokeService _keystrokeService;
    private readonly WindowHelper _windowHelper;

    [ObservableProperty]
    private string _displayTitle = string.Empty;

    [ObservableProperty]
    private Brush _foreground = Brushes.DarkGreen;

    [ObservableProperty]
    private Brush _background = Brushes.Transparent;

    public bool IsEmpty { get; }
    public bool IsDynamic { get; }
    public bool HasAction { get; }

    /// <summary>
    /// Creates an empty tile (placeholder).
    /// </summary>
    public ButtonTileViewModel(Brush foreground, CommandExecutionService commandService, KeystrokeService keystrokeService, WindowHelper windowHelper)
    {
        _config = null;
        _commandService = commandService;
        _keystrokeService = keystrokeService;
        _windowHelper = windowHelper;
        IsEmpty = true;
        IsDynamic = false;
        HasAction = false;
        DisplayTitle = string.Empty;
        Foreground = foreground;
    }

    /// <summary>
    /// Creates a tile from a ButtonItem configuration.
    /// </summary>
    public ButtonTileViewModel(ButtonItem config, Brush foreground, CommandExecutionService commandService, KeystrokeService keystrokeService, WindowHelper windowHelper)
    {
        _config = config;
        _commandService = commandService;
        _keystrokeService = keystrokeService;
        _windowHelper = windowHelper;
        IsEmpty = false;
        IsDynamic = config.IsDynamicTitle;
        HasAction = config.HasAction;
        Foreground = foreground;

        // Set initial title
        if (config.IsStaticTitle)
        {
            DisplayTitle = config.GetStaticTitle();
        }
        else if (config.IsDynamicTitle)
        {
            DisplayTitle = "Loading...";
        }
    }

    /// <summary>
    /// Command executed when the tile is clicked.
    /// </summary>
    [RelayCommand]
    private async Task ClickAsync()
    {
        if (!HasAction || _config?.Action == null)
            return;

        // Restore cursor to the position it was before touching the screen
        // Do this IMMEDIATELY, before executing the action
        _windowHelper.RestorePreviousCursorPosition();

        try
        {
            switch (_config.Action.GetActionType())
            {
                case ActionType.Keypress:
                    await _keystrokeService.SendKeysAsync(_config.Action.Keypress!);
                    break;
                case ActionType.Python:
                    await _commandService.ExecutePythonAsync(_config.Action.Python!);
                    break;
                case ActionType.Executable:
                    await _commandService.ExecuteAsync(_config.Action.Exe!);
                    break;
            }
        }
        catch (Exception ex)
        {
            DisplayTitle = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Updates the dynamic title by executing the title command.
    /// Only applies to tiles with dynamic titles.
    /// Supports JSON format: {"text": "...", "fg": "#color", "bg": "#color"}
    /// Falls back to plain text if JSON parsing fails.
    /// </summary>
    public async Task UpdateDynamicTitleAsync()
    {
        if (!IsDynamic || _config?.Title == null)
            return;

        try
        {
            var titleDef = (TitleDefinition)_config.Title;
            string output;

            if (titleDef.IsPython)
            {
                output = await _commandService.ExecutePythonAsync(titleDef.Python!, captureOutput: true);
            }
            else if (titleDef.IsExecutable)
            {
                output = await _commandService.ExecuteFromListAsync(titleDef.Exe!, captureOutput: true);
            }
            else
            {
                return;
            }

            output = output.Trim();

            // Try to parse as JSON first
            if (TryParseJsonOutput(output, out var text, out var foreground, out var background))
            {
                DisplayTitle = text;
                if (foreground != null)
                    Foreground = foreground;
                if (background != null)
                    Background = background;
            }
            else
            {
                // Fallback to plain text
                DisplayTitle = output;
            }
        }
        catch (Exception ex)
        {
            DisplayTitle = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Attempts to parse JSON output in the format: {"text": "...", "fg": "#color", "bg": "#color"}
    /// </summary>
    private bool TryParseJsonOutput(string output, out string text, out Brush? foreground, out Brush? background)
    {
        text = string.Empty;
        foreground = null;
        background = null;

        if (string.IsNullOrWhiteSpace(output) || !output.TrimStart().StartsWith("{"))
            return false;

        try
        {
            var json = JObject.Parse(output);

            // Extract text (required)
            text = json["text"]?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(text))
                return false; // JSON must have "text" field

            // Extract optional foreground color
            var fgColor = json["fg"]?.ToString();
            if (!string.IsNullOrWhiteSpace(fgColor))
            {
                foreground = Helpers.ColorConverter.ParseColor(fgColor, null);
            }

            // Extract optional background color
            var bgColor = json["bg"]?.ToString();
            if (!string.IsNullOrWhiteSpace(bgColor))
            {
                background = Helpers.ColorConverter.ParseColor(bgColor, null);
            }

            return true;
        }
        catch
        {
            // Not valid JSON or parsing failed - will fall back to plain text
            return false;
        }
    }
}
