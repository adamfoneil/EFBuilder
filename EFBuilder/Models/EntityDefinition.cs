using System.Collections.Generic;

namespace EFBuilder.Models;

public class EntityDefinition
{
    public string Name { get; set; } = string.Empty;
    public string BaseClass { get; set; } = string.Empty;
    public List<PropertyDefinition> Properties { get; set; } = new();
    public List<string> NavigationProperties { get; set; } = new();
}

public class PropertyDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsForeignKey { get; set; }
}