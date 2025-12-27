using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
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

    // Per-monitor DPI awareness APIs
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    private const uint MONITOR_DEFAULTTONEAREST = 2;
    private const int MDT_EFFECTIVE_DPI = 0;

    private readonly MonitorService _monitorService;
    private readonly WindowHelper _windowHelper;
    private readonly MainViewModel _viewModel;
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private DispatcherTimer? _cursorTrackingTimer;

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

    /// <summary>
    /// Gets the DPI scaling for a specific monitor using Win32 APIs.
    /// </summary>
    private (double scaleX, double scaleY) GetMonitorDpi(System.Drawing.Rectangle monitorBounds)
    {
        try
        {
            // Get monitor handle from a point in the center of the monitor
            var point = new POINT
            {
                X = monitorBounds.Left + monitorBounds.Width / 2,
                Y = monitorBounds.Top + monitorBounds.Height / 2
            };

            IntPtr hMonitor = MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);

            if (hMonitor != IntPtr.Zero)
            {
                int result = GetDpiForMonitor(hMonitor, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
                if (result == 0) // S_OK
                {
                    // Standard DPI is 96, so scale factor is actualDpi / 96
                    double scaleX = dpiX / 96.0;
                    double scaleY = dpiY / 96.0;
                    System.Diagnostics.Debug.WriteLine($"Monitor DPI: {dpiX}x{dpiY} (scale: {scaleX:F2}x{scaleY:F2})");
                    return (scaleX, scaleY);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get monitor DPI: {ex.Message}");
        }

        // Fallback to system DPI if per-monitor DPI fails
        var source = PresentationSource.FromVisual(this);
        if (source != null)
        {
            return (source.CompositionTarget.TransformToDevice.M11,
                    source.CompositionTarget.TransformToDevice.M22);
        }

        return (1.0, 1.0);
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

        // Get DPI scaling for BOTH primary and target monitors
        // Position coordinates use the system/primary DPI
        // Size uses the target monitor's DPI
        var primaryBounds = _monitorService.GetMonitorBounds(0);
        var (systemDpiX, systemDpiY) = GetMonitorDpi(primaryBounds);
        var (targetDpiX, targetDpiY) = GetMonitorDpi(bounds);

        System.Diagnostics.Debug.WriteLine($"Primary monitor DPI scaling: X={systemDpiX:F2}, Y={systemDpiY:F2}");
        System.Diagnostics.Debug.WriteLine($"Target monitor DPI scaling: X={targetDpiX:F2}, Y={targetDpiY:F2}");

        // Convert screen coordinates (physical pixels) to WPF logical pixels
        // Position: Scale by system/primary DPI (WPF's coordinate system is based on primary monitor)
        // Size: Scale by target monitor's DPI (window should fill the target monitor)
        Left = bounds.Left / systemDpiX;
        Top = bounds.Top / systemDpiY;
        Width = bounds.Width / targetDpiX;
        Height = bounds.Height / targetDpiY;

        // Ensure window is topmost
        Topmost = true;

        System.Diagnostics.Debug.WriteLine($"Window positioned: Left={Left}, Top={Top}, Width={Width}, Height={Height}");

        // Set our monitor bounds for cursor tracking
        _windowHelper.SetOurMonitorBounds(bounds);

        // Initialize cursor position immediately (before timer starts)
        _windowHelper.UpdateCursorPositionIfNotOnOurMonitor();

        // Start cursor tracking timer to continuously track cursor position when it's on other monitors
        _cursorTrackingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // Poll every 100ms
        };
        _cursorTrackingTimer.Tick += (s, e) => _windowHelper.UpdateCursorPositionIfNotOnOurMonitor();
        _cursorTrackingTimer.Start();
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
        _cursorTrackingTimer?.Stop();
        _notifyIcon?.Dispose();
        _viewModel?.Dispose();
    }
}
