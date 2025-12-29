using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroButtons.Helpers;
using MacroButtons.Models;
using MacroButtons.Services;
using System.Windows.Media;
using System.Windows.Threading;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Newtonsoft.Json.Linq;
using ColorConverter = MacroButtons.Helpers.ColorConverter;

namespace MacroButtons.ViewModels;

/// <summary>
/// View model for an individual button tile.
/// </summary>
public partial class ButtonTileViewModel : ViewModelBase, IDisposable
{
    private readonly ButtonItem? _config;
    private readonly MacroButtonConfig _rootConfig;
    private readonly CommandExecutionService _commandService;
    private readonly KeystrokeService _keystrokeService;
    private readonly WindowHelper _windowHelper;
    private readonly BuiltinService _builtinService;
    private DispatcherTimer? _refreshTimer;

    // Navigation callbacks
    private readonly Action<List<ButtonItem>>? _onNavigateToSubmenu;
    private readonly Action? _onNavigateBack;

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
    public ButtonTileViewModel(MacroButtonConfig rootConfig, CommandExecutionService commandService, KeystrokeService keystrokeService, WindowHelper windowHelper)
    {
        _config = null;
        _rootConfig = rootConfig;
        _commandService = commandService;
        _keystrokeService = keystrokeService;
        _windowHelper = windowHelper;
        _builtinService = new BuiltinService();
        _onNavigateToSubmenu = null;
        _onNavigateBack = null;
        IsEmpty = true;
        IsDynamic = false;
        HasAction = false;
        DisplayTitle = string.Empty;

        // Apply default theme for empty tiles
        var theme = rootConfig.GetTheme("default");
        Foreground = ColorConverter.ParseColor(theme.Foreground, Brushes.DarkGreen);
        Background = ColorConverter.ParseColor(theme.Background, Brushes.Transparent);
    }

    /// <summary>
    /// Creates a tile from a ButtonItem configuration.
    /// </summary>
    public ButtonTileViewModel(
        ButtonItem config,
        MacroButtonConfig rootConfig,
        TimeSpan globalRefreshInterval,
        CommandExecutionService commandService,
        KeystrokeService keystrokeService,
        WindowHelper windowHelper,
        Action<List<ButtonItem>>? onNavigateToSubmenu = null,
        Action? onNavigateBack = null)
    {
        _config = config;
        _rootConfig = rootConfig;
        _commandService = commandService;
        _keystrokeService = keystrokeService;
        _windowHelper = windowHelper;
        _builtinService = new BuiltinService();
        _onNavigateToSubmenu = onNavigateToSubmenu;
        _onNavigateBack = onNavigateBack;
        IsEmpty = false;
        IsDynamic = config.IsDynamicTitle;
        HasAction = config.HasAction || config.HasSubmenu;  // Include submenu buttons

        // Apply theme based on priority:
        // 1. If dynamic title, check TitleDefinition.Theme
        // 2. Otherwise, use ButtonItem.Theme
        // 3. Fall back to "default"
        string? themeName = null;
        if (config.IsDynamicTitle && config.Title is TitleDefinition titleDef)
        {
            themeName = titleDef.Theme ?? config.Theme;
        }
        else
        {
            themeName = config.Theme;
        }

        var theme = rootConfig.GetTheme(themeName);
        Foreground = ColorConverter.ParseColor(theme.Foreground, Brushes.DarkGreen);
        Background = ColorConverter.ParseColor(theme.Background, Brushes.Transparent);

        // Set initial title
        if (config.IsStaticTitle)
        {
            DisplayTitle = config.GetStaticTitle();
        }
        else if (config.IsDynamicTitle)
        {
            DisplayTitle = "Loading...";

            // Start the refresh timer for dynamic titles
            StartRefreshTimer(globalRefreshInterval);
        }
    }

    /// <summary>
    /// Starts the refresh timer for dynamic titles.
    /// Uses tile-specific refresh interval if specified, otherwise uses global interval.
    /// </summary>
    private void StartRefreshTimer(TimeSpan globalRefreshInterval)
    {
        if (!IsDynamic || _config?.Title == null)
            return;

        // Determine refresh interval (tile override or global)
        var titleDef = (TitleDefinition)_config.Title;
        var interval = titleDef.GetRefreshInterval() ?? globalRefreshInterval;

        // Initial refresh
        _ = UpdateDynamicTitleAsync();

        // Start periodic refresh
        _refreshTimer = new DispatcherTimer
        {
            Interval = interval
        };
        _refreshTimer.Tick += async (s, e) => await UpdateDynamicTitleAsync();
        _refreshTimer.Start();
    }

    /// <summary>
    /// Command executed when the tile is clicked.
    /// </summary>
    [RelayCommand]
    private async Task ClickAsync()
    {
        if (_config == null)
            return;

        // Restore cursor to the position it was before touching the screen
        // Do this IMMEDIATELY, before executing the action
        _windowHelper.RestorePreviousCursorPosition();

        try
        {
            // PHASE 1: Execute action if present
            if (_config.HasAction && _config.Action != null)
            {
                var actionType = _config.Action.GetActionType();

                // Handle BACK navigation (exclusive - return immediately)
                if (actionType == ActionType.NavigateBack)
                {
                    _onNavigateBack?.Invoke();
                    return;
                }

                // Execute regular action
                switch (actionType)
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
                    case ActionType.Builtin:
                        _builtinService.ExecuteBuiltin(_config.Action.Builtin!);
                        break;
                }
            }

            // PHASE 2: Navigate to submenu if present (happens AFTER action)
            if (_config.HasSubmenu && _config.Items != null)
            {
                _onNavigateToSubmenu?.Invoke(_config.Items);
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
            else if (titleDef.IsBuiltin)
            {
                output = _builtinService.GetBuiltinTitleValue(titleDef.Builtin!);
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
                // Fallback to plain text - reset to theme colors
                DisplayTitle = output;

                // Reset to the base theme (from TitleDefinition.Theme or ButtonItem.Theme)
                string? themeName = null;
                if (_config?.IsDynamicTitle == true && _config.Title is TitleDefinition dynamicTitleDef)
                {
                    themeName = dynamicTitleDef.Theme ?? _config.Theme;
                }
                else if (_config != null)
                {
                    themeName = _config.Theme;
                }

                var theme = _rootConfig.GetTheme(themeName);
                Foreground = ColorConverter.ParseColor(theme.Foreground, Brushes.DarkGreen);
                Background = ColorConverter.ParseColor(theme.Background, Brushes.Transparent);
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

    public void Dispose()
    {
        _refreshTimer?.Stop();
        _refreshTimer = null;
    }
}
