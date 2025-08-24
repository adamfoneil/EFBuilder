using ModelBuilder;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace EFBuilder.WPF;

/// <summary>
/// Dialog for editing code generation settings
/// </summary>
public partial class SettingsDialog : Window
{
    public LocalSettings Settings { get; private set; }
    
    public SettingsDialog(LocalSettings settings)
    {
        InitializeComponent();
        
        // Create a copy so we don't modify the original unless OK is clicked
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
        // Validate required fields
        if (string.IsNullOrWhiteSpace(Settings.IdentityType))
        {
            MessageBox.Show("Identity Type is required.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            IdentityTypeTextBox.Focus();
            return;
        }
        
        if (string.IsNullOrWhiteSpace(Settings.DefaultNamespace))
        {
            MessageBox.Show("Default Namespace is required.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            DefaultNamespaceTextBox.Focus();
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
    
    /// <summary>
    /// Load settings from settings.json file in the specified directory
    /// </summary>
    public static LocalSettings LoadSettings(string directoryPath)
    {
        var settingsFile = Path.Combine(directoryPath, "settings.json");
        
        if (File.Exists(settingsFile))
        {
            try
            {
                var json = File.ReadAllText(settingsFile);
                var settings = JsonSerializer.Deserialize<LocalSettings>(json);
                return settings ?? new LocalSettings();
            }
            catch (Exception ex)
            {
                // Log error but continue with defaults
                Console.WriteLine($"Warning: Could not load settings from {settingsFile}: {ex.Message}");
            }
        }
        
        return new LocalSettings();
    }
    
    /// <summary>
    /// Save settings to settings.json file in the specified directory
    /// </summary>
    public static void SaveSettings(string directoryPath, LocalSettings settings)
    {
        try
        {
            var settingsFile = Path.Combine(directoryPath, "settings.json");
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(settingsFile, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not save settings: {ex.Message}", ex);
        }
    }
}