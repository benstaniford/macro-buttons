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
    private System.Windows.Forms.NotifyIcon? _notifyIcon;

    public MainWindow()
    {
        InitializeComponent();

        _monitorService = new MonitorService();
        _windowHelper = new WindowHelper();

        // Initialize view model
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // Initialize system tray icon
        InitializeNotifyIcon();
    }

    private void InitializeNotifyIcon()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Text = "MacroButtons",
            Visible = true
        };

        // Try to extract icon from the application's executable
        try
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(exePath) && System.IO.File.Exists(exePath))
            {
                _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
            }
            else
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }
        }
        catch
        {
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        // Create context menu
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        var quitItem = new System.Windows.Forms.ToolStripMenuItem("Quit");
        quitItem.Click += (s, e) => QuitApplication();
        contextMenu.Items.Add(quitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (s, e) =>
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Activate();
        };
    }

    private void QuitApplication()
    {
        _notifyIcon?.Dispose();
        _viewModel?.Dispose();
        System.Windows.Application.Current.Shutdown();
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
        var monitorCount = _monitorService.GetMonitorCount();

        // Debug: Show all monitors
        System.Diagnostics.Debug.WriteLine($"=== All Monitors ({monitorCount}) ===");
        for (int i = 0; i < monitorCount; i++)
        {
            var screen = _monitorService.GetMonitorByIndex(i);
            var b = screen.Bounds;
            System.Diagnostics.Debug.WriteLine($"Monitor {i}: {b.Width}x{b.Height} @ ({b.Left},{b.Top}) - Primary: {screen.Primary}");
        }

        // Clamp monitor index to valid range
        if (monitorIndex < 0 || monitorIndex >= monitorCount)
        {
            monitorIndex = 0;
        }

        var bounds = _monitorService.GetMonitorBounds(monitorIndex);

        // Debug output
        System.Diagnostics.Debug.WriteLine($"\nRequested monitor index: {_viewModel.Config.Global.MonitorIndex}, Using: {monitorIndex}");
        System.Diagnostics.Debug.WriteLine($"Monitor bounds: Left={bounds.Left}, Top={bounds.Top}, Width={bounds.Width}, Height={bounds.Height}");

        // Safety check: If coordinates seem unreasonable (likely due to virtual desktop/DPI issues),
        // fall back to primary monitor
        if (bounds.Left > 10000 || bounds.Top > 10000 || bounds.Left < -10000 || bounds.Top < -10000)
        {
            System.Diagnostics.Debug.WriteLine("WARNING: Monitor bounds appear invalid, falling back to primary monitor");
            monitorIndex = 0;
            bounds = _monitorService.GetMonitorBounds(0);
            System.Diagnostics.Debug.WriteLine($"Fallback bounds: Left={bounds.Left}, Top={bounds.Top}, Width={bounds.Width}, Height={bounds.Height}");
        }

        // Get system DPI to convert physical screen coordinates to WPF logical coordinates
        var source = PresentationSource.FromVisual(this);
        double dpiScaleX = 1.0;
        double dpiScaleY = 1.0;

        if (source != null)
        {
            dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
            dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
            System.Diagnostics.Debug.WriteLine($"System DPI scaling: X={dpiScaleX}, Y={dpiScaleY}");
        }

        // Convert screen coordinates (physical pixels) to WPF logical pixels
        // Position needs to be scaled by system DPI
        Left = bounds.Left / dpiScaleX;
        Top = bounds.Top / dpiScaleY;

        // Size: try using physical pixels directly and let WPF handle per-monitor DPI
        Width = bounds.Width;
        Height = bounds.Height;

        // Ensure window is topmost
        Topmost = true;

        System.Diagnostics.Debug.WriteLine($"Window positioned: Left={Left}, Top={Top}, Width={Width}, Height={Height}");
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        // Store the previous window whenever something tries to activate us
        // This will be used when sending keystrokes
        _windowHelper.StorePreviousActiveWindow();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _notifyIcon?.Dispose();
        _viewModel?.Dispose();
    }
}
