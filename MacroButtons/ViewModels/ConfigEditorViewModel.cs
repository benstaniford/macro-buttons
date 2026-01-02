using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroButtons.Models;
using MacroButtons.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;

namespace MacroButtons.ViewModels;

/// <summary>
/// Stores navigation context when drilling into submenus.
/// </summary>
internal class NavigationContext
{
    public List<ButtonItem> Items { get; set; } = new();
    public string ParentTitle { get; set; } = "";
}

/// <summary>
/// View model for the configuration editor window.
/// </summary>
public partial class ConfigEditorViewModel : ViewModelBase
{
    private readonly ConfigurationService _configService;
    private readonly ProfileService _profileService;
    private string _profileName;

    // Navigation state for submenu editing
    private Stack<NavigationContext> _navigationStack = new();
    private List<ButtonItem> _currentItems;

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

    [ObservableProperty]
    private string _breadcrumb = "Root Menu";

    public bool IsAtRootLevel => _navigationStack.Count == 0;

    public ConfigEditorViewModel(ProfileService profileService, ConfigurationService configService, string profileName)
    {
        _profileService = profileService;
        _configService = configService;
        _profileName = profileName;
        _config = configService.LoadConfiguration(profileName);

        // Initialize at root level
        _currentItems = _config.Items;

        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        Tiles.Clear();

        int startIndex = 0;
        
        // If in a submenu, add a BACK button as the first tile
        if (!IsAtRootLevel)
        {
            var backButton = new ButtonItem
            {
                Title = "← BACK",
                Action = null,
                Theme = "prominent"
            };
            var backTile = new ButtonTileEditorViewModel(0, backButton, _config);
            backTile.IsBackButton = true;
            Tiles.Add(backTile);
            startIndex = 1;
        }

        // Calculate grid dimensions based on item count (including BACK button if present)
        int totalItems = _currentItems.Count + (IsAtRootLevel ? 0 : 1);
        int itemCount = Math.Max(totalItems, 15); // Minimum 3x5
        CalculateGridLayout(itemCount);

        // Create tile view models for all grid positions
        for (int i = startIndex; i < Rows * Columns; i++)
        {
            int itemIndex = i - startIndex;
            ButtonItem? item = itemIndex < _currentItems.Count ? _currentItems[itemIndex] : null;
            var tile = new ButtonTileEditorViewModel(i, item, _config);
            Tiles.Add(tile);
        }

        // Update breadcrumb
        UpdateBreadcrumb();
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
    private void AddItem()
    {
        // Add a new empty tile
        var newTile = new ButtonTileEditorViewModel(Tiles.Count, null, _config);
        Tiles.Add(newTile);
        
        // Recalculate grid layout based on new item count
        CalculateGridLayout(Tiles.Count);
    }



    [RelayCommand]
    private void SelectTile(ButtonTileEditorViewModel tile)
    {
        // Don't allow selecting BACK button
        if (tile.IsBackButton)
        {
            // Clicking BACK button should navigate back
            NavigateBack();
            return;
        }
        
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

        // Check if double-click on submenu - if so, drill down
        // For now, we'll add a separate command for drilling down
    }

    [RelayCommand]
    private void DrillIntoSubmenu()
    {
        if (SelectedTile == null || SelectedTile.SelectedActionType != "Submenu")
            return;

        // Only allow drilling into static submenus, not dynamic ones
        if (SelectedTile.ButtonItem?.HasStaticSubmenu != true)
            return;

        // Save current state to navigation stack
        _navigationStack.Push(new NavigationContext
        {
            Items = _currentItems,
            ParentTitle = SelectedTile.Title
        });

        // Get submenu items (create if needed)
        if (SelectedTile.ButtonItem?.Items == null)
        {
            SelectedTile.ButtonItem.Items = new List<ButtonItem>();
        }

        // Navigate to submenu
        _currentItems = SelectedTile.ButtonItem.Items;

        // Reload configuration with new items
        LoadConfiguration();
    }

    [RelayCommand]
    private void NavigateBack()
    {
        if (_navigationStack.Count == 0)
            return;

        // Pop previous level from stack
        var context = _navigationStack.Pop();
        _currentItems = context.Items;

        // Reload configuration
        LoadConfiguration();
    }

    private void UpdateBreadcrumb()
    {
        if (_navigationStack.Count == 0)
        {
            Breadcrumb = "Root Menu";
        }
        else
        {
            var path = new List<string> { "Root" };
            foreach (var ctx in _navigationStack.Reverse())
            {
                path.Add(ctx.ParentTitle);
            }
            Breadcrumb = string.Join(" > ", path);
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
        // Save all tiles to their ButtonItems first (skip BACK button if present)
        foreach (var tile in Tiles)
        {
            if (tile.IsBackButton)
                continue;
            
            tile.SaveToButtonItem();
        }

        // Collect all tiles back into current items list
        // Empty tiles will have "" title to maintain grid positioning
        _currentItems.Clear();
        foreach (var tile in Tiles)
        {
            if (tile.IsBackButton)
                continue;
                
            if (tile.ButtonItem != null)
            {
                _currentItems.Add(tile.ButtonItem);
            }
            else
            {
                // Add empty item with "" title to maintain grid structure
                _currentItems.Add(new ButtonItem { Title = "" });
            }
        }

        // Only save to file if at root level
        if (IsAtRootLevel)
        {
            _configService.SaveConfiguration(_config, _profileName);
        }
    }

    [RelayCommand]
    private void SaveAndClose()
    {
        // Navigate back to root if in submenu
        while (!IsAtRootLevel)
        {
            // Save current submenu level
            SaveCurrentLevel();
            
            // Pop back
            var context = _navigationStack.Pop();
            _currentItems = context.Items;
        }

        // Save at root level (writes to file)
        Save();

        // Close the window
        System.Windows.Application.Current.Windows.OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)?.Close();
    }

    private void SaveCurrentLevel()
    {
        // Save all tiles to their ButtonItems (skip BACK button if present)
        foreach (var tile in Tiles)
        {
            if (tile.IsBackButton)
                continue;
                
            tile.SaveToButtonItem();
        }

        // Update current items list
        _currentItems.Clear();
        foreach (var tile in Tiles)
        {
            if (tile.IsBackButton)
                continue;
                
            if (tile.ButtonItem != null)
            {
                _currentItems.Add(tile.ButtonItem);
            }
            else
            {
                _currentItems.Add(new ButtonItem { Title = "" });
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        // Close window without saving
        System.Windows.Application.Current.Windows.OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)?.Close();
    }

    /// <summary>
    /// Moves a tile from its current position to a target position, shifting other tiles.
    /// </summary>
    public void MoveTile(ButtonTileEditorViewModel sourceTile, ButtonTileEditorViewModel targetTile)
    {
        // Don't allow moving to/from BACK button
        if (sourceTile.IsBackButton || targetTile.IsBackButton)
            return;

        int sourceIndex = Tiles.IndexOf(sourceTile);
        int targetIndex = Tiles.IndexOf(targetTile);

        if (sourceIndex == -1 || targetIndex == -1 || sourceIndex == targetIndex)
            return;

        // Remove the source tile
        Tiles.RemoveAt(sourceIndex);

        // Insert at the target position
        Tiles.Insert(targetIndex, sourceTile);

        // Update indices for all tiles
        for (int i = 0; i < Tiles.Count; i++)
        {
            Tiles[i].Index = i;
        }
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
                // Update display title based on whether it looks like JSON
                string trimmed = value?.Trim() ?? "";
                if (trimmed.StartsWith("{") && trimmed.Contains("\""))
                {
                    DisplayTitle = "[Dynamic Title]";
                }
                else
                {
                    DisplayTitle = value;
                }
            }
        }
    }

    // Action properties
    private string _selectedActionType = "None";
    public string SelectedActionType
    {
        get => _selectedActionType;
        set
        {
            if (SetProperty(ref _selectedActionType, value))
            {
                OnPropertyChanged(nameof(IsActionValueEnabled));
            }
        }
    }

    [ObservableProperty]
    private string _actionValue = string.Empty;

    /// <summary>
    /// Returns true if the action value textbox should be enabled.
    /// Disabled for submenu action type (but enabled for Dynamic Submenu).
    /// </summary>
    public bool IsActionValueEnabled => SelectedActionType != "Submenu";

    // Theme
    [ObservableProperty]
    private string _selectedTheme = "default";

    // Selection state
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Indicates whether this tile is currently being dragged.
    /// </summary>
    [ObservableProperty]
    private bool _isDragging;

    /// <summary>
    /// Returns true if this is the BACK button tile (uneditable).
    /// </summary>
    [ObservableProperty]
    private bool _isBackButton;

    /// <summary>
    /// Returns true if this tile represents a static submenu that can be drilled into.
    /// </summary>
    public bool IsStaticSubmenu => SelectedActionType == "Submenu";

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
        "Builtin",
        "Submenu",
        "Dynamic Submenu"
    };

    public string[] Themes { get; private set; }

    public ButtonTileEditorViewModel(int index, ButtonItem? item, MacroButtonConfig rootConfig)
    {
        _index = index;
        _rootConfig = rootConfig;
        _buttonItem = item;

        // Populate Themes array from rootConfig
        if (rootConfig?.Themes != null && rootConfig.Themes.Count > 0)
        {
            Themes = rootConfig.Themes.Select(t => t.Name).ToArray();
        }
        else
        {
            // Fallback to default theme names if config has no themes
            Themes = new[] { "default", "prominent", "toggled" };
        }

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
            
            // Handle dynamic titles - serialize to JSON
            if (_buttonItem.IsDynamicTitle)
            {
                var titleDef = _buttonItem.Title as TitleDefinition;
                if (titleDef != null)
                {
                    // Serialize the TitleDefinition object to JSON for display/editing
                    var titleJson = Newtonsoft.Json.JsonConvert.SerializeObject(titleDef, Newtonsoft.Json.Formatting.Indented);
                    Title = titleJson;
                    DisplayTitle = "[Dynamic Title]";
                }
                else
                {
                    Title = string.Empty;
                    DisplayTitle = string.Empty;
                }
            }
            else
            {
                // Static title
                DisplayTitle = _buttonItem.GetStaticTitle();
                Title = _buttonItem.GetStaticTitle();
            }
            
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
                    case ActionType.Builtin:
                        SelectedActionType = "Builtin";
                        ActionValue = _buttonItem.Action.Builtin ?? string.Empty;
                        break;
                    default:
                        SelectedActionType = "None";
                        ActionValue = string.Empty;
                        break;
                }
            }
            else if (_buttonItem.HasDynamicSubmenu)
            {
                // Dynamic submenu - serialize to JSON for editing
                SelectedActionType = "Dynamic Submenu";
                var dynamicSubmenuJson = JsonConvert.SerializeObject(_buttonItem.DynamicSubmenu, Formatting.Indented);
                ActionValue = dynamicSubmenuJson;
            }
            else if (_buttonItem.HasStaticSubmenu)
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

        // Set title - try to parse as JSON (dynamic title), otherwise treat as static
        string trimmedTitle = Title?.Trim() ?? "";
        if (trimmedTitle.StartsWith("{") && trimmedTitle.Contains("\""))
        {
            // Looks like JSON - try to parse as TitleDefinition
            try
            {
                var titleDef = Newtonsoft.Json.JsonConvert.DeserializeObject<TitleDefinition>(trimmedTitle);
                _buttonItem.Title = titleDef;
                DisplayTitle = "[Dynamic Title]";
            }
            catch
            {
                // Failed to parse as JSON, treat as static title
                _buttonItem.Title = string.IsNullOrWhiteSpace(Title) ? "Untitled" : Title;
                DisplayTitle = Title;
            }
        }
        else
        {
            // Static title
            _buttonItem.Title = string.IsNullOrWhiteSpace(Title) ? "Untitled" : Title;
            DisplayTitle = Title;
        }

        // Set theme
        _buttonItem.Theme = SelectedTheme == "default" ? null : SelectedTheme;

        // Set action or submenu
        if (SelectedActionType == "Submenu")
        {
            // Ensure Items list exists for submenu
            if (_buttonItem.Items == null)
            {
                _buttonItem.Items = new List<ButtonItem>();
            }
            // Clear action and dynamic submenu when it's a static submenu
            _buttonItem.Action = null;
            _buttonItem.DynamicSubmenu = null;
        }
        else if (SelectedActionType == "Dynamic Submenu")
        {
            // Parse JSON to DynamicSubmenuDefinition
            try
            {
                var dynamicSubmenu = JsonConvert.DeserializeObject<DynamicSubmenuDefinition>(ActionValue);
                _buttonItem.DynamicSubmenu = dynamicSubmenu;
            }
            catch
            {
                // If JSON parsing fails, create empty DynamicSubmenuDefinition
                _buttonItem.DynamicSubmenu = new DynamicSubmenuDefinition();
            }
            // Clear action and static submenu
            _buttonItem.Action = null;
            _buttonItem.Items = null;
        }
        else if (SelectedActionType == "None")
        {
            _buttonItem.Action = null;
            // Clear submenu if switching away from submenu
            _buttonItem.Items = null;
            _buttonItem.DynamicSubmenu = null;
        }
        else
        {
            // Clear submenu if it's not a submenu action
            _buttonItem.Items = null;
            _buttonItem.DynamicSubmenu = null;

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
            _buttonItem.Action.Builtin = null;

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
                case "Builtin":
                    _buttonItem.Action.Builtin = ActionValue;
                    break;
            }
        }

        IsEmpty = false;
    }
}
