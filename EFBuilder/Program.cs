using EFBuilder;

var service = new EFBuilderService();

// Parse command line arguments
var outputDirectory = string.Empty;
var inputFiles = new List<string>();

for (int i = 0; i < args.Length; i++)
{
    var arg = args[i];
    
    if (arg == "--output" || arg == "-o")
    {
        if (i + 1 < args.Length)
        {
            outputDirectory = args[i + 1];
            i++; // Skip the next argument since it's the value for this flag
        }
        else
        {
            Console.WriteLine("Error: --output/-o flag requires a directory path.");
            return;
        }
    }
    else if (arg == "--help" || arg == "-h")
    {
        ShowUsage();
        return;
    }
    else if (!arg.StartsWith("-"))
    {
        inputFiles.Add(arg);
    }
    else
    {
        Console.WriteLine($"Error: Unknown flag '{arg}'.");
        ShowUsage();
        return;
    }
}

// If no input files specified, auto-discover in current directory
if (inputFiles.Count == 0)
{
    var currentDir = Directory.GetCurrentDirectory();
    var discoveredFiles = Directory.GetFiles(currentDir, "*.txt")
        .Concat(Directory.GetFiles(currentDir, "*.md"))
        .ToList();
        
    if (discoveredFiles.Count == 0)
    {
        Console.WriteLine("No .txt or .md files found in the current directory.");
        Console.WriteLine("Please specify input files or create entity definition files with .txt or .md extensions.");
        return;
    }
    
    inputFiles = discoveredFiles;
    Console.WriteLine($"Auto-discovered {inputFiles.Count} file(s): {string.Join(", ", inputFiles.Select(Path.GetFileName))}");
}

// Set default output directory to parent directory if not specified
if (string.IsNullOrEmpty(outputDirectory))
{
    outputDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
}

try
{
    var allGeneratedFiles = new Dictionary<string, string>();
    
    foreach (var inputFile in inputFiles)
    {
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: Input file '{inputFile}' not found.");
            continue;
        }
        
        Console.WriteLine($"Processing: {Path.GetFileName(inputFile)}");
        
        var input = File.ReadAllText(inputFile);
        
        var generatedFiles = service.GenerateEntitiesFromInput(input);
        
        // Merge generated files (later files override earlier ones if same name)
        foreach (var kvp in generatedFiles)
        {
            allGeneratedFiles[kvp.Key] = kvp.Value;
        }
    }
    
    if (allGeneratedFiles.Count == 0)
    {
        Console.WriteLine("No entity files were generated.");
        return;
    }
    
    // Ensure output directory exists
    Directory.CreateDirectory(outputDirectory);
    
    foreach (var kvp in allGeneratedFiles)
    {
        var outputPath = Path.Combine(outputDirectory, kvp.Key);
        
        // Check if file already exists
        if (File.Exists(outputPath))
        {
            Console.WriteLine($"Warning: File '{outputPath}' already exists, skipping to avoid overwrite.");
            continue;
        }
        
        File.WriteAllText(outputPath, kvp.Value);
        Console.WriteLine($"Generated: {outputPath}");
    }
    
    Console.WriteLine($"\nSuccessfully generated {allGeneratedFiles.Count} entity files in '{outputDirectory}'.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

static void ShowUsage()
{
    Console.WriteLine("EFBuilder.CLI - Generate Entity Framework Core entities from markdown-style syntax");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet efb [input-file...] [--output|-o <directory>]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  input-file           Path to input file(s) containing entity definitions");
    Console.WriteLine("                      If not specified, auto-discovers .txt and .md files in current directory");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --output, -o <dir>  Directory to write generated files (default: parent directory)");
    Console.WriteLine("  --help, -h          Show this help message");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet efb                           # Auto-discover files, output to parent directory");
    Console.WriteLine("  dotnet efb entities.txt             # Process specific file, output to parent directory");
    Console.WriteLine("  dotnet efb -o ./Models              # Auto-discover files, output to ./Models");
    Console.WriteLine("  dotnet efb entities.txt -o ./src    # Process specific file, output to ./src");
}
