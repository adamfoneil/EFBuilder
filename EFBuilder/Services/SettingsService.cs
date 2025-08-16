using EFBuilder.Models;
using System.Text.Json;

namespace EFBuilder.Services;

public class SettingsService
{
    private const string SettingsFileName = "efbuilder.settings.json";
    
    public EFBuilderSettings LoadSettings(string? workingDirectory = null)
    {
        var settingsPath = GetSettingsPath(workingDirectory);
        
        if (File.Exists(settingsPath))
        {
            try
            {
                var json = File.ReadAllText(settingsPath);
                var settings = JsonSerializer.Deserialize<EFBuilderSettings>(json);
                return settings ?? new EFBuilderSettings();
            }
            catch
            {
                // If there's an error reading the settings, return defaults
                return new EFBuilderSettings();
            }
        }
        
        return new EFBuilderSettings();
    }
    
    public void SaveSettings(EFBuilderSettings settings, string? workingDirectory = null)
    {
        var settingsPath = GetSettingsPath(workingDirectory);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(settingsPath, json);
    }
    
    private string GetSettingsPath(string? workingDirectory)
    {
        var directory = workingDirectory ?? Directory.GetCurrentDirectory();
        return Path.Combine(directory, SettingsFileName);
    }
}