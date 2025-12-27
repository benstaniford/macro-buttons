using System.Collections.ObjectModel;
using System.Windows.Media;
using MacroButtons.Helpers;
using MacroButtons.Models;
using MacroButtons.Services;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using ColorConverter = MacroButtons.Helpers.ColorConverter;

namespace MacroButtons.ViewModels;

/// <summary>
/// View model for the main window.
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ConfigurationService _configService;
    private readonly CommandExecutionService _commandService;
    private readonly KeystrokeService _keystrokeService;
    private readonly WindowHelper _windowHelper;

    public ObservableCollection<ButtonTileViewModel> Tiles { get; set; } = new();
    public Brush Foreground { get; private set; } = Brushes.DarkGreen;
    public Brush Background { get; private set; } = Brushes.Black;
    public int Rows { get; private set; }
    public int Columns { get; private set; }
    public MacroButtonConfig Config { get; private set; } = new();

    public MainViewModel(WindowHelper? windowHelper = null)
    {
        _configService = new ConfigurationService();
        _commandService = new CommandExecutionService();
        _windowHelper = windowHelper ?? new WindowHelper();

        try
        {
            // Load configuration first to get sendKeys config
            Config = _configService.LoadConfiguration();

            // Create keystroke service with configured sendKeys settings
            _keystrokeService = new KeystrokeService(_windowHelper, Config.Global.SendKeys);

            // Now apply the configuration
            ApplyConfiguration();
        }
        catch (Exception ex)
        {
            // If config loading fails, create with default settings
            _keystrokeService = new KeystrokeService(_windowHelper);

            // Show error and use minimal fallback
            System.Windows.MessageBox.Show(
                $"Failed to load configuration: {ex.Message}\nUsing default settings.",
                "Configuration Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            // Set minimal defaults
            Rows = 3;
            Columns = 5;
            var errorTile = new ButtonTileViewModel(Foreground, _commandService, _keystrokeService, _windowHelper)
            {
                DisplayTitle = "Config Error"
            };
            Tiles.Add(errorTile);
        }
    }

    private void ApplyConfiguration()
    {
        try
        {
            // Apply theme colors
            Foreground = ColorConverter.ParseColor(Config.Theme.Foreground, Brushes.DarkGreen);
            Background = ColorConverter.ParseColor(Config.Theme.Background, Brushes.Black);

            // Calculate grid layout
            CalculateGridLayout(Config.Items.Count);

            // Create tiles
            CreateTiles();
        }
        catch (Exception ex)
        {
            // If config application fails, use minimal fallback
            System.Windows.MessageBox.Show(
                $"Failed to apply configuration: {ex.Message}\nUsing default settings.",
                "Configuration Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            // Set minimal defaults
            Rows = 3;
            Columns = 5;
            var errorTile = new ButtonTileViewModel(Foreground, _commandService, _keystrokeService, _windowHelper)
            {
                DisplayTitle = "Config Error"
            };
            Tiles.Add(errorTile);
        }
    }

    private void CalculateGridLayout(int itemCount)
    {
        const int MIN_ROWS = 3;
        const int MIN_COLS = 5;
        const int MIN_TILES = MIN_ROWS * MIN_COLS; // 15

        // Start with minimum grid
        if (itemCount <= MIN_TILES)
        {
            Rows = MIN_ROWS;
            Columns = MIN_COLS;
            return;
        }

        // Expand preferring rows: add 3 rows before adding 1 column
        // Pattern: 3×5, 4×5, 5×5, 6×5, 6×6, 7×6, 8×6, 9×6, 9×7, ...
        int rows = MIN_ROWS;
        int cols = MIN_COLS;

        while (rows * cols < itemCount)
        {
            // Calculate how many rows we've added since the start
            int rowsAdded = rows - MIN_ROWS;

            // Add 1 column for every 3 rows added
            int targetCols = MIN_COLS + (rowsAdded / 3);

            if (cols < targetCols)
            {
                // Time to add a column
                cols++;
            }
            else
            {
                // Add another row
                rows++;
            }
        }

        Rows = rows;
        Columns = cols;
    }

    private void CreateTiles()
    {
        Tiles.Clear();

        int totalTiles = Rows * Columns;
        int itemCount = Config.Items.Count;
        var globalRefreshInterval = Config.Global.GetRefreshInterval();

        // Create tiles for configured items
        for (int i = 0; i < itemCount && i < totalTiles; i++)
        {
            var tile = new ButtonTileViewModel(Config.Items[i], Foreground, globalRefreshInterval, _commandService, _keystrokeService, _windowHelper);
            Tiles.Add(tile);
        }

        // Fill remaining slots with empty tiles
        for (int i = itemCount; i < totalTiles; i++)
        {
            var emptyTile = new ButtonTileViewModel(Foreground, _commandService, _keystrokeService, _windowHelper);
            Tiles.Add(emptyTile);
        }
    }

    public void Dispose()
    {
        // Dispose all tiles (stops their refresh timers)
        foreach (var tile in Tiles)
        {
            tile.Dispose();
        }
    }
}
