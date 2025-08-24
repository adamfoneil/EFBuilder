using ModelBuilder;
using System.IO;

// Test the DirectoryEntityEnumerator and code generation
var testDir = "/tmp/test-entities";
Console.WriteLine($"Testing directory: {testDir}");

var enumerator = new DirectoryEntityEnumerator(testDir);
var parser = new EntityParser(enumerator);
var (definitions, errors) = parser.ParseEntities();

Console.WriteLine($"Found {definitions.Length} entities with {errors.Length} errors");

if (errors.Length > 0)
{
    Console.WriteLine("\nErrors:");
    foreach (var error in errors)
    {
        Console.WriteLine($"  {error}");
    }
}

if (definitions.Length > 0)
{
    Console.WriteLine("\nEntities:");
    foreach (var entity in definitions)
    {
        Console.WriteLine($"  {entity.Name} (Base: {entity.BaseClass ?? "None"}, Properties: {entity.Properties.Length})");
    }

    // Test code generation for the first entity
    var settings = new CodeGenerator.Settings
    {
        DefaultNamespace = "Generated",
        BaseClassNamespace = "Generated.Conventions"
    };

    var firstEntity = definitions[0];
    var code = CodeGenerator.Execute(settings, firstEntity, definitions);
    
    Console.WriteLine($"\n=== Generated C# for {firstEntity.Name} ===");
    Console.WriteLine(code);
    Console.WriteLine("=== End Generated C# ===");
}

Console.WriteLine("\nDirectoryEntityEnumerator and ModelBuilder integration test completed successfully!");

// Simple DirectoryEntityEnumerator implementation for testing
public class DirectoryEntityEnumerator : IEntityEnumerator
{
    private readonly string _directoryPath;

    public DirectoryEntityEnumerator(string directoryPath)
    {
        _directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
    }

    public (string Name, string Content)[] GetContent()
    {
        if (!Directory.Exists(_directoryPath))
            return [];

        var markdownFiles = Directory.GetFiles(_directoryPath, "*.md");
        var results = new List<(string Name, string Content)>();

        foreach (var filePath in markdownFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var content = File.ReadAllText(filePath);
                results.Add((fileName, content));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not read file {filePath}: {ex.Message}");
            }
        }

        return [.. results];
    }
}