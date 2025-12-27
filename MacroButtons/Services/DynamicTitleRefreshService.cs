using System.Windows.Threading;
using MacroButtons.ViewModels;

namespace MacroButtons.Services;

/// <summary>
/// Service for periodically refreshing dynamic button titles.
/// </summary>
public class DynamicTitleRefreshService : IDisposable
{
    private DispatcherTimer? _timer;
    private readonly IEnumerable<ButtonTileViewModel> _dynamicTiles;
    private readonly TimeSpan _refreshInterval;
    private bool _disposed = false;

    public DynamicTitleRefreshService(IEnumerable<ButtonTileViewModel> tiles, TimeSpan refreshInterval)
    {
        _dynamicTiles = tiles.Where(t => t.IsDynamic).ToList();
        _refreshInterval = refreshInterval;
    }

    /// <summary>
    /// Starts the periodic refresh process.
    /// Performs an initial refresh immediately.
    /// </summary>
    public void Start()
    {
        // Initial refresh
        _ = RefreshAllTitlesAsync();

        // Start periodic refresh using DispatcherTimer (runs on UI thread)
        _timer = new DispatcherTimer
        {
            Interval = _refreshInterval
        };
        _timer.Tick += async (sender, e) => await RefreshAllTitlesAsync();
        _timer.Start();
    }

    /// <summary>
    /// Stops the periodic refresh.
    /// </summary>
    public void Stop()
    {
        _timer?.Stop();
    }

    /// <summary>
    /// Refreshes all dynamic titles in parallel.
    /// </summary>
    private async Task RefreshAllTitlesAsync()
    {
        try
        {
            var tasks = _dynamicTiles.Select(tile => tile.UpdateDynamicTitleAsync());
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Error refreshing dynamic titles: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;

        _disposed = true;
    }
}
