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
        SelectedTile = tile;
    }

    [RelayCommand]
    private void Save()
    {
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

    [ObservableProperty]
    private ButtonItem? _buttonItem;

    [ObservableProperty]
    private string _displayTitle;

    [ObservableProperty]
    private bool _isEmpty;

    public ButtonTileEditorViewModel(int index, ButtonItem? item, MacroButtonConfig rootConfig)
    {
        _index = index;
        _buttonItem = item;
        _rootConfig = rootConfig;
        _isEmpty = item == null;
        _displayTitle = item?.GetStaticTitle() ?? string.Empty;
    }

    public void UpdateFromItem(ButtonItem? item)
    {
        ButtonItem = item;
        IsEmpty = item == null;
        DisplayTitle = item?.GetStaticTitle() ?? string.Empty;
    }
}
