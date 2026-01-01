using MacroButtons.ViewModels;
using System.Windows;

namespace MacroButtons.Views;

/// <summary>
/// Interaction logic for ConfigEditorWindow.xaml
/// </summary>
public partial class ConfigEditorWindow : Window
{
    public ConfigEditorWindow(ConfigEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
