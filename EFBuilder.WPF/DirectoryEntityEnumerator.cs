using ModelBuilder;
using System.IO;

namespace EFBuilder.WPF;

/// <summary>
/// Entity enumerator that reads markdown files from a directory
/// </summary>
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
                // Skip files that can't be read
                Console.WriteLine($"Warning: Could not read file {filePath}: {ex.Message}");
            }
        }

        return [.. results];
    }
}