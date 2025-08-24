using System.IO;
using System.Text.Json;

namespace EFBuilder.WPF;

/// <summary>
/// Application settings that persist between sessions
/// </summary>
public class AppSettings
{
    public string? LastSelectedDirectory { get; set; }

    private static readonly string SettingsDirectory = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EFBuilder.WPF");
    
    private static readonly string SettingsFile = Path.Combine(SettingsDirectory, "settings.json");

    /// <summary>
    /// Load settings from disk, or return defaults if file doesn't exist
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            // Log error but continue with defaults
            Console.WriteLine($"Warning: Could not load settings: {ex.Message}");
        }

        return new AppSettings();
    }

    /// <summary>
    /// Save settings to disk
    /// </summary>
    public void Save()
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(SettingsDirectory);
            
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(SettingsFile, json);
        }
        catch (Exception ex)
        {
            // Log error but don't crash the app
            Console.WriteLine($"Warning: Could not save settings: {ex.Message}");
        }
    }
}