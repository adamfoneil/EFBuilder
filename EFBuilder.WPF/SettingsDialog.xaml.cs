using ModelBuilder;
using System.Windows;

namespace EFBuilder.WPF;

/// <summary>
/// Interaction logic for SettingsDialog.xaml
/// </summary>
public partial class SettingsDialog : Window
{
    public LocalSettings Settings { get; private set; }

    public SettingsDialog(LocalSettings settings)
    {
        InitializeComponent();
        Settings = new LocalSettings
        {
            IdentityType = settings.IdentityType,
            DefaultNamespace = settings.DefaultNamespace,
            BaseClassNamespace = settings.BaseClassNamespace,
            DbContextClass = settings.DbContextClass
        };
        DataContext = Settings;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}