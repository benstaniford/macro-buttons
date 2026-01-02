using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroButtons.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace MacroButtons.ViewModels;

/// <summary>
/// View model for the theme editor dialog.
/// </summary>
public partial class ThemeEditorViewModel : ViewModelBase
{
    private readonly MacroButtonConfig _config;

    [ObservableProperty]
    private ObservableCollection<ThemeItemViewModel> _themes = new();

    public bool DialogResult { get; private set; }

    public ThemeEditorViewModel(MacroButtonConfig config)
    {
        _config = config;

        // Load themes from config
        if (_config.Themes != null)
        {
            foreach (var theme in _config.Themes)
            {
                Themes.Add(new ThemeItemViewModel(theme));
            }
        }

        // Ensure at least a default theme exists
        if (Themes.Count == 0)
        {
            Themes.Add(new ThemeItemViewModel(new ThemeConfig
            {
                Name = "default",
                Foreground = "darkgreen",
                Background = "black"
            }));
        }
    }

    [RelayCommand]
    private void AddTheme()
    {
        var newTheme = new ThemeConfig
        {
            Name = "new-theme",
            Foreground = "white",
            Background = "black"
        };
        Themes.Add(new ThemeItemViewModel(newTheme));
    }

    [RelayCommand]
    private void DeleteTheme(ThemeItemViewModel theme)
    {
        // Don't allow deleting the last theme
        if (Themes.Count <= 1)
        {
            MessageBox.Show(
                "Cannot delete the last theme. At least one theme must exist.",
                "Delete Theme",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Don't allow deleting the default theme
        if (theme.Name == "default")
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete the 'default' theme? This may cause issues if tiles reference it.",
                "Delete Default Theme",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;
        }

        Themes.Remove(theme);
    }

    [RelayCommand]
    private void Ok()
    {
        // Validate theme names
        var names = new HashSet<string>();
        foreach (var theme in Themes)
        {
            if (string.IsNullOrWhiteSpace(theme.Name))
            {
                MessageBox.Show(
                    "All themes must have a name.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (names.Contains(theme.Name))
            {
                MessageBox.Show(
                    $"Duplicate theme name: '{theme.Name}'. Each theme must have a unique name.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            names.Add(theme.Name);
        }

        // Save themes back to config
        _config.Themes.Clear();
        foreach (var themeVm in Themes)
        {
            _config.Themes.Add(themeVm.ToThemeConfig());
        }

        DialogResult = true;
        CloseWindow();
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        CloseWindow();
    }

    private void CloseWindow()
    {
        Application.Current.Windows.OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)?.Close();
    }
}

/// <summary>
/// View model for an individual theme item.
/// </summary>
public partial class ThemeItemViewModel : ViewModelBase
{
    private string _name = "default";
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _foreground = "darkgreen";
    public string Foreground
    {
        get => _foreground;
        set
        {
            if (SetProperty(ref _foreground, value))
            {
                OnPropertyChanged(nameof(ForegroundBrush));
            }
        }
    }

    private string _background = "black";
    public string Background
    {
        get => _background;
        set
        {
            if (SetProperty(ref _background, value))
            {
                OnPropertyChanged(nameof(BackgroundBrush));
            }
        }
    }

    /// <summary>
    /// Returns a Brush for previewing the foreground color.
    /// </summary>
    public Brush ForegroundBrush
    {
        get
        {
            try
            {
                return Helpers.ColorConverter.ParseColor(Foreground, Brushes.White);
            }
            catch
            {
                return Brushes.White;
            }
        }
    }

    /// <summary>
    /// Returns a Brush for previewing the background color.
    /// </summary>
    public Brush BackgroundBrush
    {
        get
        {
            try
            {
                return Helpers.ColorConverter.ParseColor(Background, Brushes.Black);
            }
            catch
            {
                return Brushes.Black;
            }
        }
    }

    public ThemeItemViewModel(ThemeConfig theme)
    {
        Name = theme.Name;
        Foreground = theme.Foreground;
        Background = theme.Background;
    }

    [RelayCommand]
    private void PickForegroundColor()
    {
        var color = PickColor(Foreground);
        if (color.HasValue)
        {
            Foreground = $"#{color.Value.R:X2}{color.Value.G:X2}{color.Value.B:X2}";
        }
    }

    [RelayCommand]
    private void PickBackgroundColor()
    {
        var color = PickColor(Background);
        if (color.HasValue)
        {
            Background = $"#{color.Value.R:X2}{color.Value.G:X2}{color.Value.B:X2}";
        }
    }

    private Color? PickColor(string currentColorString)
    {
        // Parse current color
        Color currentColor;
        try
        {
            var brush = Helpers.ColorConverter.ParseColor(currentColorString, Brushes.Black);
            if (brush is SolidColorBrush solidBrush)
            {
                currentColor = solidBrush.Color;
            }
            else
            {
                currentColor = Colors.Black;
            }
        }
        catch
        {
            currentColor = Colors.Black;
        }

        // Show color picker dialog
        var dialog = new System.Windows.Forms.ColorDialog
        {
            Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B),
            FullOpen = true,
            AnyColor = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            return Color.FromArgb(255, dialog.Color.R, dialog.Color.G, dialog.Color.B);
        }

        return null;
    }

    public ThemeConfig ToThemeConfig()
    {
        return new ThemeConfig
        {
            Name = Name,
            Foreground = Foreground,
            Background = Background
        };
    }
}
