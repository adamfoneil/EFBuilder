using EFBuilder.Models;
using System.Text.RegularExpressions;

namespace EFBuilder.Services;

public class EntityParser
{
    public static List<EntityDefinition> ParseInput(string input)
    {
        var entities = new List<EntityDefinition>();
        var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                         .Select(line => line.Trim())
                         .Where(line => !string.IsNullOrEmpty(line))
                         .ToArray();

        EntityDefinition? currentEntity = null;

        foreach (var line in lines)
        {
            if (currentEntity == null || IsEntityHeader(line))
            {
                // Parse entity definition: EntityName : BaseClass
                currentEntity = ParseEntityHeader(line);
                if (currentEntity != null)
                {
                    entities.Add(currentEntity);
                }
            }
            else if (currentEntity != null)
            {
                // Parse property definition
                var property = ParseProperty(line);
                if (property != null)
                {
                    currentEntity.Properties.Add(property);
                }
            }
        }

		// Detect foreign key relationships and add navigation properties
		AddNavigationProperties(entities);

        return entities;
    }

    private static bool IsEntityHeader(string line) =>
		line.Contains(':') && (line.StartsWith("#") || Regex.IsMatch(line, @"^\w+\s*:\s*\w+"));

	private static EntityDefinition? ParseEntityHeader(string line)
    {
        // Parse: EntityName : BaseClass (with or without # prefix)
        var cleanLine = line.StartsWith("#") ? line.Substring(1).Trim() : line;
        var match = Regex.Match(cleanLine, @"^(\w+)\s*:\s*(\w+)$");
        if (match.Success)
        {
            return new EntityDefinition
            {
                Name = match.Groups[1].Value,
                BaseClass = match.Groups[2].Value
            };
        }
        return null;
    }

    private static PropertyDefinition? ParseProperty(string line)
    {
        // Parse various property formats:
        // PropertyName type(length)?
        // PropertyName type(length)
        // PropertyName type = defaultValue
        // PropertyName type
        // PropertyName (for foreign keys ending in "Id")
        // #PropertyName type (for unique index properties)

        var property = new PropertyDefinition();
        
        // Check if this property should be part of a unique index
        if (line.StartsWith("#"))
        {
            property.IsUniqueIndex = true;
            line = line[1..].Trim();
        }

        // Check for default value assignment
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            property.DefaultValue = parts[1].Trim();
            line = parts[0].Trim();
        }

        // Check for nullable indicator
        if (line.EndsWith("?"))
        {
            property.IsNullable = true;
            line = line.TrimEnd('?').Trim();
        }

        // Check if this is just a property name (foreign key)
        if (Regex.IsMatch(line, @"^\w+$"))
        {
            property.Name = line.Trim();
            
            // Check if this is a foreign key (ends with "Id")
            if (property.Name.EndsWith("Id") && property.Name != "Id")
            {
                property.IsForeignKey = true;
                property.Type = "int"; // Foreign keys are always int
                return property;
            }
            
            // If it's not a foreign key and has no type, it's invalid
            return null;
        }

        // Parse property name and type with optional length
        var match = Regex.Match(line, @"^(\w+)\s+(\w+)(?:\((\d+)\))?$");
        if (match.Success)
        {
            property.Name = match.Groups[1].Value;
            var typeName = match.Groups[2].Value;
            
            // Convert type names to C# types
            property.Type = ConvertTypeName(typeName);
            
            if (match.Groups[3].Success && int.TryParse(match.Groups[3].Value, out var length))
            {
                property.MaxLength = length;
            }

            return property;
        }

        return null;
    }

    private static string ConvertTypeName(string typeName) =>
		typeName.ToLower() switch
		{
			"string" => "string",
			"int" => "int",
			"decimal" => "decimal",
			"bool" => "bool",
			"datetime" => "DateTime",
			_ => typeName
		};

	private static void AddNavigationProperties(List<EntityDefinition> entities)
    {
        foreach (var entity in entities)
        {
            foreach (var property in entity.Properties.Where(p => p.IsForeignKey))
            {
                // Find the related entity (remove "Id" suffix)
                var relatedEntityName = property.Name.Substring(0, property.Name.Length - 2);
                var relatedEntity = entities.FirstOrDefault(e => e.Name.Equals(relatedEntityName, StringComparison.OrdinalIgnoreCase));
                
                if (relatedEntity != null)
                {
                    // Add navigation property to current entity
                    entity.NavigationProperties.Add($"public {relatedEntityName}? {relatedEntityName} {{ get; set; }}");
                    
                    // Add collection navigation property to related entity
                    var collectionProperty = $"public ICollection<{entity.Name}> {entity.Name}s {{ get; set; }} = [];";
                    if (!relatedEntity.NavigationProperties.Contains(collectionProperty))
                    {
                        relatedEntity.NavigationProperties.Add(collectionProperty);
                    }
                }
            }
        }
    }
}