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
/// Represents a level in the navigation hierarchy.
/// Stores the menu items and grid dimensions for a menu level.
/// </summary>
internal class NavigationLevel
{
    public List<ButtonItem> Items { get; set; }
    public int Rows { get; set; }
    public int Columns { get; set; }

    public NavigationLevel(List<ButtonItem> items, int rows, int columns)
    {
        Items = items;
        Rows = rows;
        Columns = columns;
    }
}

/// <summary>
/// View model for the main window.
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ProfileService _profileService;
    private readonly ConfigurationService _configService;
    private readonly CommandExecutionService _commandService;
    private readonly PowerShellService _powershellService;
    private readonly LoggingService _loggingService;
    private KeystrokeService _keystrokeService;
    private readonly WindowHelper _windowHelper;

    // Navigation state
    private readonly Stack<NavigationLevel> _navigationStack = new();
    private List<ButtonItem> _currentItems = new();

    // Profile state
    private string _currentProfileName;
    private bool _isAutoSwitched = false;
    private string? _baseProfileName = null; // The profile to return to when no window matches

    public ObservableCollection<ButtonTileViewModel> Tiles { get; set; } = new();
    public Brush WindowForeground { get; private set; } = Brushes.DarkGreen;
    public Brush WindowBackground { get; private set; } = Brushes.Black;
    public int Rows { get; private set; }
    public int Columns { get; private set; }
    public MacroButtonConfig Config { get; private set; } = new();

    /// <summary>
    /// Returns true if currently at the root menu level.
    /// </summary>
    public bool IsAtRootLevel => _navigationStack.Count == 0;

    /// <summary>
    /// Gets the current profile name.
    /// </summary>
    public string CurrentProfileName => _currentProfileName;

    public MainViewModel(WindowHelper? windowHelper = null, ProfileService? profileService = null)
    {
        _profileService = profileService ?? new ProfileService();
        _configService = new ConfigurationService(_profileService);
        _loggingService = new LoggingService();
        _commandService = new CommandExecutionService();
        _powershellService = new PowerShellService(_loggingService);
        _windowHelper = windowHelper ?? new WindowHelper();

        _loggingService.LogInfo("========== MacroButtons Started ==========");

        try
        {
            // Get current profile name
            _currentProfileName = _profileService.GetCurrentProfileName();
            _loggingService.LogInfo($"Loading profile: {_currentProfileName}");

            // Load configuration first to get sendKeys config
            Config = _configService.LoadConfiguration(_currentProfileName);

            // Create keystroke service with configured sendKeys settings
            _keystrokeService = new KeystrokeService(_windowHelper, Config.Global.SendKeys);

            // Initialize base profile to the starting profile
            // This ensures that if we auto-switch, we know where to return to
            _baseProfileName = _currentProfileName;

            // Now apply the configuration
            ApplyConfiguration();
        }
        catch (Exception ex)
        {
            // If config loading fails, create with default settings
            _keystrokeService = new KeystrokeService(_windowHelper);
            _currentProfileName = "default";

            // Show error and use minimal fallback
            System.Windows.MessageBox.Show(
                $"Failed to load configuration: {ex.Message}\nUsing default settings.",
                "Configuration Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            // Set minimal defaults
            Rows = 3;
            Columns = 5;
            var errorTile = new ButtonTileViewModel(Config, _commandService, _keystrokeService, _powershellService, _loggingService, _windowHelper)
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
            // Apply theme colors (use default theme for window background)
            var defaultTheme = Config.GetTheme("default");
            WindowForeground = ColorConverter.ParseColor(defaultTheme.Foreground, Brushes.DarkGreen);
            WindowBackground = ColorConverter.ParseColor(defaultTheme.Background, Brushes.Black);

            // Reset to root level (clear navigation stack)
            ResetToRootLevel();

            // Calculate grid layout for current items
            CalculateGridLayout(_currentItems.Count);

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
            var errorTile = new ButtonTileViewModel(Config, _commandService, _keystrokeService, _powershellService, _loggingService, _windowHelper)
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
        int itemCount = _currentItems.Count;
        var globalRefreshInterval = Config.Global.GetRefreshInterval();

        // Create tiles for configured items
        for (int i = 0; i < itemCount && i < totalTiles; i++)
        {
            var tile = new ButtonTileViewModel(
                _currentItems[i],
                Config,
                globalRefreshInterval,
                _commandService,
                _keystrokeService,
                _powershellService,
                _loggingService,
                _windowHelper,
                NavigateToSubmenu,
                NavigateBack);
            Tiles.Add(tile);
        }

        // Fill remaining slots with empty tiles
        for (int i = itemCount; i < totalTiles; i++)
        {
            var emptyTile = new ButtonTileViewModel(Config, _commandService, _keystrokeService, _powershellService, _loggingService, _windowHelper);
            Tiles.Add(emptyTile);
        }
    }

    /// <summary>
    /// Navigates to a submenu by replacing the current grid with submenu items.
    /// Adds BACK button as first item automatically.
    /// </summary>
    public void NavigateToSubmenu(List<ButtonItem> submenuItems)
    {
        if (submenuItems == null || submenuItems.Count == 0)
            return;

        // Dispose current tiles (stops all dynamic refresh timers)
        DisposeTiles();

        // Save current level to navigation stack
        var currentLevel = new NavigationLevel(_currentItems, Rows, Columns);
        _navigationStack.Push(currentLevel);

        // Create BACK button (always first in submenu)
        var backButton = new ButtonItem
        {
            Title = "<- BACK",
            Action = new ActionDefinition { Keypress = "__NAVIGATE_BACK__" },
            Items = null
        };

        // Combine BACK button + submenu items
        _currentItems = new List<ButtonItem> { backButton };
        _currentItems.AddRange(submenuItems);

        // Recalculate grid for new item count
        CalculateGridLayout(_currentItems.Count);

        // Recreate tiles for submenu (starts new dynamic tile timers)
        CreateTiles();
    }

    /// <summary>
    /// Navigates back to the parent menu level.
    /// </summary>
    public void NavigateBack()
    {
        if (IsAtRootLevel)
            return; // Already at root, can't go back

        // Dispose current tiles (stops submenu dynamic timers)
        DisposeTiles();

        // Pop parent level from stack
        var parentLevel = _navigationStack.Pop();

        // Restore parent items and grid dimensions
        _currentItems = parentLevel.Items;
        Rows = parentLevel.Rows;
        Columns = parentLevel.Columns;

        // Recreate tiles for parent menu (restarts parent dynamic timers)
        CreateTiles();
    }

    /// <summary>
    /// Disposes all current tiles (stops their refresh timers).
    /// Called before navigation to ensure no orphaned timers.
    /// </summary>
    private void DisposeTiles()
    {
        foreach (var tile in Tiles)
        {
            tile.Dispose(); // Stops DispatcherTimer
        }
        Tiles.Clear();
    }

    /// <summary>
    /// Resets navigation to root level.
    /// Called during initialization or configuration reload.
    /// </summary>
    private void ResetToRootLevel()
    {
        // Dispose any existing tiles
        DisposeTiles();

        // Clear navigation stack
        _navigationStack.Clear();

        // Set current items to root config items
        _currentItems = new List<ButtonItem>(Config.Items);
    }

    /// <summary>
    /// Switches to a different profile and reloads the configuration.
    /// </summary>
    /// <param name="profileName">The profile to switch to</param>
    /// <param name="isAutoSwitch">True if this is an automatic switch triggered by window change</param>
    public void SwitchProfile(string profileName, bool isAutoSwitch = false)
    {
        try
        {
            // Load new configuration
            var newConfig = _configService.LoadConfiguration(profileName);

            // Store old config for comparison
            var oldConfig = Config;

            // Track whether this is a manual or auto switch
            if (!isAutoSwitch)
            {
                // Manual switch: remember this as the base profile
                _baseProfileName = profileName;
                _isAutoSwitched = false;
            }
            else
            {
                // Auto switch: mark that we're in auto-switched mode
                _isAutoSwitched = true;
            }

            // Update current profile name
            _currentProfileName = profileName;
            _profileService.SetCurrentProfileName(profileName);

            // Update config reference
            Config = newConfig;

            // Check if SendKeysConfig changed - if so, recreate KeystrokeService
            if (!SendKeysConfigEquals(oldConfig.Global.SendKeys, newConfig.Global.SendKeys))
            {
                _keystrokeService = new KeystrokeService(_windowHelper, newConfig.Global.SendKeys);
            }

            // Apply new configuration (theme, tiles, layout)
            ApplyConfiguration();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to switch profile: {ex.Message}",
                "Profile Switch Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles active window changes and switches profiles automatically if needed.
    /// Called periodically by MainWindow to check if profile should change based on active window.
    /// </summary>
    public void HandleActiveWindowChange(IntPtr activeWindowHandle)
    {
        // Get all available profiles
        var allProfiles = _profileService.ListProfiles();

        // Check each profile to see if it matches the active window
        string? matchingProfile = null;
        foreach (var profileName in allProfiles)
        {
            try
            {
                var config = _configService.LoadConfiguration(profileName);
                var activeWindowPattern = config.Global.ActiveWindow;

                if (!string.IsNullOrWhiteSpace(activeWindowPattern))
                {
                    if (_windowHelper.WindowMatchesPattern(activeWindowHandle, activeWindowPattern))
                    {
                        matchingProfile = profileName;
                        break;
                    }
                }
            }
            catch
            {
                // Ignore errors loading other profiles
                continue;
            }
        }

        // Decide what to do based on match result
        if (matchingProfile != null)
        {
            // Found a matching profile
            if (_currentProfileName != matchingProfile)
            {
                // Switch to the matching profile (auto-switch)
                SwitchProfile(matchingProfile, isAutoSwitch: true);
            }
        }
        else
        {
            // No matching profile found
            if (_isAutoSwitched)
            {
                // We're currently on an auto-switched profile, return to base
                var targetProfile = _baseProfileName ?? "default";
                if (_currentProfileName != targetProfile)
                {
                    SwitchProfile(targetProfile, isAutoSwitch: false);
                }
            }
        }
    }

    /// <summary>
    /// Compares two SendKeysConfig instances for equality.
    /// </summary>
    private bool SendKeysConfigEquals(SendKeysConfig a, SendKeysConfig b)
    {
        if (a == null && b == null)
            return true;
        if (a == null || b == null)
            return false;

        return a.Delay == b.Delay && a.Duration == b.Duration;
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
