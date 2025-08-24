using ModelBuilder;
using System.IO;
using System.Windows;

namespace EFBuilder.WPF;

/// <summary>
/// Dialog for scaffolding entity definitions from a database connection
/// </summary>
public partial class ScaffoldingDialog : Window
{
    public string? TargetDirectory { get; set; }
    public bool ScaffoldingCompleted { get; private set; }

    public ScaffoldingDialog()
    {
        InitializeComponent();
        StatusTextBlock.Text = "Enter your SQL Server connection string and click OK to generate entity definitions.";
    }

    private async void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var connectionString = ConnectionStringTextBox.Text?.Trim();
        
        if (string.IsNullOrEmpty(connectionString))
        {
            MessageBox.Show("Please enter a connection string.", "Connection String Required", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            ConnectionStringTextBox.Focus();
            return;
        }

        if (string.IsNullOrEmpty(TargetDirectory))
        {
            MessageBox.Show("No target directory specified.", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        await PerformScaffoldingAsync(connectionString);
    }

    private async Task PerformScaffoldingAsync(string connectionString)
    {
        try
        {
            // Disable UI during scaffolding
            OkButton.IsEnabled = false;
            CancelButton.Content = "Close";
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            
            StatusTextBlock.Text = "Connecting to database...";
            
            var scaffolder = new SqlServerScaffolder();
            
            StatusTextBlock.Text = "Discovering tables and generating entity definitions...";
            
            var entities = await scaffolder.ScaffoldEntitiesAsync(connectionString);
            
            StatusTextBlock.Text = $"Found {entities.Length} tables. Writing entity files...";
            
            // Ensure target directory exists
            if (!Directory.Exists(TargetDirectory!))
            {
                Directory.CreateDirectory(TargetDirectory!);
            }
            
            var filesWritten = 0;
            foreach (var (fileName, content) in entities)
            {
                var filePath = Path.Combine(TargetDirectory!, fileName);
                await File.WriteAllTextAsync(filePath, content);
                filesWritten++;
                
                StatusTextBlock.Text = $"Written {filesWritten}/{entities.Length} files...";
            }
            
            ScaffoldingCompleted = true;
            StatusTextBlock.Text = $"Scaffolding completed successfully!\n\n" +
                                 $"Generated {entities.Length} entity definition files in:\n{TargetDirectory}\n\n" +
                                 $"Files created:\n" + 
                                 string.Join("\n", entities.Select(e => $"• {e.FileName}"));
            
            MessageBox.Show($"Scaffolding completed! Generated {entities.Length} entity files.", 
                "Scaffolding Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Error during scaffolding:\n\n{ex.Message}\n\n" +
                                 "Please check your connection string and try again.";
            
            MessageBox.Show($"Scaffolding failed:\n\n{ex.Message}", "Scaffolding Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            OkButton.IsEnabled = true;
            CancelButton.Content = ScaffoldingCompleted ? "Close" : "Cancel";
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = ScaffoldingCompleted;
        Close();
    }
}