namespace ModelBuilder;

public class EntityParser(IEntityEnumerator contentAccessor)
{
	private readonly IEntityEnumerator _contentAccessor = contentAccessor;

	private static string[] _clrTypes => 
	[	
		"int", "long", "string", "DateTime", "bool", "decimal",
		"float", "double", "Guid", "byte[]", "char", "DateOnly", "TimeOnly"
	];

	public (EntityDefinition[] EntityDefinitions, string[] Errors) ParseEntities()
	{
		var entitySources = _contentAccessor.GetContent();
		var entities = new List<EntityDefinition>();
		var entityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		List<string> errors = [];

		foreach (var (name, source) in entitySources)
		{
			try
			{				
				var entity = ParseEntity(source, entityNames);
				entities.Add(entity);
				entityNames.Add(entity.Name);  // Add to known entity names
			}
			catch (Exception ex)
			{
				errors.Add($"{name}: {ex.Message}");
			}
		}

		return ([.. entities], [..errors]);
	}

	private static EntityDefinition ParseEntity(string source, HashSet<string> entityNames)
	{
		var lines = source.Split('\n', StringSplitOptions.RemoveEmptyEntries)
						.Select(l => l.Trim())
						.Where(l => !string.IsNullOrEmpty(l))
						.ToArray();

		if (lines.Length == 0)
			throw new Exception("No entity definition found");

		// Parse header: EntityName[: BaseClass]
		var headerLine = lines[0];
		var headerMatch = System.Text.RegularExpressions.Regex.Match(headerLine, @"^(\w+)(?:\s*:\s*(\w+))?$");
		if (!headerMatch.Success)
			throw new Exception($"Invalid entity header: {headerLine}");

		var entity = new EntityDefinition
		{
			Name = headerMatch.Groups[1].Value,
			BaseClass = headerMatch.Groups[2].Success ? headerMatch.Groups[2].Value : null,
			Properties = []
		};

		var properties = new List<PropertyDefinition>();

		for (int i = 1; i < lines.Length; i++)
		{
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			var prop = new PropertyDefinition();

			// Unique index
			if (line.StartsWith("#"))
			{
				prop.IsUnique = true;
				line = line[1..].Trim();
			}

			// Default value
			var parts = line.Split('=', 2);
			if (parts.Length == 2)
			{
				prop.DefaultValue = parts[1].Trim();
				line = parts[0].Trim();
			}

			// Auto-increment
			if (line.EndsWith("++"))
			{
				prop.IsAutoIncrement = true;
				line = line.TrimEnd('+').Trim();
			}

			// Nullable
			if (line.EndsWith("?"))
			{
				prop.IsNullable = true;
				line = line.TrimEnd('?').Trim();
			}

			// Foreign key: PropertyNameId
			if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\w+$"))
			{
				prop.Name = line;
				if (prop.Name.EndsWith("Id") && prop.Name != "Id")
				{
					prop.ClrType = "int";
					prop.ReferencedEntity = prop.Name.Substring(0, prop.Name.Length - 2);
					// Set child collection to current entity name (use simple pluralization)
					prop.ChildCollection = entity.Name.EndsWith("s") ? entity.Name : entity.Name + "s";
				}
				else
				{
					prop.ClrType = "string";
				}
				properties.Add(prop);
				continue;
			}

			// PropertyName EntityName.ColumnName <CollectionName (custom column reference)
			var customRefMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(\w+)\s+(\w+)\.(\w+)\s*<(\w+)$", System.Text.RegularExpressions.RegexOptions.None);
			if (customRefMatch.Success)
			{
				prop.Name = customRefMatch.Groups[1].Value;
				prop.ReferencedEntity = customRefMatch.Groups[2].Value;
				prop.ReferencedColumn = customRefMatch.Groups[3].Value;
				prop.ChildCollection = customRefMatch.Groups[4].Value;
				prop.ClrType = "int"; // Default to int for foreign keys, could be enhanced later
				properties.Add(prop);
				continue;
			}

			// PropertyName EntityName <CollectionName (standard entity reference)
			var entityRefMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(\w+)\s+(\w+)\s*<(\w+)$", System.Text.RegularExpressions.RegexOptions.None);
			if (entityRefMatch.Success)
			{
				prop.Name = entityRefMatch.Groups[1].Value;
				prop.ReferencedEntity = entityRefMatch.Groups[2].Value;
				prop.ChildCollection = entityRefMatch.Groups[3].Value;
				prop.ClrType = "int"; // Default to int for foreign keys
				properties.Add(prop);
				continue;
			}

			// PropertyId <CollectionName (inferred entity reference from property name)
			var inferredRefMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(\w+Id)\s*<(\w+)$", System.Text.RegularExpressions.RegexOptions.None);
			if (inferredRefMatch.Success)
			{
				prop.Name = inferredRefMatch.Groups[1].Value;
				prop.ReferencedEntity = prop.Name.Substring(0, prop.Name.Length - 2); // Remove "Id" suffix
				prop.ChildCollection = inferredRefMatch.Groups[2].Value;
				prop.ClrType = "int"; // Default to int for foreign keys
				properties.Add(prop);
				continue;
			}

			// PropertyName type(length)
			var match = System.Text.RegularExpressions.Regex.Match(line, @"^(\w+)\s+(\w+)(?:\((\d+)\))?$", System.Text.RegularExpressions.RegexOptions.None);
			if (match.Success)
			{
				prop.Name = match.Groups[1].Value;
				var typeName = match.Groups[2].Value;
				prop.ClrType = typeName.ToLower() switch
				{
					"string" => "string",
					"int" => "int",
					"decimal" => "decimal",
					"bool" => "bool",
					"datetime" => "DateTime",
					_ => typeName
				};
				if (match.Groups[3].Success && int.TryParse(match.Groups[3].Value, out var len))
				{
					prop.MaxLength = len;
				}
				properties.Add(prop);
				continue;
			}

			// Invalid property line
			prop.ParseError = true;
			prop.ParseException = $"Could not parse property line: {line}";
			properties.Add(prop);
		}

		entity.Properties = properties.ToArray();
		return entity;
	}
}
