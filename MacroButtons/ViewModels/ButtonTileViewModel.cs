using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroButtons.Models;
using MacroButtons.Services;
using System.Windows.Media;

namespace MacroButtons.ViewModels;

/// <summary>
/// View model for an individual button tile.
/// </summary>
public partial class ButtonTileViewModel : ViewModelBase
{
    private readonly ButtonItem? _config;
    private readonly CommandExecutionService _commandService;
    private readonly KeystrokeService _keystrokeService;

    [ObservableProperty]
    private string _displayTitle = string.Empty;

    [ObservableProperty]
    private Brush _foreground = Brushes.DarkGreen;

    public bool IsEmpty { get; }
    public bool IsDynamic { get; }
    public bool HasAction { get; }

    /// <summary>
    /// Creates an empty tile (placeholder).
    /// </summary>
    public ButtonTileViewModel(Brush foreground, CommandExecutionService commandService, KeystrokeService keystrokeService)
    {
        _config = null;
        _commandService = commandService;
        _keystrokeService = keystrokeService;
        IsEmpty = true;
        IsDynamic = false;
        HasAction = false;
        DisplayTitle = string.Empty;
        Foreground = foreground;
    }

    /// <summary>
    /// Creates a tile from a ButtonItem configuration.
    /// </summary>
    public ButtonTileViewModel(ButtonItem config, Brush foreground, CommandExecutionService commandService, KeystrokeService keystrokeService)
    {
        _config = config;
        _commandService = commandService;
        _keystrokeService = keystrokeService;
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

            DisplayTitle = output.Trim();
        }
        catch (Exception ex)
        {
            DisplayTitle = $"Error: {ex.Message}";
        }
    }
}
