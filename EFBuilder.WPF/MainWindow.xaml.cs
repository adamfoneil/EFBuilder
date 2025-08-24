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
    private AppSettings _settings;
    private LocalSettings _localSettings;
    private ObservableCollection<string> _childCollections = new();

    public string? SelectedDirectory
    {
        get => _selectedDirectory;
        set
        {
            _selectedDirectory = value;
            OnPropertyChanged();
            LoadEntitiesFromDirectory();
            
            // Load local settings for this directory
            if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
            {
                _localSettings = SettingsDialog.LoadSettings(value);
            }
            else
            {
                _localSettings = new LocalSettings();
            }
            
            // Save the selected directory to settings
            if (_settings != null)
            {
                _settings.LastSelectedDirectory = value;
                _settings.Save();
            }
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

    public ObservableCollection<string> ChildCollections
    {
        get => _childCollections;
        set
        {
            _childCollections = value;
            OnPropertyChanged();
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        
        // Load settings and restore last directory
        _settings = AppSettings.Load();
        _localSettings = new LocalSettings(); // Initialize with defaults
        
        if (!string.IsNullOrEmpty(_settings.LastSelectedDirectory) && 
            Directory.Exists(_settings.LastSelectedDirectory))
        {
            SelectedDirectory = _settings.LastSelectedDirectory;
        }
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
        
        // First, load all entities without child collection counts
        foreach (var file in markdownFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            Entities.Add(new EntityFileItem
            {
                Name = fileName,
                FilePath = file,
                ChildCollectionCount = ""
            });
        }
        
        // Then calculate child collection counts
        UpdateChildCollectionCounts();
    }
    
    private void UpdateChildCollectionCounts()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedDirectory))
                return;
                
            var directoryEnumerator = new DirectoryEntityEnumerator(SelectedDirectory);
            var parser = new EntityParser(directoryEnumerator);
            var (allDefinitions, errors) = parser.ParseEntities();
            
            if (errors.Length == 0 && allDefinitions.Length > 0)
            {
                var childCollections = CodeGenerator.GetChildCollections(allDefinitions);
                
                foreach (var entity in Entities)
                {
                    if (childCollections.TryGetValue(entity.Name, out var children))
                    {
                        entity.ChildCollectionCount = children.Length > 0 ? $"({children.Length})" : "";
                    }
                    else
                    {
                        entity.ChildCollectionCount = "";
                    }
                }
            }
        }
        catch
        {
            // If there are parsing errors, just leave counts empty
            foreach (var entity in Entities)
            {
                entity.ChildCollectionCount = "";
            }
        }
    }

    private void LoadEntityContent()
    {
        if (SelectedEntity == null || !File.Exists(SelectedEntity.FilePath))
        {
            MarkdownContent = "";
            CSharpContent = "";
            ChildCollections.Clear();
            return;
        }

        var content = File.ReadAllText(SelectedEntity.FilePath);
        
        // Update markdown content without triggering the setter's generation
        _markdownContent = content;
        OnPropertyChanged(nameof(MarkdownContent));
        
        // Generate C# content 
        GenerateCSharpContent();
        
        // Update child collections
        UpdateCurrentEntityChildCollections();
    }
    
    private void UpdateCurrentEntityChildCollections()
    {
        ChildCollections.Clear();
        
        try
        {
            if (SelectedEntity == null || string.IsNullOrEmpty(SelectedDirectory))
                return;
                
            var directoryEnumerator = new DirectoryEntityEnumerator(SelectedDirectory);
            var parser = new EntityParser(directoryEnumerator);
            var (allDefinitions, errors) = parser.ParseEntities();
            
            if (errors.Length == 0 && allDefinitions.Length > 0)
            {
                var childCollections = CodeGenerator.GetChildCollections(allDefinitions);
                
                if (childCollections.TryGetValue(SelectedEntity.Name, out var children))
                {
                    foreach (var child in children)
                    {
                        ChildCollections.Add(child);
                    }
                }
            }
        }
        catch
        {
            // If there are parsing errors, just leave child collections empty
        }
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
                IdentityType = _localSettings.IdentityType,
                DefaultNamespace = _localSettings.DefaultNamespace,
                BaseClassNamespace = _localSettings.BaseClassNamespace
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

    private void AddEntityButton_Click(object sender, RoutedEventArgs e)
    {
        // Save current entity if any
        SaveCurrentEntity();
        
        // Clear the selected entity and content to start fresh
        SelectedEntity = null;
        MarkdownContent = "";
        
        // Focus on the markdown editor
        MarkdownTextBox.Focus();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(MarkdownContent))
            {
                MessageBox.Show("Cannot save: No content in the editor.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Extract entity name from the first line
            var lines = MarkdownContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                MessageBox.Show("Cannot save: No content to determine entity name.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var firstLine = lines[0].Trim();
            if (string.IsNullOrWhiteSpace(firstLine))
            {
                MessageBox.Show("Cannot save: First line is empty. Cannot determine entity name.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Extract entity name (everything before ':' if present, otherwise the whole line)
            var entityName = firstLine.Contains(':') ? firstLine.Split(':')[0].Trim() : firstLine.Trim();
            
            if (string.IsNullOrWhiteSpace(entityName))
            {
                MessageBox.Show("Cannot save: Could not determine entity name from first line.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ensure we have a selected directory
            if (string.IsNullOrEmpty(SelectedDirectory))
            {
                MessageBox.Show("Cannot save: No directory selected. Please select a directory first.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create the file path
            var fileName = $"{entityName}.md";
            var filePath = Path.Combine(SelectedDirectory, fileName);

            // Save the file
            File.WriteAllText(filePath, MarkdownContent);

            // Refresh the entities list and select the new/updated entity
            LoadEntitiesFromDirectory();
            var entityItem = Entities.FirstOrDefault(e => string.Equals(e.Name, entityName, StringComparison.OrdinalIgnoreCase));
            if (entityItem != null)
            {
                SelectedEntity = entityItem;
            }

            MessageBox.Show($"Entity '{entityName}' saved successfully!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving entity: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ScaffoldButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(SelectedDirectory))
        {
            MessageBox.Show("Please select a directory first before scaffolding.", "No Directory Selected", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var dialog = new ScaffoldingDialog
            {
                Owner = this,
                TargetDirectory = SelectedDirectory
            };
            
            var result = dialog.ShowDialog();
            
            if (dialog.ScaffoldingCompleted)
            {
                // Refresh the entities list to show the newly scaffolded files
                LoadEntitiesFromDirectory();
                
                MessageBox.Show("Scaffolding completed! The entity list has been refreshed.", 
                    "Scaffolding Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening scaffolding dialog: {ex.Message}", "Scaffolding Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(SelectedDirectory))
        {
            MessageBox.Show("Please select a directory first before configuring settings.", "No Directory Selected", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var dialog = new SettingsDialog(_localSettings);
            if (dialog.ShowDialog() == true)
            {
                // Update local settings with the edited values
                _localSettings = dialog.Settings;
                
                // Save settings to file
                SettingsDialog.SaveSettings(SelectedDirectory, _localSettings);
                
                // Regenerate C# content with new settings
                GenerateCSharpContent();
                
                MessageBox.Show("Settings saved successfully!", "Settings Saved", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Settings Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void ChildCollectionsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox?.SelectedItem is string childEntityName)
        {
            // Find the entity with this name in the entities list
            var entityToSelect = Entities.FirstOrDefault(entity => 
                string.Equals(entity.Name, childEntityName, StringComparison.OrdinalIgnoreCase));
                
            if (entityToSelect != null)
            {
                SelectedEntity = entityToSelect;
            }
        }
    }
}

public class EntityFileItem : INotifyPropertyChanged
{
    private string _name = "";
    private string _filePath = "";
    private string _childCollectionCount = "";

    public string Name 
    { 
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }
    
    public string FilePath 
    { 
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged();
        }
    }
    
    public string ChildCollectionCount 
    { 
        get => _childCollectionCount;
        set
        {
            _childCollectionCount = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}