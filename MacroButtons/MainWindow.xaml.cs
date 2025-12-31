using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using MacroButtons.Helpers;
using MacroButtons.Services;
using MacroButtons.ViewModels;
using MacroButtons.Views;

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
    private readonly ProfileService _profileService;
    private readonly SettingsService _settingsService;
    private readonly MainViewModel _viewModel;
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private DispatcherTimer? _cursorTrackingTimer;

    public MainWindow()
    {
        InitializeComponent();

        _monitorService = new MonitorService();
        _windowHelper = new WindowHelper();
        _profileService = new ProfileService();
        _settingsService = new SettingsService();

        // Initialize view model with the shared instances
        _viewModel = new MainViewModel(_windowHelper, _profileService);
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

        // Build context menu
        BuildTrayContextMenu();

        _notifyIcon.DoubleClick += (s, e) =>
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Activate();
        };
    }

    private void BuildTrayContextMenu()
    {
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();

        // Profiles submenu
        var profilesMenuItem = new System.Windows.Forms.ToolStripMenuItem("Profiles");
        BuildProfilesSubmenu(profilesMenuItem);
        contextMenu.Items.Add(profilesMenuItem);

        // Monitor submenu
        var monitorMenuItem = new System.Windows.Forms.ToolStripMenuItem("Monitor");
        BuildMonitorSubmenu(monitorMenuItem);
        contextMenu.Items.Add(monitorMenuItem);

        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        // Edit Config
        var editConfigItem = new System.Windows.Forms.ToolStripMenuItem("Edit Config");
        editConfigItem.Click += (s, e) => OpenConfigFile();
        contextMenu.Items.Add(editConfigItem);

        // Reload current profile
        var reloadItem = new System.Windows.Forms.ToolStripMenuItem("Reload");
        reloadItem.Click += (s, e) => ReloadCurrentProfile();
        contextMenu.Items.Add(reloadItem);

        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        // View Log
        var viewLogItem = new System.Windows.Forms.ToolStripMenuItem("View Log");
        viewLogItem.Click += (s, e) => LoggingService.OpenLogInNotepad();
        contextMenu.Items.Add(viewLogItem);

        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        // Quit
        var quitItem = new System.Windows.Forms.ToolStripMenuItem("Quit");
        quitItem.Click += (s, e) => QuitApplication();
        contextMenu.Items.Add(quitItem);

        _notifyIcon!.ContextMenuStrip = contextMenu;
    }

    private void BuildProfilesSubmenu(System.Windows.Forms.ToolStripMenuItem profilesMenuItem)
    {
        profilesMenuItem.DropDownItems.Clear();

        var currentProfile = _viewModel.CurrentProfileName;
        var profiles = _profileService.ListProfiles();

        // Add each profile as a menu item with checkmark for current
        foreach (var profile in profiles)
        {
            var profileItem = new System.Windows.Forms.ToolStripMenuItem(profile);
            profileItem.Checked = (profile == currentProfile);
            profileItem.Click += (s, e) => SwitchToProfile(profile);
            profilesMenuItem.DropDownItems.Add(profileItem);
        }

        profilesMenuItem.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

        // New Profile
        var newProfileItem = new System.Windows.Forms.ToolStripMenuItem("New Profile...");
        newProfileItem.Click += (s, e) => CreateNewProfile();
        profilesMenuItem.DropDownItems.Add(newProfileItem);

        // Import Profile
        var importProfileItem = new System.Windows.Forms.ToolStripMenuItem("Import Profile...");
        importProfileItem.Click += (s, e) => ImportProfile();
        profilesMenuItem.DropDownItems.Add(importProfileItem);

        // Rename Profile
        var renameProfileItem = new System.Windows.Forms.ToolStripMenuItem("Rename Profile...");
        renameProfileItem.Click += (s, e) => RenameCurrentProfile();
        profilesMenuItem.DropDownItems.Add(renameProfileItem);

        // Delete Profile
        var deleteProfileItem = new System.Windows.Forms.ToolStripMenuItem("Delete Profile...");
        deleteProfileItem.Click += (s, e) => DeleteCurrentProfile();
        profilesMenuItem.DropDownItems.Add(deleteProfileItem);
    }

    private void BuildMonitorSubmenu(System.Windows.Forms.ToolStripMenuItem monitorMenuItem)
    {
        monitorMenuItem.DropDownItems.Clear();

        var currentMonitorIndex = _settingsService.GetOrInitializeMonitorIndex(_monitorService);
        var monitorCount = _monitorService.GetMonitorCount();

        // Add each monitor as a menu item with checkmark for current
        for (int i = 0; i < monitorCount; i++)
        {
            var screen = _monitorService.GetMonitorByIndex(i);
            var bounds = screen.Bounds;

            // Display as 1-based index (Monitor 1, Monitor 2, etc.)
            var displayIndex = i + 1;
            var menuText = $"Monitor {displayIndex} ({bounds.Width}x{bounds.Height})";
            if (screen.Primary)
            {
                menuText += " - Primary";
            }

            var monitorItem = new System.Windows.Forms.ToolStripMenuItem(menuText);
            monitorItem.Checked = (i == currentMonitorIndex);

            // Capture the index for the click handler
            int capturedIndex = i;
            monitorItem.Click += (s, e) => SwitchToMonitor(capturedIndex);

            monitorMenuItem.DropDownItems.Add(monitorItem);
        }
    }

    private void SwitchToMonitor(int monitorIndex)
    {
        try
        {
            // Save to registry
            _settingsService.SetMonitorIndex(monitorIndex);

            // Rebuild tray menu to update checkmarks
            BuildTrayContextMenu();

            // Reload window position
            RepositionWindow();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to switch monitor: {ex.Message}",
                "Monitor Switch Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void SwitchToProfile(string profileName)
    {
        if (profileName == _viewModel.CurrentProfileName)
            return; // Already on this profile

        _viewModel.SwitchProfile(profileName);

        // Rebuild tray menu to update checkmarks
        BuildTrayContextMenu();
    }

    private void ReloadCurrentProfile()
    {
        var currentProfile = _viewModel.CurrentProfileName;
        _viewModel.SwitchProfile(currentProfile);
    }

    private void CreateNewProfile()
    {
        // Show input dialog
        var dialog = new ProfileNameDialog("Create New Profile");
        if (dialog.ShowDialog() != true)
            return;

        var newProfileName = dialog.ProfileName;

        // Validate
        if (!_profileService.ValidateProfileName(newProfileName, out var errorMessage))
        {
            System.Windows.MessageBox.Show(
                errorMessage,
                "Invalid Profile Name",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (_profileService.ProfileExists(newProfileName))
        {
            System.Windows.MessageBox.Show(
                $"Profile '{newProfileName}' already exists.",
                "Profile Exists",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Ask: copy current or use default?
        var result = System.Windows.MessageBox.Show(
            "Do you want to copy the current profile settings?\n\nYes = Copy current profile\nNo = Use default settings",
            "Create Profile",
            System.Windows.MessageBoxButton.YesNoCancel,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Cancel)
            return;

        try
        {
            string? sourceProfileName = null;
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                sourceProfileName = _viewModel.CurrentProfileName;
            }

            _profileService.CreateProfile(newProfileName, sourceProfileName);

            // Switch to new profile
            SwitchToProfile(newProfileName);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to create profile: {ex.Message}",
                "Create Profile Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void ImportProfile()
    {
        // Try to determine the samples directory location
        string? initialDirectory = null;
        try
        {
            // First, try the installed samples location (Program Files)
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(exePath))
            {
                var exeDir = System.IO.Path.GetDirectoryName(exePath);
                if (!string.IsNullOrEmpty(exeDir))
                {
                    var samplesPath = System.IO.Path.Combine(exeDir, "samples");
                    if (System.IO.Directory.Exists(samplesPath))
                    {
                        initialDirectory = samplesPath;
                    }
                }
            }

            // Fallback: check if we're running from the source repo (development)
            if (initialDirectory == null)
            {
                var currentDir = System.IO.Directory.GetCurrentDirectory();
                var samplesPath = System.IO.Path.Combine(currentDir, "samples");
                if (System.IO.Directory.Exists(samplesPath))
                {
                    initialDirectory = samplesPath;
                }
            }
        }
        catch
        {
            // If any errors, just use default location (user's documents)
        }

        // Open file browser to select JSON file
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Profile to Import",
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            FilterIndex = 1,
            Multiselect = false,
            InitialDirectory = initialDirectory
        };

        if (openFileDialog.ShowDialog() != true)
            return;

        var sourceFilePath = openFileDialog.FileName;

        // Extract suggested profile name from filename (without extension)
        var suggestedName = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);

        // Show input dialog with suggested name
        var dialog = new ProfileNameDialog("Import Profile", suggestedName);
        if (dialog.ShowDialog() != true)
            return;

        var newProfileName = dialog.ProfileName;

        // Validate
        if (!_profileService.ValidateProfileName(newProfileName, out var errorMessage))
        {
            System.Windows.MessageBox.Show(
                errorMessage,
                "Invalid Profile Name",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (_profileService.ProfileExists(newProfileName))
        {
            // Ask user if they want to overwrite
            var overwriteResult = System.Windows.MessageBox.Show(
                $"Profile '{newProfileName}' already exists.\n\nDo you want to overwrite it?",
                "Profile Exists",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (overwriteResult != System.Windows.MessageBoxResult.Yes)
                return;

            // Delete the existing profile before importing
            try
            {
                _profileService.DeleteProfile(newProfileName);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Failed to delete existing profile: {ex.Message}",
                    "Delete Profile Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }
        }

        try
        {
            _profileService.ImportProfile(sourceFilePath, newProfileName);

            // Switch to new profile
            SwitchToProfile(newProfileName);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to import profile: {ex.Message}",
                "Import Profile Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void RenameCurrentProfile()
    {
        var currentProfile = _viewModel.CurrentProfileName;

        // Show input dialog with current name
        var dialog = new ProfileNameDialog("Rename Profile", currentProfile);
        if (dialog.ShowDialog() != true)
            return;

        var newProfileName = dialog.ProfileName;

        if (newProfileName == currentProfile)
            return; // No change

        // Validate
        if (!_profileService.ValidateProfileName(newProfileName, out var errorMessage))
        {
            System.Windows.MessageBox.Show(
                errorMessage,
                "Invalid Profile Name",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (_profileService.ProfileExists(newProfileName))
        {
            System.Windows.MessageBox.Show(
                $"Profile '{newProfileName}' already exists.",
                "Profile Exists",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            _profileService.RenameProfile(currentProfile, newProfileName);

            // Switch to renamed profile (updates UI and menu)
            SwitchToProfile(newProfileName);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to rename profile: {ex.Message}",
                "Rename Profile Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void DeleteCurrentProfile()
    {
        var currentProfile = _viewModel.CurrentProfileName;
        var profiles = _profileService.ListProfiles();

        // Prevent deleting last profile
        if (profiles.Count <= 1)
        {
            System.Windows.MessageBox.Show(
                "Cannot delete the last profile.",
                "Delete Profile",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Confirmation
        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete the profile '{currentProfile}'?\n\nThis action cannot be undone.",
            "Delete Profile",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        try
        {
            // Determine which profile to switch to after deletion
            var nextProfile = profiles.FirstOrDefault(p => p != currentProfile);
            if (nextProfile == null)
            {
                System.Windows.MessageBox.Show(
                    "Cannot determine next profile to switch to.",
                    "Delete Profile Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            // Delete the profile
            _profileService.DeleteProfile(currentProfile);

            // Switch to another profile
            SwitchToProfile(nextProfile);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to delete profile: {ex.Message}",
                "Delete Profile Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void QuitApplication()
    {
        _notifyIcon?.Dispose();
        _viewModel?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    private void OpenConfigFile()
    {
        try
        {
            var currentProfile = _viewModel.CurrentProfileName;
            var configPath = _profileService.GetProfilePath(currentProfile);

            // Ensure the file exists
            if (!System.IO.File.Exists(configPath))
            {
                System.Windows.MessageBox.Show(
                    $"Configuration file not found at:\n{configPath}",
                    "Config Not Found",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Open with default JSON editor
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = configPath,
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to open configuration file: {ex.Message}",
                "Open Config Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
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

        // Store our window handle for tracking foreground window
        _windowHelper.SetOurWindowHandle(hwnd);
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

    /// <summary>
    /// Repositions the window on the configured monitor.
    /// </summary>
    private void RepositionWindow()
    {
        // Get monitor index from settings (uses smallest monitor as default on first run)
        var monitorIndex = _settingsService.GetOrInitializeMonitorIndex(_monitorService);
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
            _settingsService.SetMonitorIndex(monitorIndex);
        }

        var bounds = _monitorService.GetMonitorBounds(monitorIndex);

        // Debug output
        System.Diagnostics.Debug.WriteLine($"\nUsing monitor index: {monitorIndex}");
        System.Diagnostics.Debug.WriteLine($"Monitor bounds: Left={bounds.Left}, Top={bounds.Top}, Width={bounds.Width}, Height={bounds.Height}");

        // Safety check: If coordinates seem unreasonable (likely due to virtual desktop/DPI issues),
        // fall back to primary monitor
        if (bounds.Left > 10000 || bounds.Top > 10000 || bounds.Left < -10000 || bounds.Top < -10000)
        {
            System.Diagnostics.Debug.WriteLine("WARNING: Monitor bounds appear invalid, falling back to primary monitor");
            monitorIndex = 0;
            bounds = _monitorService.GetMonitorBounds(0);
            _settingsService.SetMonitorIndex(monitorIndex);
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
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Position window on the monitor from settings
        RepositionWindow();

        // Initialize cursor and window tracking immediately (before timer starts)
        _windowHelper.UpdateCursorPositionIfNotOnOurMonitor();
        _windowHelper.UpdatePreviousWindowIfNotUs();

        // Start tracking timer to continuously track both cursor position and foreground window
        _cursorTrackingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // Poll every 100ms
        };
        _cursorTrackingTimer.Tick += (s, e) =>
        {
            _windowHelper.UpdateCursorPositionIfNotOnOurMonitor();
            _windowHelper.UpdatePreviousWindowIfNotUs();

            // Check for profile auto-switching based on active window
            var foregroundWindow = _windowHelper.GetCurrentForegroundWindow();
            if (foregroundWindow != IntPtr.Zero)
            {
                _viewModel.HandleActiveWindowChange(foregroundWindow);
            }
        };
        _cursorTrackingTimer.Start();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        // Store the previous window as a fallback (should rarely be called due to WS_EX_NOACTIVATE)
        // The primary tracking happens via the timer calling UpdatePreviousWindowIfNotUs()
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
