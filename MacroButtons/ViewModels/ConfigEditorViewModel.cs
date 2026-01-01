using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroButtons.Models;
using MacroButtons.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace MacroButtons.ViewModels;

/// <summary>
/// View model for the configuration editor window.
/// </summary>
public partial class ConfigEditorViewModel : ViewModelBase
{
    private readonly ConfigurationService _configService;
    private readonly ProfileService _profileService;
    private string _profileName;

    [ObservableProperty]
    private MacroButtonConfig _config;

    [ObservableProperty]
    private ObservableCollection<ButtonTileEditorViewModel> _tiles = new();

    [ObservableProperty]
    private ButtonTileEditorViewModel? _selectedTile;

    [ObservableProperty]
    private int _rows;

    [ObservableProperty]
    private int _columns;

    public ConfigEditorViewModel(ProfileService profileService, ConfigurationService configService, string profileName)
    {
        _profileService = profileService;
        _configService = configService;
        _profileName = profileName;
        _config = configService.LoadConfiguration(profileName);

        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        Tiles.Clear();

        // Calculate grid dimensions based on item count
        int itemCount = Math.Max(_config.Items.Count, 15); // Minimum 3x5
        CalculateGridLayout(itemCount);

        // Create tile view models for all grid positions
        for (int i = 0; i < Rows * Columns; i++)
        {
            ButtonItem? item = i < _config.Items.Count ? _config.Items[i] : null;
            var tile = new ButtonTileEditorViewModel(i, item, _config);
            Tiles.Add(tile);
        }
    }

    private void CalculateGridLayout(int itemCount)
    {
        const int MIN_ROWS = 3;
        const int MIN_COLS = 5;
        const int MIN_TILES = MIN_ROWS * MIN_COLS; // 15

        if (itemCount <= MIN_TILES)
        {
            Rows = MIN_ROWS;
            Columns = MIN_COLS;
            return;
        }

        int rows = MIN_ROWS;
        int cols = MIN_COLS;

        while (rows * cols < itemCount)
        {
            int rowsAdded = rows - MIN_ROWS;
            int targetCols = MIN_COLS + (rowsAdded / 3);

            if (cols < targetCols)
            {
                cols++;
            }
            else
            {
                rows++;
            }
        }

        Rows = rows;
        Columns = cols;
    }

    [RelayCommand]
    private void AddRow()
    {
        Rows++;
        RefreshGrid();
    }

    [RelayCommand]
    private void DeleteRow()
    {
        if (Rows > 3)
        {
            Rows--;
            RefreshGrid();
        }
    }

    [RelayCommand]
    private void AddColumn()
    {
        Columns++;
        RefreshGrid();
    }

    [RelayCommand]
    private void DeleteColumn()
    {
        if (Columns > 5)
        {
            Columns--;
            RefreshGrid();
        }
    }

    private void RefreshGrid()
    {
        int newCount = Rows * Columns;
        int currentCount = Tiles.Count;

        if (newCount > currentCount)
        {
            // Add new tiles
            for (int i = currentCount; i < newCount; i++)
            {
                var tile = new ButtonTileEditorViewModel(i, null, _config);
                Tiles.Add(tile);
            }
        }
        else if (newCount < currentCount)
        {
            // Remove excess tiles
            while (Tiles.Count > newCount)
            {
                Tiles.RemoveAt(Tiles.Count - 1);
            }
        }
    }

    [RelayCommand]
    private void SelectTile(ButtonTileEditorViewModel tile)
    {
        // Deselect previous tile
        if (SelectedTile != null)
        {
            SelectedTile.IsSelected = false;
        }

        SelectedTile = tile;

        // Select new tile
        if (SelectedTile != null)
        {
            SelectedTile.IsSelected = true;
        }
    }

    [RelayCommand]
    private void ClearTile()
    {
        if (SelectedTile != null)
        {
            SelectedTile.Title = string.Empty;
            SelectedTile.SelectedActionType = "None";
            SelectedTile.ActionValue = string.Empty;
            SelectedTile.SelectedTheme = "default";
        }
    }

    [RelayCommand]
    private void Save()
    {
        // Save all tiles to their ButtonItems first
        foreach (var tile in Tiles)
        {
            tile.SaveToButtonItem();
        }

        // Collect non-empty tiles back into config.Items
        _config.Items.Clear();
        foreach (var tile in Tiles)
        {
            if (tile.ButtonItem != null)
            {
                _config.Items.Add(tile.ButtonItem);
            }
        }

        // Save configuration
        _configService.SaveConfiguration(_config, _profileName);

        MessageBox.Show("Configuration saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

        // Close the window
        Application.Current.Windows.OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        // Close window without saving
        Application.Current.Windows.OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)?.Close();
    }
}

/// <summary>
/// View model for an individual tile in the editor grid.
/// </summary>
public partial class ButtonTileEditorViewModel : ViewModelBase
{
    private readonly MacroButtonConfig _rootConfig;

    [ObservableProperty]
    private int _index;

    private ButtonItem? _buttonItem;

    [ObservableProperty]
    private string _displayTitle = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

    // Title properties
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                // Update display title immediately as user types
                DisplayTitle = value;
            }
        }
    }

    // Action properties
    [ObservableProperty]
    private string _selectedActionType = "None";

    [ObservableProperty]
    private string _actionValue = string.Empty;

    // Theme
    [ObservableProperty]
    private string _selectedTheme = "default";

    // Selection state
    [ObservableProperty]
    private bool _isSelected;

    public ButtonItem? ButtonItem
    {
        get => _buttonItem;
        set
        {
            _buttonItem = value;
            LoadFromButtonItem();
        }
    }

    public string[] ActionTypes { get; } = new[]
    {
        "None",
        "Keypress",
        "Executable",
        "Python",
        "PowerShell",
        "PowerShell Script",
        "Submenu"
    };

    public string[] Themes { get; } = new[]
    {
        "default",
        "prominent",
        "toggled"
    };

    public ButtonTileEditorViewModel(int index, ButtonItem? item, MacroButtonConfig rootConfig)
    {
        _index = index;
        _rootConfig = rootConfig;
        _buttonItem = item;
        LoadFromButtonItem();
    }

    private void LoadFromButtonItem()
    {
        if (_buttonItem == null)
        {
            IsEmpty = true;
            DisplayTitle = string.Empty;
            Title = string.Empty;
            SelectedActionType = "None";
            ActionValue = string.Empty;
            SelectedTheme = "default";
        }
        else
        {
            IsEmpty = false;
            DisplayTitle = _buttonItem.GetStaticTitle();
            Title = _buttonItem.IsStaticTitle ? _buttonItem.GetStaticTitle() : string.Empty;
            SelectedTheme = _buttonItem.Theme ?? "default";

            // Load action
            if (_buttonItem.Action != null)
            {
                var actionType = _buttonItem.Action.GetActionType();
                switch (actionType)
                {
                    case ActionType.Keypress:
                        SelectedActionType = "Keypress";
                        ActionValue = _buttonItem.Action.Keypress ?? string.Empty;
                        break;
                    case ActionType.Executable:
                        SelectedActionType = "Executable";
                        ActionValue = _buttonItem.Action.Exe ?? string.Empty;
                        break;
                    case ActionType.Python:
                        SelectedActionType = "Python";
                        ActionValue = _buttonItem.Action.Python != null ? string.Join(" ", _buttonItem.Action.Python) : string.Empty;
                        break;
                    case ActionType.PowerShell:
                        SelectedActionType = "PowerShell";
                        ActionValue = _buttonItem.Action.PowerShell ?? string.Empty;
                        break;
                    case ActionType.PowerShellScript:
                        SelectedActionType = "PowerShell Script";
                        ActionValue = _buttonItem.Action.PowerShellScript ?? string.Empty;
                        break;
                    default:
                        SelectedActionType = "None";
                        ActionValue = string.Empty;
                        break;
                }
            }
            else if (_buttonItem.HasSubmenu)
            {
                SelectedActionType = "Submenu";
                ActionValue = $"({_buttonItem.Items?.Count ?? 0} items)";
            }
            else
            {
                SelectedActionType = "None";
                ActionValue = string.Empty;
            }
        }
    }

    public void SaveToButtonItem()
    {
        // If title is empty and action is None, set ButtonItem to null
        if (string.IsNullOrWhiteSpace(Title) && SelectedActionType == "None")
        {
            _buttonItem = null;
            IsEmpty = true;
            DisplayTitle = string.Empty;
            return;
        }

        // Create or update ButtonItem
        if (_buttonItem == null)
        {
            _buttonItem = new ButtonItem();
        }

        // Set title
        _buttonItem.Title = string.IsNullOrWhiteSpace(Title) ? "Untitled" : Title;
        DisplayTitle = Title;

        // Set theme
        _buttonItem.Theme = SelectedTheme == "default" ? null : SelectedTheme;

        // Set action
        if (SelectedActionType == "None")
        {
            _buttonItem.Action = null;
        }
        else
        {
            if (_buttonItem.Action == null)
            {
                _buttonItem.Action = new ActionDefinition();
            }

            // Clear all action properties first
            _buttonItem.Action.Keypress = null;
            _buttonItem.Action.Exe = null;
            _buttonItem.Action.Python = null;
            _buttonItem.Action.PowerShell = null;
            _buttonItem.Action.PowerShellScript = null;

            switch (SelectedActionType)
            {
                case "Keypress":
                    _buttonItem.Action.Keypress = ActionValue;
                    break;
                case "Executable":
                    _buttonItem.Action.Exe = ActionValue;
                    break;
                case "Python":
                    // Split by spaces for simple cases
                    _buttonItem.Action.Python = string.IsNullOrWhiteSpace(ActionValue)
                        ? new List<string>()
                        : ActionValue.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "PowerShell":
                    _buttonItem.Action.PowerShell = ActionValue;
                    break;
                case "PowerShell Script":
                    _buttonItem.Action.PowerShellScript = ActionValue;
                    break;
            }
        }

        IsEmpty = false;
    }
}
