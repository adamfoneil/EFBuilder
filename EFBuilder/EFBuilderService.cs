using EFBuilder.Services;

namespace EFBuilder;

public class EFBuilderService
{
    private readonly EntityParser _parser;
    private readonly CodeGenerator _generator;
    private readonly SettingsService _settingsService;
    
    public EFBuilderService()
    {
        _parser = new EntityParser();
        _generator = new CodeGenerator();
        _settingsService = new SettingsService();
    }
    
    public Dictionary<string, string> GenerateEntitiesFromInput(string input, string? namespaceName = null)
    {
        // Load settings and use default namespace if none provided
        var settings = _settingsService.LoadSettings();
        var actualNamespace = namespaceName ?? settings.DefaultNamespace;
        
        var entities = EntityParser.ParseInput(input);
        var results = new Dictionary<string, string>();
        
        foreach (var entity in entities)
        {
            var code = _generator.GenerateEntityClass(entity, actualNamespace, settings);
            var fileName = $"{entity.Name}.cs";
			results.Add(fileName, code);
            Console.WriteLine($"Generated: {fileName}");
		}
        
        return results;
    }
}