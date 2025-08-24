using ModelBuilder;

var testEntity = @"TestEntity : BaseTable
#Name string(100)
#AutoId int++
IsActive bool = true";

var enumerator = new SimpleFileEnumerator([("TestEntity.md", testEntity)]);
var (definitions, errors) = new EntityParser(enumerator).ParseEntities();

Console.WriteLine($"Parsed {definitions.Length} entities with {errors.Length} errors");
if (errors.Length > 0) { 
    foreach(var error in errors) Console.WriteLine($"Error: {error}");
    return;
}

var entity = definitions[0];
foreach (var prop in entity.Properties)
{
    Console.WriteLine($"Property: {prop.Name}, Type: {prop.ClrType}, IsAutoIncrement: {prop.IsAutoIncrement}, IsUnique: {prop.IsUnique}");
}

var settings = new CodeGenerator.Settings() { DefaultNamespace = "TestNamespace" };
var files = CodeGenerator.Execute(settings, definitions);

Console.WriteLine($"Generated {files.Length} files");
foreach (var file in files) {
    Console.WriteLine($"File: {file.Filename}");
    Console.WriteLine("Generated Content:");
    Console.WriteLine(file.Content);
}

public class SimpleFileEnumerator(IEnumerable<(string Name, string Content)> files) : IEntityEnumerator
{
    public (string Name, string Content)[] GetContent() => [.. files];
}