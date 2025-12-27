using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using MacroButtons.Helpers;
using MacroButtons.Services;
using MacroButtons.ViewModels;

namespace MacroButtons;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Win32 API constants and imports for non-activating window
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int GWL_EXSTYLE = -20;

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    private readonly MonitorService _monitorService;
    private readonly WindowHelper _windowHelper;
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _monitorService = new MonitorService();
        _windowHelper = new WindowHelper();

        // Initialize view model
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Make window non-activating (never steals focus)
        var hwnd = new WindowInteropHelper(this).Handle;
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);

        // Make window topmost to occlude start menu
        _windowHelper.MakeWindowTopmost(hwnd);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Position window on the specified monitor from configuration
        var monitorIndex = _viewModel.Config.Global.MonitorIndex;
        var bounds = _monitorService.GetMonitorBounds(monitorIndex);

        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;

        // Ensure window is topmost
        Topmost = true;
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        // Store the previous window whenever something tries to activate us
        // This will be used when sending keystrokes
        _windowHelper.StorePreviousActiveWindow();
    }
}
