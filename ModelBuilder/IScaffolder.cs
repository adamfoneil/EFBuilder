using System.Data.Common;

namespace ModelBuilder;

/// <summary>
/// Interface for scaffolding entity definitions from database schemas
/// </summary>
public interface IScaffolder
{
    /// <summary>
    /// Generate entity markdown definitions from a database connection
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>Array of entity files with name and markdown content</returns>
    Task<(string FileName, string Content)[]> ScaffoldEntitiesAsync(string connectionString);

    /// <summary>
    /// Get the mapping from database types to CLR types for this provider
    /// </summary>
    /// <returns>Dictionary mapping database type names to CLR type names</returns>
    Dictionary<string, string> GetTypeMapping();
}