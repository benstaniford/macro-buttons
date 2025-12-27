using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroButtons.Models;
using System.Windows.Media;

namespace MacroButtons.ViewModels;

/// <summary>
/// View model for an individual button tile.
/// </summary>
public partial class ButtonTileViewModel : ViewModelBase
{
    private readonly ButtonItem? _config;

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
    public ButtonTileViewModel(Brush foreground)
    {
        _config = null;
        IsEmpty = true;
        IsDynamic = false;
        HasAction = false;
        DisplayTitle = string.Empty;
        Foreground = foreground;
    }

    /// <summary>
    /// Creates a tile from a ButtonItem configuration.
    /// </summary>
    public ButtonTileViewModel(ButtonItem config, Brush foreground)
    {
        _config = config;
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

        // Placeholder - will be implemented in Step 5, 6, 7
        await Task.CompletedTask;
    }

    /// <summary>
    /// Updates the dynamic title by executing the title command.
    /// Only applies to tiles with dynamic titles.
    /// </summary>
    public async Task UpdateDynamicTitleAsync()
    {
        if (!IsDynamic || _config?.Title == null)
            return;

        // Placeholder - will be implemented in Step 7
        await Task.CompletedTask;
    }
}
