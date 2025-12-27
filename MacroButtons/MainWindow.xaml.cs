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

        // Try to use the application icon if available
        var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MacroButtons.ico");
        if (System.IO.File.Exists(iconPath))
        {
            _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
        }
        else
        {
            // Fallback to default application icon
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

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _notifyIcon?.Dispose();
        _viewModel?.Dispose();
    }
}
