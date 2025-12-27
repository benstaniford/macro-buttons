using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace MacroButtons;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private System.Threading.Mutex? _mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure single instance
        _mutex = new System.Threading.Mutex(true, "MacroButtons_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("MacroButtons is already running.", "Already Running",
                          MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        var mainWindow = new MainWindow();
        mainWindow.Show();
    }

    /// <summary>
    /// Releases the single-instance mutex early (used for reload).
    /// </summary>
    public void ReleaseSingleInstanceMutex()
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        _mutex = null;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
