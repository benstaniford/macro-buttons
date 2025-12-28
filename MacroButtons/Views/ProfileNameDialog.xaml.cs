using System.Windows;

namespace MacroButtons.Views;

/// <summary>
/// Dialog for entering a profile name.
/// </summary>
public partial class ProfileNameDialog : Window
{
    /// <summary>
    /// Gets the profile name entered by the user.
    /// </summary>
    public string ProfileName { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a new profile name dialog.
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="initialValue">Initial value for the profile name (optional)</param>
    public ProfileNameDialog(string title = "Profile Name", string initialValue = "")
    {
        InitializeComponent();

        Title = title;
        ProfileNameTextBox.Text = initialValue;
        ProfileName = initialValue;

        // Focus the text box when loaded
        Loaded += (s, e) =>
        {
            ProfileNameTextBox.SelectAll();
            ProfileNameTextBox.Focus();
        };
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        ProfileName = ProfileNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(ProfileName))
        {
            System.Windows.MessageBox.Show(
                "Profile name cannot be empty.",
                "Invalid Name",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
