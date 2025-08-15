using EFBuilder;

var service = new EFBuilderService();

if (args.Length == 0)
{
    Console.WriteLine("Usage: EFBuilder <input-file> [output-directory]");
    Console.WriteLine("  input-file: Path to the input file containing entity definitions");
    Console.WriteLine("  output-directory: Optional directory to write generated files (default: current directory)");
    return;
}

var inputFile = args[0];
var outputDirectory = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();

if (!File.Exists(inputFile))
{
    Console.WriteLine($"Error: Input file '{inputFile}' not found.");
    return;
}

try
{
    var input = File.ReadAllText(inputFile);
    var namespaceName = "Generated"; // Default namespace
    
    var generatedFiles = service.GenerateEntitiesFromInput(input, namespaceName);
    
    // Ensure output directory exists
    Directory.CreateDirectory(outputDirectory);
    
    foreach (var kvp in generatedFiles)
    {
        var outputPath = Path.Combine(outputDirectory, kvp.Key);
        File.WriteAllText(outputPath, kvp.Value);
        Console.WriteLine($"Generated: {outputPath}");
    }
    
    Console.WriteLine($"\nSuccessfully generated {generatedFiles.Count} entity files.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
