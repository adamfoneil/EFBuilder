using Microsoft.Win32;
using ModelBuilder;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace EFBuilder.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string? _selectedDirectory;
    private EntityFileItem? _selectedEntity;
    private string _markdownContent = "";
    private string _csharpContent = "";

    public string? SelectedDirectory
    {
        get => _selectedDirectory;
        set
        {
            _selectedDirectory = value;
            OnPropertyChanged();
            LoadEntitiesFromDirectory();
        }
    }

    public EntityFileItem? SelectedEntity
    {
        get => _selectedEntity;
        set
        {
            _selectedEntity = value;
            OnPropertyChanged();
            LoadEntityContent();
        }
    }

    public string MarkdownContent
    {
        get => _markdownContent;
        set
        {
            _markdownContent = value;
            OnPropertyChanged();
            GenerateCSharpContent();
        }
    }

    public string CSharpContent
    {
        get => _csharpContent;
        set
        {
            _csharpContent = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<EntityFileItem> Entities { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Directory Containing Entity Markdown Files"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedDirectory = dialog.FolderName;
        }
    }

    private void LoadEntitiesFromDirectory()
    {
        Entities.Clear();
        
        if (string.IsNullOrEmpty(SelectedDirectory) || !Directory.Exists(SelectedDirectory))
            return;

        var markdownFiles = Directory.GetFiles(SelectedDirectory, "*.md");
        
        foreach (var file in markdownFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            Entities.Add(new EntityFileItem
            {
                Name = fileName,
                FilePath = file
            });
        }
    }

    private void LoadEntityContent()
    {
        if (SelectedEntity == null || !File.Exists(SelectedEntity.FilePath))
        {
            MarkdownContent = "";
            CSharpContent = "";
            return;
        }

        var content = File.ReadAllText(SelectedEntity.FilePath);
        
        // Update markdown content without triggering the setter's generation
        _markdownContent = content;
        OnPropertyChanged(nameof(MarkdownContent));
        
        // Generate C# content 
        GenerateCSharpContent();
    }

    private void GenerateCSharpContent()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(MarkdownContent) || SelectedEntity == null)
            {
                CSharpContent = "";
                return;
            }

            // Create a directory enumerator to parse all entities for proper reference resolution
            if (string.IsNullOrEmpty(SelectedDirectory))
            {
                CSharpContent = "// No directory selected";
                return;
            }

            var directoryEnumerator = new DirectoryEntityEnumerator(SelectedDirectory);
            var parser = new EntityParser(directoryEnumerator);
            var (allDefinitions, errors) = parser.ParseEntities();

            if (errors.Length > 0)
            {
                CSharpContent = $"// Parsing errors:\n{string.Join("\n", errors.Select(e => $"// {e}"))}";
                return;
            }

            if (allDefinitions.Length == 0)
            {
                CSharpContent = "// No entities found";
                return;
            }

            // Find the current entity definition
            var currentEntity = allDefinitions.FirstOrDefault(e => 
                string.Equals(e.Name, SelectedEntity.Name, StringComparison.OrdinalIgnoreCase));

            if (currentEntity == null)
            {
                CSharpContent = $"// Entity '{SelectedEntity.Name}' not found in parsed definitions";
                return;
            }

            var settings = new CodeGenerator.Settings
            {
                DefaultNamespace = "Generated",
                BaseClassNamespace = "Generated.Conventions"
            };

            CSharpContent = CodeGenerator.Execute(settings, currentEntity, allDefinitions);
        }
        catch (Exception ex)
        {
            CSharpContent = $"// Error generating C#:\n// {ex.Message}";
        }
    }

    private void MarkdownTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        SaveCurrentEntity();
    }

    private void SaveCurrentEntity()
    {
        if (SelectedEntity != null && File.Exists(SelectedEntity.FilePath))
        {
            try
            {
                File.WriteAllText(SelectedEntity.FilePath, MarkdownContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(CSharpContent);
            MessageBox.Show("C# code copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public class EntityFileItem
{
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
}