using System.Collections.ObjectModel;
using System.Windows.Media;
using MacroButtons.Helpers;
using MacroButtons.Models;
using MacroButtons.Services;

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
    private DynamicTitleRefreshService? _refreshService;

    public ObservableCollection<ButtonTileViewModel> Tiles { get; set; } = new();
    public Brush Foreground { get; private set; } = Brushes.DarkGreen;
    public Brush Background { get; private set; } = Brushes.Black;
    public int Rows { get; private set; }
    public int Columns { get; private set; }
    public MacroButtonConfig Config { get; private set; } = new();

    public MainViewModel()
    {
        _configService = new ConfigurationService();
        _commandService = new CommandExecutionService();
        _windowHelper = new WindowHelper();
        _keystrokeService = new KeystrokeService(_windowHelper);

        LoadConfiguration();
        StartDynamicTitleRefresh();
    }

    private void LoadConfiguration()
    {
        try
        {
            Config = _configService.LoadConfiguration();

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
            // If config loading fails, use minimal fallback
            System.Windows.MessageBox.Show(
                $"Failed to load configuration: {ex.Message}\nUsing default settings.",
                "Configuration Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            // Set minimal defaults
            Rows = 3;
            Columns = 4;
            var errorTile = new ButtonTileViewModel(Foreground, _commandService, _keystrokeService)
            {
                DisplayTitle = "Config Error"
            };
            Tiles.Add(errorTile);
        }
    }

    private void StartDynamicTitleRefresh()
    {
        var refreshInterval = Config.Global.GetRefreshInterval();
        _refreshService = new DynamicTitleRefreshService(Tiles, refreshInterval);
        _refreshService.Start();
    }

    private void CalculateGridLayout(int itemCount)
    {
        const int MIN_ROWS = 3;
        const int MIN_COLS = 4;
        const int MIN_TILES = MIN_ROWS * MIN_COLS; // 12

        // Ensure we have at least minimum grid
        int totalTiles = Math.Max(itemCount, MIN_TILES);

        // Calculate optimal columns (prefer landscape, 4:3 ratio)
        Columns = Math.Max(MIN_COLS, (int)Math.Ceiling(Math.Sqrt(totalTiles * 1.33)));

        // Calculate required rows
        Rows = Math.Max(MIN_ROWS, (int)Math.Ceiling((double)totalTiles / Columns));
    }

    private void CreateTiles()
    {
        Tiles.Clear();

        int totalTiles = Rows * Columns;
        int itemCount = Config.Items.Count;

        // Create tiles for configured items
        for (int i = 0; i < itemCount && i < totalTiles; i++)
        {
            var tile = new ButtonTileViewModel(Config.Items[i], Foreground, _commandService, _keystrokeService);
            Tiles.Add(tile);
        }

        // Fill remaining slots with empty tiles
        for (int i = itemCount; i < totalTiles; i++)
        {
            var emptyTile = new ButtonTileViewModel(Foreground, _commandService, _keystrokeService);
            Tiles.Add(emptyTile);
        }
    }

    /// <summary>
    /// Gets all tiles that have dynamic titles for refresh purposes.
    /// </summary>
    public IEnumerable<ButtonTileViewModel> GetDynamicTiles()
    {
        return Tiles.Where(t => t.IsDynamic);
    }

    public void Dispose()
    {
        _refreshService?.Dispose();
    }
}
