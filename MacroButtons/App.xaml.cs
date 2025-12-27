using System.Windows;

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

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
