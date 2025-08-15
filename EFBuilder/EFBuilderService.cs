using EFBuilder.Services;

namespace EFBuilder;

public class EFBuilderService
{
    private readonly EntityParser _parser;
    private readonly CodeGenerator _generator;
    
    public EFBuilderService()
    {
        _parser = new EntityParser();
        _generator = new CodeGenerator();
    }
    
    public Dictionary<string, string> GenerateEntitiesFromInput(string input, string namespaceName = "Generated")
    {
        var entities = _parser.ParseInput(input);
        var results = new Dictionary<string, string>();
        
        foreach (var entity in entities)
        {
            var code = _generator.GenerateEntityClass(entity, namespaceName);
            results.Add($"{entity.Name}.cs", code);
        }
        
        return results;
    }
}