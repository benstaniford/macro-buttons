using System.Collections.ObjectModel;
using System.Windows.Media;
using MacroButtons.Helpers;
using MacroButtons.Models;
using MacroButtons.Services;

namespace MacroButtons.ViewModels;

/// <summary>
/// View model for the main window.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly ConfigurationService _configService;

    public ObservableCollection<ButtonTileViewModel> Tiles { get; set; } = new();
    public Brush Foreground { get; private set; } = Brushes.DarkGreen;
    public Brush Background { get; private set; } = Brushes.Black;
    public int Rows { get; private set; }
    public int Columns { get; private set; }
    public MacroButtonConfig Config { get; private set; } = new();

    public MainViewModel()
    {
        _configService = new ConfigurationService();
        LoadConfiguration();
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
            Tiles.Add(new ButtonTileViewModel(Foreground)
            {
                DisplayTitle = "Config Error"
            });
        }
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
            var tile = new ButtonTileViewModel(Config.Items[i], Foreground);
            Tiles.Add(tile);
        }

        // Fill remaining slots with empty tiles
        for (int i = itemCount; i < totalTiles; i++)
        {
            Tiles.Add(new ButtonTileViewModel(Foreground));
        }
    }

    /// <summary>
    /// Gets all tiles that have dynamic titles for refresh purposes.
    /// </summary>
    public IEnumerable<ButtonTileViewModel> GetDynamicTiles()
    {
        return Tiles.Where(t => t.IsDynamic);
    }
}
