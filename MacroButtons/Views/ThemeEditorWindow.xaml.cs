using MacroButtons.ViewModels;
using System.Windows;

namespace MacroButtons.Views;

/// <summary>
/// Interaction logic for ThemeEditorWindow.xaml
/// </summary>
public partial class ThemeEditorWindow : Window
{
    public ThemeEditorWindow(ThemeEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
